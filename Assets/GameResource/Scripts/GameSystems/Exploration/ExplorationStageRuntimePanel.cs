using System;
using System.Threading;
using Backend.GameSystems.Character;
using Backend.GameSystems.DynamicEvent;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Stage;
using Backend.Object.UI;
using Backend.Util;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 탐험 중앙 스테이지 뷰 — 이동·전투·드롭 연출 후 ExplorationStageSystem.CompleteCurrentBeat 호출.
    /// </summary>
    public sealed class ExplorationStageRuntimePanel : ExplorationHudSubview<ExplorationStageRuntimePresenter>
    {
    }

    public sealed class ExplorationStageRuntimePresenter : UIPresenter<ExplorationStageRuntimePanel>
    {
        private const float MoveDuration = 0.65f;
        private const float CombatHitInterval = 0.28f;
        private const float ShortBeatDuration = 0.45f;

        private RectTransform _stageRoot;
        private Image _stageBackground;
        private Image _groundLine;
        private RectTransform _partyActor;
        private RectTransform _monsterActor;
        private Image _partyBody;
        private Image _monsterBody;
        private Image _monsterEliteRing;
        private Image _parallaxBanner;
        private Slider _partyHpBar;
        private Slider _monsterHpBar;
        private TextMeshProUGUI _monsterNameText;
        private RectTransform _floatRoot;
        private TextMeshProUGUI _statusLine;
        private TextMeshProUGUI _segmentLabel;
        private RectTransform _parallaxFar;
        private Image _slashVfx;
        private CanvasGroup _slashCanvasGroup;
        private StageFloatingTextPool _floatPool;
        private CancellationTokenSource _beatCts;
        private CompositeDisposable _disposables;
        private float _partyHomeX;
        private float _monsterHomeX;

        public override void OnOpen()
        {
            EnsureStageBuilt();
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            ExplorationStageSystem.SetDirectorReady(true);
            ExplorationStageSystem.OnBeatStarted
                .Subscribe(PlayBeatAsync)
                .AddTo(_disposables);

            ExplorationChannels.OnStateChanged
                .Subscribe(state =>
                {
                    ApplyZoneTheme(state);
                    ApplyPartyVisual(state?.Party);
                    RefreshSegmentLabel(state);
                    if (state?.IsExploring != true)
                        ResetStageVisual();
                })
                .AddTo(_disposables);

            DynamicEventChannels.OnEventStarted
                .Subscribe(_ =>
                {
                    CancelBeatPlayback(flushBeat: false);
                    ExplorationStageSystem.AbortCurrentBeat();
                    ExplorationStageSystem.SetOverlayHold(true);
                })
                .AddTo(_disposables);

            DynamicEventChannels.OnEventResolved
                .Subscribe(_ => ExplorationStageSystem.SetOverlayHold(false))
                .AddTo(_disposables);

            RefreshSegmentLabel(ExplorationSystem.GetCurrentState());
            ApplyZoneTheme(ExplorationSystem.GetCurrentState());
            ApplyPartyVisual(ExplorationSystem.GetCurrentState()?.Party);
        }

        public override void OnClose()
        {
            CancelBeatPlayback(flushBeat: true);
            ExplorationStageSystem.SetDirectorReady(false);
            _disposables?.Dispose();
            _disposables = null;
        }

        private void EnsureStageBuilt()
        {
            if (_stageRoot != null)
                return;

            var exploreRoot = ResolveExploreContent();
            if (exploreRoot == null)
                return;

            _stageRoot = CreateRect("StageViewport", exploreRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _stageRoot.SetAsFirstSibling();

            var bg = CreateImage("StageBackground", _stageRoot, new Color(0.08f, 0.12f, 0.1f, 0.92f));
            _stageBackground = bg;
            Stretch(bg.rectTransform);

            _parallaxFar = CreateImage("ParallaxFar", _stageRoot, new Color(0.12f, 0.18f, 0.16f, 0.55f)).rectTransform;
            _parallaxBanner = _parallaxFar.GetComponent<Image>();
            _parallaxFar.anchorMin = new Vector2(0f, 0.18f);
            _parallaxFar.anchorMax = new Vector2(1f, 0.92f);
            _parallaxFar.offsetMin = Vector2.zero;
            _parallaxFar.offsetMax = Vector2.zero;
            RuntimeUiSprites.ApplySimpleImage(_parallaxBanner, RuntimeUiSprites.IllustZoneBanner, Color.white);

            _segmentLabel = CreateLabel("SegmentLabel", _stageRoot, 20, TextAnchor.UpperRight, new Vector2(-16f, -12f));
            _segmentLabel.alignment = TextAlignmentOptions.TopRight;

            _slashVfx = CreateImage("SlashVfx", _stageRoot, new Color(1f, 0.95f, 0.55f, 0.85f));
            _slashVfx.rectTransform.sizeDelta = new Vector2(96f, 96f);
            _slashVfx.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _slashCanvasGroup = _slashVfx.gameObject.AddComponent<CanvasGroup>();
            RuntimeStageSprites.ApplyVfx(_slashVfx, StageVisualCatalog.VfxSlash, Color.white);
            _slashVfx.gameObject.SetActive(false);

            var ground = CreateImage("GroundLine", _stageRoot, new Color(0.22f, 0.32f, 0.24f, 1f));
            _groundLine = ground;
            var groundRt = ground.rectTransform;
            groundRt.anchorMin = new Vector2(0f, 0f);
            groundRt.anchorMax = new Vector2(1f, 0f);
            groundRt.pivot = new Vector2(0.5f, 0f);
            groundRt.sizeDelta = new Vector2(0f, 10f);
            groundRt.anchoredPosition = new Vector2(0f, ExplorationHudLayoutMetrics.StageGroundInset);

            _partyActor = CreateActorRoot("PartyActor", _stageRoot, new Color(1f, 1f, 1f, 0f), out _partyBody, out _partyHpBar, true);
            _monsterActor = CreateActorRoot("MonsterActor", _stageRoot, new Color(1f, 1f, 1f, 0f), out _monsterBody, out _monsterHpBar, false);
            _monsterEliteRing = CreateImage("EliteRing", _monsterActor, new Color(1f, 0.82f, 0.35f, 0.55f));
            var ringRt = _monsterEliteRing.rectTransform;
            ringRt.anchorMin = new Vector2(0.5f, 0f);
            ringRt.anchorMax = new Vector2(0.5f, 0f);
            ringRt.pivot = new Vector2(0.5f, 0.5f);
            ringRt.sizeDelta = new Vector2(88f, 88f);
            ringRt.anchoredPosition = new Vector2(0f, 44f);
            _monsterEliteRing.gameObject.SetActive(false);
            _monsterActor.gameObject.SetActive(false);

            _monsterNameText = CreateLabel("MonsterName", _monsterActor, 16, TextAnchor.LowerCenter, new Vector2(0f, 58f));
            _floatRoot = CreateRect("FloatingTextRoot", _stageRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _floatPool = new StageFloatingTextPool(_floatRoot);

            _statusLine = CreateLabel("StageStatus", _stageRoot, 18, TextAnchor.LowerLeft, new Vector2(16f, 12f));
            _statusLine.alignment = TextAlignmentOptions.BottomLeft;

            _partyHomeX = -168f;
            _monsterHomeX = 188f;
            var actorY = ExplorationHudLayoutMetrics.StageGroundInset + 8f;
            _partyActor.anchoredPosition = new Vector2(_partyHomeX, actorY);
            _monsterActor.anchoredPosition = new Vector2(_monsterHomeX, actorY);

            ApplyPartyVisual(ExplorationSystem.GetCurrentState()?.Party);
        }

        private void PlayBeatAsync(StageBeatRequest request)
        {
            CancelBeatPlayback();
            _beatCts = new CancellationTokenSource();
            PlayBeatInternalAsync(request, _beatCts.Token).Forget();
        }
        private async UniTaskVoid PlayBeatInternalAsync(StageBeatRequest request, CancellationToken token)
        {
            try
            {
                if (_stageRoot == null)
                {
                    ExplorationStageSystem.CompleteCurrentBeat();
                    return;
                }

                switch (request.Kind)
                {
                    case StageBeatKind.Combat:
                        await PlayCombatBeatAsync(request, token);
                        break;
                    case StageBeatKind.Discovery:
                        await PlayDiscoveryBeatAsync(request, token);
                        break;
                    case StageBeatKind.Rest:
                    case StageBeatKind.Trap:
                        await PlayShortBeatAsync(request, token);
                        break;
                    case StageBeatKind.Milestone:
                        await PlayMilestoneBeatAsync(request, token);
                        break;
                    default:
                        await PlayMoveBeatAsync(request, token);
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            finally
            {
                if (!token.IsCancellationRequested)
                    ExplorationStageSystem.CompleteCurrentBeat();
            }
        }

        private async UniTask PlayMoveBeatAsync(StageBeatRequest request, CancellationToken token)
        {
            SetStatus("탐색 중…");
            _monsterActor.gameObject.SetActive(false);
            await MovePartyAsync(_partyHomeX + 48f, MoveDuration, token);
            await MovePartyAsync(_partyHomeX, MoveDuration * 0.6f, token);
        }

        private async UniTask PlayCombatBeatAsync(StageBeatRequest request, CancellationToken token)
        {
            if (request.IsCombatBatch)
            {
                await PlayCombatBatchBeatAsync(request.CombatBatch, request.Party, token);
                return;
            }

            var combat = request.Event.Combat;
            var monsterVisual = StageMonsterVisual.Resolve(request.Event);
            var monsterName = string.IsNullOrEmpty(combat?.MonsterDisplayName) ? "몬스터" : combat.MonsterDisplayName;
            SetStatus($"{monsterName} 조우");
            ApplyMonsterVisual(monsterVisual);
            _monsterActor.gameObject.SetActive(true);
            _monsterNameText.text = monsterName;
            _monsterHpBar.value = 1f;
            _partyHpBar.value = GetPartyHpRatio(request.Party);

            _monsterActor.localScale = Vector3.zero;
            await StageActorMotion.PlaySpawnScaleAsync(_monsterActor, monsterVisual.Scale, token);

            var hitCount = StageVfxDensitySettings.CapCombatHitCount(Mathf.Clamp(combat?.DurationTicks ?? 2, 1, 4));
            var hitInterval = CombatHitInterval * StageVfxDensitySettings.HitIntervalMultiplier;
            var damagePerHit = combat == null || hitCount == 0
                ? 0
                : Mathf.Max(1, combat.DamageDealt / hitCount);
            var monsterHp = 1f;

            for (var i = 0; i < hitCount; i++)
            {
                await StageActorMotion.PlayAttackLungeAsync(_partyActor, 28f, token);
                await FlashSlashAsync(token);
                monsterHp = Mathf.Max(0f, monsterHp - (1f / hitCount));
                _monsterHpBar.value = monsterHp;
                SpawnFloatingText(_monsterActor.anchoredPosition + new Vector2(0f, 40f), $"-{damagePerHit}", new Color(1f, 0.82f, 0.35f));

                if (combat != null && combat.DamageTaken > 0 && i % 2 == 1 && StageVfxDensitySettings.ShowPartyDamageFloaters)
                {
                    var taken = Mathf.Max(1, combat.DamageTaken / Mathf.Max(1, hitCount / 2));
                    SpawnFloatingText(_partyActor.anchoredPosition + new Vector2(0f, 40f), $"-{taken}", new Color(1f, 0.45f, 0.45f));
                    await StageActorMotion.PlayHitShakeAsync(_partyActor, token);
                    _partyHpBar.value = GetPartyHpRatio(request.Party);
                }

                await UniTask.Delay(TimeSpan.FromSeconds(hitInterval), cancellationToken: token);
            }

            if (combat?.Outcome == CombatOutcome.Victory)
            {
                SetStatus("처치!");
                await StageActorMotion.PlayDeathScaleAsync(_monsterActor, token);
                _monsterActor.gameObject.SetActive(false);

                if (combat.GoldGained > 0)
                    SpawnFloatingText(_partyActor.anchoredPosition + new Vector2(24f, 56f), $"+{combat.GoldGained} G", new Color(0.95f, 0.85f, 0.35f));

                foreach (var loot in combat.Loot)
                {
                    if (loot.Quantity <= 0)
                        continue;

                    var lootColor = request.Event.Salience >= SalienceGrade.Significant
                        ? new Color(0.95f, 0.62f, 1f)
                        : new Color(0.75f, 0.9f, 1f);
                    SpawnFloatingText(_partyActor.anchoredPosition + new Vector2(-12f, 72f), $"+{loot.Quantity}", lootColor);
                }

                TryShowMemoryHint(request.Party, request.Event);
            }
            else
            {
                SetStatus("후퇴");
                _monsterActor.gameObject.SetActive(false);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: token);
        }

        private async UniTask PlayDiscoveryBeatAsync(StageBeatRequest request, CancellationToken token)
        {
            SetStatus("발견!");
            _monsterActor.gameObject.SetActive(false);
            await MovePartyAsync(_partyHomeX + 24f, MoveDuration * 0.5f, token);

            var label = string.IsNullOrEmpty(request.Event.DiscoveryDisplayName)
                ? "전리품"
                : request.Event.DiscoveryDisplayName;
            SpawnFloatingText(_partyActor.anchoredPosition + new Vector2(0f, 64f), label, new Color(0.95f, 0.85f, 0.35f));

            if (request.Event.GoldDelta > 0)
                SpawnFloatingText(_partyActor.anchoredPosition + new Vector2(0f, 88f), $"+{request.Event.GoldDelta} G", new Color(0.95f, 0.85f, 0.35f));

            await UniTask.Delay(TimeSpan.FromSeconds(0.35f), cancellationToken: token);
            await MovePartyAsync(_partyHomeX, MoveDuration * 0.4f, token);
        }

        private async UniTask PlayShortBeatAsync(StageBeatRequest request, CancellationToken token)
        {
            SetStatus(request.Kind == StageBeatKind.Rest ? "휴식" : "함정!");
            _monsterActor.gameObject.SetActive(false);
            await UniTask.Delay(TimeSpan.FromSeconds(ShortBeatDuration), cancellationToken: token);
        }

        private async UniTask PlayMilestoneBeatAsync(StageBeatRequest request, CancellationToken token)
        {
            SetStatus("층 돌파!");
            _monsterActor.gameObject.SetActive(false);
            await MovePartyAsync(_partyHomeX + 80f, MoveDuration, token);
            SpawnFloatingText(_partyActor.anchoredPosition + new Vector2(0f, 72f), "PORTAL", new Color(0.78f, 0.62f, 1f));
            await UniTask.Delay(TimeSpan.FromSeconds(0.45f), cancellationToken: token);
            await MovePartyAsync(_partyHomeX, MoveDuration * 0.5f, token);
        }

        private async UniTask PlayCombatBatchBeatAsync(StageCombatBatch batch, PartyState party, CancellationToken token)
        {
            var count = batch.Count;
            var monsterName = batch.PrimaryMonsterName;
            SetStatus($"{monsterName} ×{count}");
            var batchVisual = StageMonsterVisual.Resolve(batch.Events[0]);
            ApplyMonsterVisual(batchVisual);
            _monsterActor.gameObject.SetActive(true);
            _monsterNameText.text = $"{monsterName} ×{count}";
            _monsterHpBar.value = 1f;
            _partyHpBar.value = GetPartyHpRatio(party);

            _monsterActor.localScale = Vector3.zero;
            await StageActorMotion.PlaySpawnScaleAsync(_monsterActor, batchVisual.Scale, token);

            var hitCount = StageVfxDensitySettings.CapCombatHitCount(Mathf.Clamp(count + 1, 2, 5));
            var hitInterval = CombatHitInterval * 0.85f * StageVfxDensitySettings.HitIntervalMultiplier;
            var damagePerHit = Mathf.Max(1, batch.TotalDamageDealt / hitCount);
            var monsterHp = 1f;

            for (var i = 0; i < hitCount; i++)
            {
                await StageActorMotion.PlayAttackLungeAsync(_partyActor, 28f, token);
                await FlashSlashAsync(token);
                monsterHp = Mathf.Max(0f, monsterHp - (1f / hitCount));
                _monsterHpBar.value = monsterHp;
                SpawnFloatingText(_monsterActor.anchoredPosition + new Vector2(0f, 40f), $"-{damagePerHit}", new Color(1f, 0.82f, 0.35f));
                await UniTask.Delay(TimeSpan.FromSeconds(hitInterval), cancellationToken: token);
            }

            SetStatus($"처치 ×{count}");
            await StageActorMotion.PlayDeathScaleAsync(_monsterActor, token);
            _monsterActor.gameObject.SetActive(false);

            if (batch.TotalGold > 0)
                SpawnFloatingText(_partyActor.anchoredPosition + new Vector2(24f, 56f), $"+{batch.TotalGold} G", new Color(0.95f, 0.85f, 0.35f));

            await UniTask.Delay(TimeSpan.FromSeconds(0.15f), cancellationToken: token);
        }

        private async UniTask FlashSlashAsync(CancellationToken token)
        {
            if (!StageVfxDensitySettings.ShowSlashVfx || _slashVfx == null || _monsterActor == null)
                return;

            var rt = _slashVfx.rectTransform;
            rt.anchoredPosition = _monsterActor.anchoredPosition + new Vector2(-12f, 48f);
            rt.localRotation = Quaternion.Euler(0f, 0f, -24f);

            if (_slashCanvasGroup != null)
            {
                await StageActorMotion.PlaySlashFadeAsync(_slashCanvasGroup, token);
                return;
            }

            _slashVfx.gameObject.SetActive(true);
            _slashVfx.color = new Color(1f, 0.95f, 0.55f, 0.9f);
            await LMotion.Create(0.9f, 0f, 0.14f)
                .Bind(alpha => _slashVfx.color = new Color(1f, 0.95f, 0.55f, alpha))
                .ToUniTask(token);
            _slashVfx.gameObject.SetActive(false);
        }

        private void RefreshSegmentLabel(ExplorationState state)
        {
            if (_segmentLabel == null)
                return;

            if (state?.IsExploring != true)
            {
                _segmentLabel.text = string.Empty;
                return;
            }

            var label = StageSegmentUtil.BuildStageLabel(state);
            var hint = StageSegmentUtil.BuildSegmentHint(state);
            _segmentLabel.text = $"{label}  ·  {hint}";
        }

        private async UniTask MovePartyAsync(float targetX, float duration, CancellationToken token)
        {
            var start = _partyActor.anchoredPosition;
            var end = new Vector2(targetX, start.y);
            var parallaxShift = (targetX - start.x) * 0.15f * StageVfxDensitySettings.ParallaxShiftMultiplier;
            var parallaxStart = _parallaxFar != null ? _parallaxFar.anchoredPosition.x : 0f;

            await LMotion.Create(start, end, duration)
                .WithEase(Ease.OutQuad)
                .Bind(value =>
                {
                    _partyActor.anchoredPosition = value;
                    if (_parallaxFar != null)
                    {
                        var t = Mathf.InverseLerp(start.x, end.x, value.x);
                        _parallaxFar.anchoredPosition = new Vector2(parallaxStart - parallaxShift * t, _parallaxFar.anchoredPosition.y);
                    }
                })
                .ToUniTask(token);
        }

        private void SpawnFloatingText(Vector2 anchoredPosition, string text, Color color)
        {
            if (_floatPool == null)
                return;

            var label = _floatPool.Rent();
            var rt = label.rectTransform;
            rt.anchoredPosition = anchoredPosition;
            label.text = text;
            label.color = color;
            label.fontStyle = FontStyles.Bold;
            FloatAndFadeAsync(rt, label, View.destroyCancellationToken).Forget();
        }

        private async UniTaskVoid FloatAndFadeAsync(RectTransform rt, TextMeshProUGUI label, CancellationToken token)
        {
            var start = rt.anchoredPosition;
            var end = start + new Vector2(0f, 48f);
            try
            {
                await LMotion.Create(start, end, 0.55f)
                    .WithEase(Ease.OutQuad)
                    .Bind(value => rt.anchoredPosition = value)
                    .ToUniTask(token);

                var color = label.color;
                await LMotion.Create(color.a, 0f, 0.25f)
                    .Bind(alpha =>
                    {
                        color.a = alpha;
                        label.color = color;
                    })
                    .ToUniTask(token);
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
            finally
            {
                if (label != null)
                    _floatPool?.Release(label);
            }
        }

        private void TryShowMemoryHint(PartyState party, ExplorationEvent explorationEvent)
        {
            if (explorationEvent == null || explorationEvent.Salience < SalienceGrade.Significant)
                return;

            var leader = party?.Leader;
            if (leader == null)
                return;

            var preview = CharacterMemorySystem.BuildHudPreview(leader.CharacterId);
            if (string.IsNullOrEmpty(preview))
                return;

            var firstLine = preview.Split('\n')[0].Trim();
            if (!string.IsNullOrEmpty(firstLine))
                SetStatus(firstLine);
        }

        private void ApplyZoneTheme(ExplorationState state)
        {
            if (_stageBackground == null || state == null)
                return;

            var theme = StageZoneTheme.Resolve(state.ZoneId);
            _stageBackground.color = theme.Background;

            if (_parallaxBanner != null)
            {
                if (_parallaxBanner.sprite == null)
                    RuntimeUiSprites.ApplySimpleImage(_parallaxBanner, RuntimeUiSprites.IllustZoneBanner, theme.ParallaxBannerTint);
                else
                    _parallaxBanner.color = theme.ParallaxBannerTint;
            }

            if (_groundLine != null)
                _groundLine.color = theme.Ground;
        }

        private void ApplyPartyVisual(PartyState party)
        {
            if (_partyBody == null)
                return;

            var leader = party?.Leader;
            if (leader == null)
                return;

            var spriteKey = StageVisualCatalog.ResolvePartySpriteKey(leader.Role);
            var tint = ExplorationHudStatusFormatter.GetRoleTintColor(leader.Role);
            var hasSprite = RuntimeStageSprites.Get(spriteKey) != null;
            RuntimeStageSprites.ApplyActor(_partyBody, spriteKey, hasSprite ? Color.white : tint);
            _partyBody.rectTransform.localScale = Vector3.one;
            _partyBody.rectTransform.sizeDelta = new Vector2(
                ExplorationHudLayoutMetrics.StageActorPartyWidth,
                ExplorationHudLayoutMetrics.StageActorPartyHeight);
        }

        private void ApplyMonsterVisual(StageMonsterVisual visual)
        {
            if (_monsterBody != null)
            {
                var hasSprite = !string.IsNullOrEmpty(visual.SpriteKey) && RuntimeStageSprites.Get(visual.SpriteKey) != null;
                RuntimeStageSprites.ApplyActor(_monsterBody, visual.SpriteKey, hasSprite ? Color.white : visual.BodyColor);
                _monsterBody.rectTransform.localScale = new Vector3(-1f, 1f, 1f);
                _monsterBody.rectTransform.sizeDelta = new Vector2(
                    ExplorationHudLayoutMetrics.StageActorMonsterWidth,
                    ExplorationHudLayoutMetrics.StageActorMonsterHeight);
            }

            if (_monsterEliteRing != null)
                _monsterEliteRing.gameObject.SetActive(visual.ShowEliteRing);
        }

        private void SetStatus(string text)
        {
            if (_statusLine != null)
                _statusLine.text = text;
        }

        private void ResetStageVisual()
        {
            CancelBeatPlayback(flushBeat: true);
            if (_partyActor != null)
                _partyActor.anchoredPosition = new Vector2(_partyHomeX, ExplorationHudLayoutMetrics.StageGroundInset + 8f);
            if (_monsterActor != null)
            {
                _monsterActor.gameObject.SetActive(false);
                _monsterActor.localScale = Vector3.one;
            }
            if (_monsterEliteRing != null)
                _monsterEliteRing.gameObject.SetActive(false);
            SetStatus(string.Empty);
        }

        private void CancelBeatPlayback(bool flushBeat = false)
        {
            _beatCts?.Cancel();
            _beatCts?.Dispose();
            _beatCts = null;

            if (flushBeat && ExplorationStageSystem.IsBusy)
                ExplorationStageSystem.CompleteCurrentBeat();
        }

        private static float GetPartyHpRatio(PartyState party)
        {
            if (party?.Members == null || party.Members.Count == 0)
                return 1f;

            var current = 0;
            var max = 0;
            foreach (var member in party.Members)
            {
                current += member.CurrentHp;
                max += member.MaxHp;
            }

            return max <= 0 ? 1f : Mathf.Clamp01((float)current / max);
        }

        private Transform ResolveExploreContent()
        {
            if (View.transform.name == "ExploreContent")
                return View.transform;

            var fromView = View.transform.Find("Body/CenterPanel/ExploreContent")
                ?? View.transform.Find("ExploreContent");
            if (fromView != null)
                return fromView;

            var hud = View.GetComponent<ExplorationHudPanel>() ?? View.GetComponentInParent<ExplorationHudPanel>();
            return hud == null ? null : hud.transform.Find("Body/CenterPanel/ExploreContent");
        }

        private static RectTransform CreateRect(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            return rt;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static RectTransform CreateActorRoot(
            string name,
            Transform parent,
            Color bodyColor,
            out Image body,
            out Slider hpBar,
            bool isParty)
        {
            var root = CreateRect(name, parent, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, Vector2.zero);
            root.sizeDelta = new Vector2(
                isParty ? ExplorationHudLayoutMetrics.StageActorPartyWidth + 16f : ExplorationHudLayoutMetrics.StageActorMonsterWidth + 16f,
                isParty ? ExplorationHudLayoutMetrics.StageActorPartyHeight + 24f : ExplorationHudLayoutMetrics.StageActorMonsterHeight + 24f);

            body = CreateImage("Body", root, new Color(1f, 1f, 1f, 0f));
            var bodyRt = body.rectTransform;
            bodyRt.anchorMin = new Vector2(0.5f, 0f);
            bodyRt.anchorMax = new Vector2(0.5f, 0f);
            bodyRt.pivot = new Vector2(0.5f, 0f);
            bodyRt.sizeDelta = new Vector2(
                isParty ? ExplorationHudLayoutMetrics.StageActorPartyWidth : ExplorationHudLayoutMetrics.StageActorMonsterWidth,
                isParty ? ExplorationHudLayoutMetrics.StageActorPartyHeight : ExplorationHudLayoutMetrics.StageActorMonsterHeight);
            bodyRt.anchoredPosition = new Vector2(0f, 8f);

            var hpGo = new GameObject("HpBar", typeof(RectTransform), typeof(Slider));
            hpGo.transform.SetParent(root, false);
            var hpRt = hpGo.GetComponent<RectTransform>();
            hpRt.anchorMin = new Vector2(0.5f, 1f);
            hpRt.anchorMax = new Vector2(0.5f, 1f);
            hpRt.pivot = new Vector2(0.5f, 1f);
            hpRt.sizeDelta = new Vector2(64f, 10f);
            hpRt.anchoredPosition = new Vector2(0f, 8f);

            hpBar = hpGo.GetComponent<Slider>();
            hpBar.minValue = 0f;
            hpBar.maxValue = 1f;
            hpBar.value = 1f;
            hpBar.interactable = false;
            hpBar.transition = Selectable.Transition.None;

            var bg = CreateImage("Background", hpRt, new Color(0.15f, 0.15f, 0.15f, 0.85f));
            Stretch(bg.rectTransform);
            RuntimeUiSprites.ApplyHpTrack(bg);

            var fillArea = CreateRect("Fill Area", hpRt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var fill = CreateImage("Fill", fillArea, isParty ? new Color(0.35f, 0.9f, 0.45f) : new Color(0.95f, 0.25f, 0.25f));
            Stretch(fill.rectTransform);
            RuntimeUiSprites.ApplyHpFill(fill);
            hpBar.fillRect = fill.rectTransform;
            hpBar.targetGraphic = fill;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;

            return root;
        }

        private static TextMeshProUGUI CreateLabel(
            string name,
            Transform parent,
            int fontSize,
            TextAnchor anchor,
            Vector2 anchoredPosition)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(220f, 40f);
            rt.anchoredPosition = anchoredPosition;

            var text = go.GetComponent<TextMeshProUGUI>();
            text.font = RuntimeUiTmpFont.Get();
            text.fontSize = fontSize;
            text.alignment = UiTmpUtil.ToAlignment(anchor);
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            return text;
        }
    }
}
