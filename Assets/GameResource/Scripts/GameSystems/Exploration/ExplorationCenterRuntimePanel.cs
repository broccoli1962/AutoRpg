using Backend.GameSystems.DynamicEvent;
using Backend.GameSystems.Exploration.Data;
using Backend.Object.UI;
using Backend.Util;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// v2 중앙 패널 — 프리팹 ExploreContent 바인딩·상태 갱신.
    /// </summary>
    public sealed class ExplorationCenterRuntimePanel : MonoBehaviour
    {
        private GameObject _exploreRoot;
        private Image _zoneBanner;
        private Text _zoneTitleText;
        private Text _floorText;
        private Slider _progressSlider;
        private Text _progressLabel;
        private Transform _portraitStripRoot;
        private Text _statusText;
        private CompositeDisposable _disposables;

        private void Start()
        {
            BindPrefab();
            _disposables = new CompositeDisposable();

            ExplorationChannels.OnStateChanged
                .Subscribe(Refresh)
                .AddTo(_disposables);

            Refresh(ExplorationManager.GetCurrentState());
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }

        private void BindPrefab()
        {
            var centerPanel = FindHudTransform("Body/CenterPanel");
            if (centerPanel == null)
                return;

            _exploreRoot = centerPanel.Find("ExploreContent")?.gameObject;
            if (_exploreRoot == null)
                return;

            _zoneBanner = _exploreRoot.transform.Find("BannerSlot")?.GetComponent<Image>();
            _zoneTitleText = _exploreRoot.transform.Find("ZoneTitle")?.GetComponent<Text>();
            _floorText = _exploreRoot.transform.Find("FloorLine")?.GetComponent<Text>();
            _progressSlider = _exploreRoot.transform.Find("ProgressTrack")?.GetComponent<Slider>();
            _progressLabel = _exploreRoot.transform.Find("ProgressLabel")?.GetComponent<Text>();
            _portraitStripRoot = _exploreRoot.transform.Find("PartyStrip");
            _statusText = _exploreRoot.transform.Find("StatusLine")?.GetComponent<Text>();
        }

        private Transform FindHudTransform(string relativePath)
        {
            var hudPanel = GetComponent<ExplorationHudPanel>() ?? GetComponentInParent<ExplorationHudPanel>();
            return hudPanel == null ? null : hudPanel.transform.Find(relativePath);
        }

        private void Refresh(ExplorationState state)
        {
            if (_exploreRoot == null)
                return;

            var isExploring = state?.IsExploring == true;
            _exploreRoot.SetActive(isExploring);

            var startCard = FindHudTransform("Body/CenterPanel/StartCard")?.gameObject;
            if (startCard != null)
                startCard.SetActive(!isExploring);

            if (!isExploring)
                return;

            if (_zoneBanner != null)
                RuntimeUiSprites.ApplySimpleImage(_zoneBanner, RuntimeUiSprites.IllustZoneBanner, Color.white);

            if (_zoneTitleText != null)
                _zoneTitleText.text = ZoneDefinitions.GetZoneDisplayName(state.ZoneId);

            if (_floorText != null)
                _floorText.text = $"현재 {state.CurrentFloor} / {state.MaxFloor} 층";

            if (_progressSlider != null)
                _progressSlider.value = Mathf.Clamp01(state.FloorProgress / 100f);

            if (_progressLabel != null)
                _progressLabel.text = $"층 진행 {state.FloorProgress:0.#}% · Tick {state.CurrentTick}";

            RefreshPortraitStrip(state);
            if (_statusText != null)
                _statusText.text = BuildStatusLine(state);
        }

        private void RefreshPortraitStrip(ExplorationState state)
        {
            if (_portraitStripRoot == null)
                return;

            var members = state?.Party?.Members;
            for (var i = 0; i < _portraitStripRoot.childCount; i++)
            {
                var slot = _portraitStripRoot.GetChild(i);
                var image = slot.GetComponent<Image>();
                if (image == null)
                    continue;

                if (members != null && i < members.Count)
                {
                    slot.gameObject.SetActive(true);
                    RuntimeUiSprites.ApplyPortraitFrame(image);
                    image.color = Color.Lerp(GetRoleTintColor(members[i].Role), Color.white, 0.25f);
                }
                else
                {
                    slot.gameObject.SetActive(false);
                }
            }
        }

        private static string BuildStatusLine(ExplorationState state)
        {
            if (!state.IsExploring)
                return "탐험이 종료되었습니다.";

            if (state.IsPaused)
                return "일시정지 중 · R 또는 귀환 버튼으로 재개/귀환";

            if (DynamicEventManager.IsAwaitingManualChoice)
                return "이벤트 선택 대기 · 1/2 키 또는 팝업에서 선택";

            return "자동 탐험 진행 중";
        }

        private static Color GetRoleTintColor(CharacterRole role) =>
            role switch
            {
                CharacterRole.Warrior => new Color(0.88f, 0.48f, 0.48f, 1f),
                CharacterRole.Rogue => new Color(0.62f, 0.83f, 0.62f, 1f),
                CharacterRole.Mage => new Color(0.43f, 0.77f, 1f, 1f),
                CharacterRole.Bard => new Color(1f, 0.85f, 0.4f, 1f),
                CharacterRole.Cleric => new Color(0.79f, 0.63f, 1f, 1f),
                _ => new Color(0.8f, 0.8f, 0.8f, 1f)
            };
    }
}
