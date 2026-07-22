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

        private ChronicleRuntimePanel _chroniclePanel;
        private ExplorationSettingsRuntimePanel _settingsPanel;
        private ExplorationHudShortcuts _shortcuts;

        protected override void Awake()
        {
            base.Awake();
            if (_logFeedView == null)
                ExplorationHudPanelLayoutBuilder.TryBuild(this);
        }

        internal void ConfigureRuntime(
            Text zoneFloorText,
            Text goldText,
            Slider progressSlider,
            Text progressText,
            CommonButton pauseButton,
            CommonButton resumeButton,
            CommonButton returnButton,
            ExplorationLogFeedView logFeedView,
            Text filterText,
            Text helpText,
            ChronicleRuntimePanel chroniclePanel,
            ExplorationSettingsRuntimePanel settingsPanel)
        {
            _zoneFloorText = zoneFloorText;
            _goldText = goldText;
            _progressSlider = progressSlider;
            _progressText = progressText;
            _pauseButton = pauseButton;
            _resumeButton = resumeButton;
            _returnButton = returnButton;
            _logFeedView = logFeedView;
            _filterText = filterText;
            _helpText = helpText;
            _chroniclePanel = chroniclePanel;
            _settingsPanel = settingsPanel;
        }

        internal void BindShortcuts(System.Action refreshStatus)
        {
            if (_shortcuts == null)
                _shortcuts = gameObject.AddComponent<ExplorationHudShortcuts>();

            _settingsPanel?.Configure(refreshStatus);
            _shortcuts.Initialize(_logFeedView, _chroniclePanel, _settingsPanel, _filterText, refreshStatus);
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
            View.LogFeedView?.Show();
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
            if (state == null)
                return;

            if (View.ZoneFloorText != null)
            {
                View.ZoneFloorText.supportRichText = true;
                View.ZoneFloorText.text = ExplorationHudStatusFormatter.Build(state);
            }

            if (View.ProgressSlider != null)
            {
                View.ProgressSlider.gameObject.SetActive(state.IsExploring);
                View.ProgressSlider.value = Mathf.Clamp01(state.FloorProgress / 100f);
            }

            if (View.ProgressText != null)
            {
                View.ProgressText.gameObject.SetActive(state.IsExploring);
                View.ProgressText.text = $"{state.FloorProgress:0.#}%";
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
