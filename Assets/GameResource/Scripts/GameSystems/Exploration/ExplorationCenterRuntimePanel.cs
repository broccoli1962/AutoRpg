using Backend.GameSystems.Exploration.Data;
using Backend.Object.UI;
using Backend.Util;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// v2 중앙 패널 — 프리팹 ExploreContent 바인딩·상태 갱신.
    /// </summary>
    public sealed class ExplorationCenterRuntimePanel : ExplorationHudSubview<ExplorationCenterRuntimePresenter>
    {
    }

    public sealed class ExplorationCenterRuntimePresenter : UIPresenter<ExplorationCenterRuntimePanel>
    {
        private GameObject _exploreRoot;
        private GameObject _startCardRoot;
        private Image _zoneBanner;
        private TextMeshProUGUI _zoneTitleText;
        private TextMeshProUGUI _floorText;
        private Slider _progressSlider;
        private TextMeshProUGUI _progressLabel;
        private TextMeshProUGUI _statusText;
        private CompositeDisposable _disposables;

        public override void OnOpen()
        {
            BindWidgets();
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            ExplorationChannels.OnStateChanged
                .Subscribe(Refresh)
                .AddTo(_disposables);

            Refresh(ExplorationSystem.GetCurrentState());
        }

        public override void OnClose()
        {
            _disposables?.Dispose();
            _disposables = null;
        }

        private void BindWidgets()
        {
            if (_exploreRoot != null)
                return;

            var centerPanel = ResolveCenterPanelTransform();
            if (centerPanel == null)
                return;

            _exploreRoot = centerPanel.Find("ExploreContent")?.gameObject;
            _startCardRoot = centerPanel.Find("StartCard")?.gameObject;
            if (_exploreRoot == null)
                return;

            _zoneBanner = _exploreRoot.transform.Find("BannerSlot")?.GetComponent<Image>();
            _zoneTitleText = _exploreRoot.transform.Find("ZoneTitle")?.GetComponent<TextMeshProUGUI>();
            _floorText = _exploreRoot.transform.Find("FloorLine")?.GetComponent<TextMeshProUGUI>();
            _progressSlider = _exploreRoot.transform.Find("ProgressTrack")?.GetComponent<Slider>();
            _progressLabel = _exploreRoot.transform.Find("ProgressLabel")?.GetComponent<TextMeshProUGUI>();
            _statusText = _exploreRoot.transform.Find("StatusLine")?.GetComponent<TextMeshProUGUI>();
        }

        private void Refresh(ExplorationState state)
        {
            if (_exploreRoot == null)
                return;

            var isExploring = state?.IsExploring == true;
            _exploreRoot.SetActive(isExploring);
            if (_startCardRoot != null)
                _startCardRoot.SetActive(!isExploring);

            if (!isExploring)
                return;

            if (_zoneBanner != null)
            {
                RuntimeUiSprites.ApplySimpleImage(_zoneBanner, RuntimeUiSprites.IllustZoneBanner, Color.white);
                _zoneBanner.preserveAspect = false;
                _zoneBanner.gameObject.SetActive(false);
            }

            if (_zoneTitleText != null)
                _zoneTitleText.text = ZoneDefinitions.GetZoneDisplayName(state.ZoneId);

            if (_floorText != null)
                _floorText.text = $"현재 {state.CurrentFloor} / {state.MaxFloor} 층";

            if (_progressSlider != null)
                _progressSlider.value = Mathf.Clamp01(state.FloorProgress / 100f);

            if (_progressLabel != null)
                _progressLabel.text = $"층 진행 {state.FloorProgress:0.#}% · Tick {state.CurrentTick}";

            if (_statusText != null)
                _statusText.text = ExplorationHudStatusFormatter.BuildCenterStatusLine(state);
        }

        private Transform ResolveCenterPanelTransform()
        {
            if (View.transform.name == "CenterPanel")
                return View.transform;

            var fromView = View.transform.Find("Body/CenterPanel") ?? View.transform.Find("CenterPanel");
            if (fromView != null)
                return fromView;

            var hudPanel = View.GetComponent<ExplorationHudPanel>() ?? View.GetComponentInParent<ExplorationHudPanel>();
            return hudPanel == null ? null : hudPanel.transform.Find("Body/CenterPanel");
        }

    }
}
