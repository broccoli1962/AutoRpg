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
        private const int MaxVisibleParty = 3;
        private const float CompanionSpacingX = 52f;
        private const float CompanionScale = 0.82f;

        private RectTransform _stageRoot;
        private Image _stageBackground;
        private Image _groundLine;
        private RectTransform _partyActor;
        private RectTransform[] _companionActors;
        private RectTransform _monsterActor;
        private Image _partyBody;
        private Image[] _companionBodies;
        private Image _monsterBody;
        private Image _monsterEliteRing;
        private Image _parallaxBanner;
        private Slider _partyHpBar;
        private Slider[] _companionHpBars;
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
        private float[] _companionHomeOffsets;

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

            _stageRoot = exploreRoot.Find("StageViewport") as RectTransform;
            if (_stageRoot == null)
            {
                Debug.LogError("[ExplorationStageRuntimePanel] Prefab StageViewport missing under ExploreContent. Bake via Unity MCP.");
                return;
            }

            _stageBackground = _stageRoot.Find("StageBackground")?.GetComponent<Image>();
            _parallaxFar = _stageRoot.Find("ParallaxFar") as RectTransform;
            _parallaxBanner = _parallaxFar != null ? _parallaxFar.GetComponent<Image>() : null;
            if (_parallaxBanner != null)
                RuntimeUiSprites.ApplySimpleImage(_parallaxBanner, RuntimeUiSprites.IllustZoneBanner, Color.white);

            _segmentLabel = _stageRoot.Find("SegmentLabel")?.GetComponent<TextMeshProUGUI>();
            _slashVfx = _stageRoot.Find("SlashVfx")?.GetComponent<Image>();
            if (_slashVfx != null)
            {
                if (!_slashVfx.TryGetComponent(out _slashCanvasGroup))
                    _slashCanvasGroup = _slashVfx.gameObject.AddComponent<CanvasGroup>();
                RuntimeStageSprites.ApplyVfx(_slashVfx, StageVisualCatalog.VfxSlash, Color.white);
            }

            _groundLine = _stageRoot.Find("GroundLine")?.GetComponent<Image>();
            _partyActor = _stageRoot.Find("PartyActor") as RectTransform;
            _partyBody = _partyActor != null ? _partyActor.Find("Body")?.GetComponent<Image>() : null;
            _partyHpBar = _partyActor != null ? _partyActor.Find("HpBar")?.GetComponent<Slider>() : null;

            _companionActors = new RectTransform[MaxVisibleParty - 1];
            _companionBodies = new Image[MaxVisibleParty - 1];
            _companionHpBars = new Slider[MaxVisibleParty - 1];
            _companionHomeOffsets = new float[MaxVisibleParty - 1];
            for (var i = 0; i < _companionActors.Length; i++)
            {
                _companionActors[i] = _stageRoot.Find($"PartyCompanion{i + 1}") as RectTransform;
                _companionBodies[i] = _companionActors[i] != null
                    ? _companionActors[i].Find("Body")?.GetComponent<Image>()
                    : null;
                _companionHpBars[i] = _companionActors[i] != null
                    ? _companionActors[i].Find("HpBar")?.GetComponent<Slider>()
                    : null;
                _companionHomeOffsets[i] = -CompanionSpacingX * (i + 1);
                if (_companionActors[i] != null)
                    _companionActors[i].gameObject.SetActive(false);
            }

            _monsterActor = _stageRoot.Find("MonsterActor") as RectTransform;
            _monsterBody = _monsterActor != null ? _monsterActor.Find("Body")?.GetComponent<Image>() : null;
            _monsterHpBar = _monsterActor != null ? _monsterActor.Find("HpBar")?.GetComponent<Slider>() : null;
            _monsterEliteRing = _monsterActor != null ? _monsterActor.Find("EliteRing")?.GetComponent<Image>() : null;
            _monsterNameText = _monsterActor != null
                ? _monsterActor.Find("MonsterName")?.GetComponent<TextMeshProUGUI>()
                : null;
            if (_monsterActor != null)
                _monsterActor.gameObject.SetActive(false);
            if (_monsterEliteRing != null)
                _monsterEliteRing.gameObject.SetActive(false);

            _floatRoot = _stageRoot.Find("FloatingTextRoot") as RectTransform;
            if (_floatRoot != null)
                _floatPool = new StageFloatingTextPool(_floatRoot);

            _statusLine = _stageRoot.Find("StageStatus")?.GetComponent<TextMeshProUGUI>();

            _partyHomeX = _partyActor != null ? _partyActor.anchoredPosition.x : -168f;
            _monsterHomeX = _monsterActor != null ? _monsterActor.anchoredPosition.x : 188f;
            var actorY = _partyActor != null
                ? _partyActor.anchoredPosition.y
                : ExplorationHudLayoutMetrics.StageGroundInset + 8f;
            PlaceCompanionsAtLeaderX(_partyHomeX, actorY);

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
            ApplyPartyVisual(request.Party);

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
                    ApplyPartyVisual(request.Party);
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
            ApplyPartyVisual(party);

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
                    PlaceCompanionsAtLeaderX(value.x, value.y);
                    if (_parallaxFar != null)
                    {
                        var t = Mathf.InverseLerp(start.x, end.x, value.x);
                        _parallaxFar.anchoredPosition = new Vector2(parallaxStart - parallaxShift * t, _parallaxFar.anchoredPosition.y);
                    }
                })
                .ToUniTask(token);
        }

        private void PlaceCompanionsAtLeaderX(float leaderX, float actorY)
        {
            if (_companionActors == null || _companionHomeOffsets == null)
                return;

            for (var i = 0; i < _companionActors.Length; i++)
            {
                if (_companionActors[i] == null)
                    continue;

                _companionActors[i].anchoredPosition = new Vector2(leaderX + _companionHomeOffsets[i], actorY);
            }
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

            var members = party?.Members;
            if (members == null || members.Count == 0)
            {
                HideCompanions();
                return;
            }

            ApplyMemberVisual(_partyBody, _partyHpBar, members[0], isCompanion: false);

            if (_companionActors == null)
                return;

            for (var i = 0; i < _companionActors.Length; i++)
            {
                var memberIndex = i + 1;
                var show = memberIndex < members.Count && memberIndex < MaxVisibleParty;
                if (_companionActors[i] != null)
                    _companionActors[i].gameObject.SetActive(show);

                if (!show)
                    continue;

                ApplyMemberVisual(_companionBodies[i], _companionHpBars[i], members[memberIndex], isCompanion: true);
            }

            var actorY = _partyActor != null
                ? _partyActor.anchoredPosition.y
                : ExplorationHudLayoutMetrics.StageGroundInset + 8f;
            var leaderX = _partyActor != null ? _partyActor.anchoredPosition.x : _partyHomeX;
            PlaceCompanionsAtLeaderX(leaderX, actorY);
        }

        private static void ApplyMemberVisual(Image body, Slider hpBar, CharacterState member, bool isCompanion)
        {
            if (body == null || member == null)
                return;

            var spriteKey = StageVisualCatalog.ResolvePartySpriteKey(member.Role);
            var tint = ExplorationHudStatusFormatter.GetRoleTintColor(member.Role);
            var hasSprite = RuntimeStageSprites.Get(spriteKey) != null;
            RuntimeStageSprites.ApplyActor(body, spriteKey, hasSprite ? Color.white : tint);
            body.rectTransform.localScale = Vector3.one;
            var width = isCompanion
                ? ExplorationHudLayoutMetrics.StageActorPartyWidth * CompanionScale
                : ExplorationHudLayoutMetrics.StageActorPartyWidth;
            var height = isCompanion
                ? ExplorationHudLayoutMetrics.StageActorPartyHeight * CompanionScale
                : ExplorationHudLayoutMetrics.StageActorPartyHeight;
            body.rectTransform.sizeDelta = new Vector2(width, height);

            if (hpBar != null)
                hpBar.value = member.MaxHp <= 0 ? 1f : Mathf.Clamp01((float)member.CurrentHp / member.MaxHp);
        }

        private void HideCompanions()
        {
            if (_companionActors == null)
                return;

            foreach (var companion in _companionActors)
            {
                if (companion != null)
                    companion.gameObject.SetActive(false);
            }
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
            var actorY = ExplorationHudLayoutMetrics.StageGroundInset + 8f;
            if (_partyActor != null)
                _partyActor.anchoredPosition = new Vector2(_partyHomeX, actorY);
            PlaceCompanionsAtLeaderX(_partyHomeX, actorY);
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
    }
}
