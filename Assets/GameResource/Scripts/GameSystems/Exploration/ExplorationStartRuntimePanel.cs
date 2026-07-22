using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Prestige;
using Backend.Object.UI;
using Backend.Util;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// v2 길드 대기 StartCard — 프리팹 바인딩·탐험 시작 CTA.
    /// </summary>
    public sealed class ExplorationStartRuntimePanel : ExplorationOverlayView
    {
        private GameObject _startCardRoot;
        private Text _titleText;
        private Text _summaryText;
        private Button _startButton;
        private CompositeDisposable _disposables;
        private bool _guildExploreTabActive = true;

        private void Awake()
        {
            BindPrefab();
        }

        private void Start()
        {
            _disposables = new CompositeDisposable();

            ExplorationChannels.OnStateChanged
                .Subscribe(Refresh)
                .AddTo(_disposables);

            ExplorationChannels.OnExplorationEnded
                .Subscribe(_ => Refresh(ExplorationManager.GetCurrentState()))
                .AddTo(_disposables);

            Refresh(ExplorationManager.GetCurrentState());
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }

        private void BindPrefab()
        {
            var startCard = transform.Find("Body/CenterPanel/StartCard");
            if (startCard == null)
                return;

            _startCardRoot = startCard.gameObject;
            _titleText = startCard.Find("Title")?.GetComponent<Text>();
            _summaryText = startCard.Find("Summary")?.GetComponent<Text>();
            _startButton = startCard.Find("StartButton")?.GetComponent<Button>();

            if (_startButton != null)
            {
                _startButton.onClick.RemoveListener(OnStartClicked);
                _startButton.onClick.AddListener(OnStartClicked);
            }

            if (_titleText != null)
                ModernUiStyle.ApplyTitleLarge(_titleText);
        }

        public void SetGuildTabActive(bool isExploreTab)
        {
            _guildExploreTabActive = isExploreTab;
            Refresh(ExplorationManager.GetCurrentState());
        }

        private void Refresh(ExplorationState state)
        {
            if (_startCardRoot == null)
                return;

            var show = (state == null || !state.IsExploring) && _guildExploreTabActive;
            _startCardRoot.SetActive(show);

            if (!show)
                return;

            UpdateSummary(state);
        }

        private void UpdateSummary(ExplorationState state)
        {
            PrestigeManager.EnsureInitialized();
            var meta = PrestigeManager.GetMeta();
            var prestigeCount = Mathf.Max(1, (meta?.PrestigeCount ?? 0) + 1);
            var legacy = meta?.LegacyPoints ?? 0;
            var startingGold = PrestigeManager.GetStartingGoldBonus();
            var zoneName = ZoneDefinitions.GetZoneDisplayName(ZoneDefinitions.MossyHollowId);

            if (_summaryText != null)
            {
                _summaryText.text =
                    $"<b>제 {prestigeCount}회차 길드</b>\n" +
                    $"누적 유산 {legacy}\n" +
                    $"시작 골드 +{startingGold}\n\n" +
                    $"다음 목적지: {zoneName}\n" +
                    "준비가 끝나면 아래 버튼을 눌러 탐험을 시작하세요.";
            }
        }

        private void OnStartClicked()
        {
            ExplorationManager.BeginExplorationFromPlayer();
        }
    }
}
