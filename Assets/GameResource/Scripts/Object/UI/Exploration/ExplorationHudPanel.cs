using Backend.GameSystems.Exploration;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Prestige;
using Backend.Object.UI.Exploration;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Object.UI
{
    public class ExplorationHudPanel : UIPanel<ExplorationHudPresenter>
    {
        public override UILayer Layer => UILayer.HUD;

        [Header("Status")]
        [SerializeField] private Text _zoneFloorText;
        [SerializeField] private Text _goldText;
        [SerializeField] private Slider _progressSlider;
        [SerializeField] private Text _progressText;

        [Header("Controls")]
        [SerializeField] private CommonButton _pauseButton;
        [SerializeField] private CommonButton _resumeButton;
        [SerializeField] private CommonButton _returnButton;

        [Header("Views")]
        [SerializeField] private ExplorationLogFeedView _logFeedView;
        [SerializeField] private Text _filterText;
        [SerializeField] private Text _helpText;

        private ExplorationHudShortcuts _shortcuts;

        internal void BindShortcuts(System.Action refreshStatus)
        {
            if (_shortcuts == null)
                _shortcuts = gameObject.AddComponent<ExplorationHudShortcuts>();

            var chroniclePanel = GetComponent<ChronicleRuntimePanel>();
            var settingsPanel = GetComponent<ExplorationSettingsRuntimePanel>();
            settingsPanel?.Configure(refreshStatus);
            _shortcuts.Initialize(_logFeedView, chroniclePanel, settingsPanel, _filterText, refreshStatus);

            var enhancePanel = gameObject.GetComponent<EnhanceRuntimePanel>();
            var guildPanel = gameObject.GetComponent<GuildFacilityRuntimePanel>();
            var tabController = gameObject.GetComponent<GuildHudTabController>();
            guildPanel?.Configure(refreshStatus);
            tabController?.Initialize(chroniclePanel, enhancePanel, guildPanel, refreshStatus);
        }

        public Text ZoneFloorText => _zoneFloorText;
        public Text GoldText => _goldText;
        public Slider ProgressSlider => _progressSlider;
        public Text ProgressText => _progressText;
        public CommonButton PauseButton => _pauseButton;
        public CommonButton ResumeButton => _resumeButton;
        public CommonButton ReturnButton => _returnButton;
        public ExplorationLogFeedView LogFeedView => _logFeedView;
    }

    public class ExplorationHudPresenter : UIPresenter<ExplorationHudPanel>
    {
        private CompositeDisposable _disposables;

        public override void OnOpen()
        {
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            PrestigeManager.EnsureInitialized();
            View.BindShortcuts(() => RefreshState(ExplorationManager.GetCurrentState()));
            BindControls();
            RefreshState(ExplorationManager.GetCurrentState());

            ExplorationChannels.OnStateChanged
                .Subscribe(RefreshState)
                .AddTo(_disposables);

            ExplorationChannels.OnExplorationEnded
                .Subscribe(_ => RefreshControls())
                .AddTo(_disposables);
        }

        public override void OnClose()
        {
            View.LogFeedView?.Hide();
            _disposables?.Dispose();
            _disposables = null;
        }

        private void BindControls()
        {
            if (View.PauseButton != null)
            {
                View.PauseButton.OnClickAsObservable()
                    .Subscribe(_ => ExplorationManager.PauseExploration())
                    .AddTo(_disposables);
            }

            if (View.ResumeButton != null)
            {
                View.ResumeButton.OnClickAsObservable()
                    .Subscribe(_ => ExplorationManager.ResumeExploration())
                    .AddTo(_disposables);
            }

            if (View.ReturnButton != null)
            {
                View.ReturnButton.OnClickAsObservable()
                    .Subscribe(_ => ExplorationManager.ReturnToGuild())
                    .AddTo(_disposables);
            }
        }

        private void RefreshState(ExplorationState state)
        {
            var isWaiting = state == null || !state.IsExploring;

            if (View.ZoneFloorText != null)
            {
                View.ZoneFloorText.supportRichText = true;
                View.ZoneFloorText.text = isWaiting
                    ? "<color=#F5D673>길드 대기</color>  <size=14><color=#9AA8BC>하단 탭에서 시설·강화·연대기를 확인하세요</color></size>"
                    : ExplorationHudStatusFormatter.Build(state);
            }

            if (View.GoldText != null)
                View.GoldText.gameObject.SetActive(!isWaiting);

            if (View.ProgressSlider != null)
            {
                View.ProgressSlider.gameObject.SetActive(!isWaiting);
                if (!isWaiting)
                    View.ProgressSlider.value = Mathf.Clamp01(state.FloorProgress / 100f);
            }

            if (View.ProgressText != null)
            {
                View.ProgressText.gameObject.SetActive(!isWaiting);
                if (!isWaiting)
                    View.ProgressText.text = $"{state.FloorProgress:0.#}%";
            }

            if (View.LogFeedView != null)
            {
                if (isWaiting)
                    View.LogFeedView.ShowIdlePlaceholder();
                else
                    View.LogFeedView.Show();
            }

            RefreshControls(state);
        }

        private void RefreshControls(ExplorationState state = null)
        {
            state ??= ExplorationManager.GetCurrentState();
            var isRunning = state?.IsExploring == true;
            var isPaused = state?.IsPaused == true;

            if (View.PauseButton != null)
                View.PauseButton.gameObject.SetActive(isRunning && !isPaused);

            if (View.ResumeButton != null)
                View.ResumeButton.gameObject.SetActive(isRunning && isPaused);

            if (View.ReturnButton != null)
                View.ReturnButton.gameObject.SetActive(isRunning);
        }
    }
}
