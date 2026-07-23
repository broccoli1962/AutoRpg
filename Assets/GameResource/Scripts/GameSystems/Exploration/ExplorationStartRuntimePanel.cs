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
    /// v2 길드 대기 StartCard — 프리팹 바인딩·탐험 시작 CTA.
    /// </summary>
    public sealed class ExplorationStartRuntimePanel : ExplorationHudSubview<ExplorationStartRuntimePresenter>
    {
        public void SetGuildTabActive(bool isExploreTab) => ReadyPresenter.SetGuildTabActive(isExploreTab);
    }

    public sealed class ExplorationStartRuntimePresenter : UIPresenter<ExplorationStartRuntimePanel>
    {
        private GameObject _startCardRoot;
        private TextMeshProUGUI _summaryText;
        private Button _startButton;
        private CompositeDisposable _disposables;
        private bool _guildExploreTabActive = true;

        public override void OnOpen()
        {
            BindWidgets();
            if (_startButton != null)
            {
                _startButton.onClick.RemoveListener(OnStartClicked);
                _startButton.onClick.AddListener(OnStartClicked);
            }

            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            ExplorationChannels.OnStateChanged
                .Subscribe(Refresh)
                .AddTo(_disposables);

            ExplorationChannels.OnExplorationEnded
                .Subscribe(_ => Refresh(ExplorationSystem.GetCurrentState()))
                .AddTo(_disposables);

            Refresh(ExplorationSystem.GetCurrentState());
        }

        public override void OnClose()
        {
            if (_startButton != null)
                _startButton.onClick.RemoveListener(OnStartClicked);

            _disposables?.Dispose();
            _disposables = null;
        }

        public void SetGuildTabActive(bool isExploreTab)
        {
            _guildExploreTabActive = isExploreTab;
            Refresh(ExplorationSystem.GetCurrentState());
        }

        private void BindWidgets()
        {
            if (_startButton != null)
                return;

            var startCard = ResolveStartCardTransform();
            if (startCard == null)
                return;

            _startCardRoot = startCard.gameObject;
            _summaryText = startCard.Find("Summary")?.GetComponent<TextMeshProUGUI>();
            _startButton = startCard.Find("StartButton")?.GetComponent<Button>();

            var titleText = startCard.Find("Title")?.GetComponent<TextMeshProUGUI>();
            if (titleText != null)
                ModernUiStyle.ApplyTitleLarge(titleText);
        }

        private Transform ResolveStartCardTransform()
        {
            if (View.transform.name == "StartCard")
                return View.transform;

            var fromView = View.transform.Find("Body/CenterPanel/StartCard")
                ?? View.transform.Find("CenterPanel/StartCard");
            if (fromView != null)
                return fromView;

            var hudPanel = View.GetComponent<ExplorationHudPanel>() ?? View.GetComponentInParent<ExplorationHudPanel>();
            return hudPanel == null ? null : hudPanel.transform.Find("Body/CenterPanel/StartCard");
        }

        private void Refresh(ExplorationState state)
        {
            if (_startCardRoot == null)
                BindWidgets();

            if (_startCardRoot == null)
                return;

            var show = (state == null || !state.IsExploring) && _guildExploreTabActive;
            _startCardRoot.SetActive(show);

            if (!show || _summaryText == null)
                return;

            _summaryText.text = ExplorationHudStatusFormatter.BuildStartCardSummary(state);
        }

        private void OnStartClicked()
        {
            ExplorationSystem.BeginExplorationFromPlayer();
        }
    }
}
