using Backend.GameSystems.Exploration;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Prestige;
using Backend.Object.Controller;
using Backend.Object.UI.Exploration;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Object.UI
{
    public class ExplorationHudPanel : UIPanel<ExplorationHudPresenter>
    {
        public override UILayer Layer => UILayer.HUD;

        [Header("Status")]
        [SerializeField] private TextMeshProUGUI _zoneFloorText;
        [SerializeField] private TextMeshProUGUI _goldText;
        [SerializeField] private Slider _progressSlider;
        [SerializeField] private TextMeshProUGUI _progressText;

        [Header("Controls")]
        [SerializeField] private CommonButton _pauseButton;
        [SerializeField] private CommonButton _resumeButton;
        [SerializeField] private CommonButton _returnButton;

        [Header("Views")]
        [SerializeField] private ExplorationLogFeedView _logFeedView;
        [SerializeField] private TextMeshProUGUI _filterText;
        [SerializeField] private TextMeshProUGUI _helpText;

        internal void BindControllers(System.Action refreshStatus)
        {
            var chroniclePanel = GetComponentInChildren<ChronicleRuntimePanel>(true);
            var settingsPanel = GetComponentInChildren<ExplorationSettingsRuntimePanel>(true);
            var enhancePanel = GetComponentInChildren<EnhanceRuntimePanel>(true);
            var guildPanel = GetComponentInChildren<GuildFacilityRuntimePanel>(true);
            var characterDetailPanel = GetComponentInChildren<CharacterDetailRuntimePanel>(true);
            var startPanel = GetComponentInChildren<ExplorationStartRuntimePanel>(true);
            var centerPanel = GetComponentInChildren<ExplorationCenterRuntimePanel>(true);
            var stagePanel = EnsureStagePanel();
            var partyPanel = GetComponentInChildren<PartyRuntimePanel>(true);

            chroniclePanel?.EnsurePresenterReady();
            settingsPanel?.EnsurePresenterReady();
            enhancePanel?.EnsurePresenterReady();
            guildPanel?.EnsurePresenterReady();
            characterDetailPanel?.EnsurePresenterReady();
            startPanel?.EnsurePresenterReady();
            centerPanel?.EnsurePresenterReady();
            stagePanel?.EnsurePresenterReady();
            partyPanel?.EnsurePresenterReady();

            settingsPanel?.Configure(refreshStatus);
            guildPanel?.Configure(refreshStatus);

            EnsureComponent<ExplorationHudInputController>()
                .Initialize(_logFeedView, chroniclePanel, settingsPanel, enhancePanel, guildPanel, characterDetailPanel, _filterText, refreshStatus);

            EnsureComponent<GuildHudTabController>()
                .Initialize(chroniclePanel, enhancePanel, guildPanel, refreshStatus);
        }

        private ExplorationStageRuntimePanel EnsureStagePanel()
        {
            var existing = GetComponentInChildren<ExplorationStageRuntimePanel>(true);
            if (existing != null)
                return existing;

            var exploreContent = transform.Find("Body/CenterPanel/ExploreContent");
            if (exploreContent == null)
                return null;

            return exploreContent.gameObject.AddComponent<ExplorationStageRuntimePanel>();
        }

        private T EnsureComponent<T>() where T : Component
        {
            if (!TryGetComponent<T>(out var component))
                component = gameObject.AddComponent<T>();

            return component;
        }

        public TextMeshProUGUI ZoneFloorText => _zoneFloorText;
        public TextMeshProUGUI GoldText => _goldText;
        public Slider ProgressSlider => _progressSlider;
        public TextMeshProUGUI ProgressText => _progressText;
        public CommonButton PauseButton => _pauseButton;
        public CommonButton ResumeButton => _resumeButton;
        public CommonButton ReturnButton => _returnButton;
        public ExplorationLogFeedView LogFeedView => _logFeedView;
        public TextMeshProUGUI FilterText => _filterText;
    }

    public class ExplorationHudPresenter : UIPresenter<ExplorationHudPanel>
    {
        private CompositeDisposable _disposables;

        public override void OnOpen()
        {
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            PrestigeManager.EnsureInitialized();
            ExplorationSystem.EnsureRuntime();
            View.BindControllers(() => RefreshState(ExplorationSystem.GetCurrentState()));
            ExplorationHudLayoutApplier.ApplyStageFirstLayout(View.transform, ExplorationSystem.GetCurrentState()?.IsExploring == true);
            BindControls();
            RefreshState(ExplorationSystem.GetCurrentState());

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
                    .Subscribe(_ => ExplorationSystem.PauseExploration())
                    .AddTo(_disposables);
            }

            if (View.ResumeButton != null)
            {
                View.ResumeButton.OnClickAsObservable()
                    .Subscribe(_ => ExplorationSystem.ResumeExploration())
                    .AddTo(_disposables);
            }

            if (View.ReturnButton != null)
            {
                View.ReturnButton.OnClickAsObservable()
                    .Subscribe(_ => ExplorationSystem.ReturnToGuild())
                    .AddTo(_disposables);
            }
        }

        private void RefreshState(ExplorationState state)
        {
            var isWaiting = state == null || !state.IsExploring;
            ExplorationHudLayoutApplier.ApplyStageFirstLayout(View.transform, !isWaiting);

            if (View.ZoneFloorText != null)
            {
                View.ZoneFloorText.richText = true;
                if (isWaiting)
                {
                    View.ZoneFloorText.text =
                        "<color=#F5D673>길드 대기</color>\n<size=20><color=#9AA8BC>하단 탭에서 시설·강화·연대기를 확인하세요</color></size>";
                }
                else
                {
                    View.ZoneFloorText.text = ExplorationHudStatusFormatter.BuildTopResourceBar(state);
                }
            }

            if (View.GoldText != null)
            {
                View.GoldText.gameObject.SetActive(!isWaiting);
                if (!isWaiting)
                {
                    View.GoldText.richText = true;
                    View.GoldText.text = ExplorationHudStatusFormatter.BuildExplorationLine(state);
                }
            }

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
            state ??= ExplorationSystem.GetCurrentState();
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
