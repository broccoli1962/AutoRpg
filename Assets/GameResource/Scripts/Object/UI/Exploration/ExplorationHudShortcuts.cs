using Backend.GameSystems.DynamicEvent;
using Backend.GameSystems.Exploration;
using Backend.GameSystems.Exploration.Narration;
using Backend.GameSystems.LLM;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Object.UI.Exploration
{
    /// <summary>
    /// Addressable ExplorationHudPanel용 RuntimeHud 단축키(L/A/G/C/R/F/B/[/]) 입력.
    /// </summary>
    public sealed class ExplorationHudShortcuts : MonoBehaviour
    {
        private ExplorationLogFeedView _logFeed;
        private ChronicleRuntimePanel _chroniclePanel;
        private ExplorationSettingsRuntimePanel _settingsPanel;
        private Text _filterText;
        private System.Action _refreshStatus;

        public void Initialize(
            ExplorationLogFeedView logFeed,
            ChronicleRuntimePanel chroniclePanel,
            ExplorationSettingsRuntimePanel settingsPanel,
            Text filterText,
            System.Action refreshStatus)
        {
            _logFeed = logFeed;
            _chroniclePanel = chroniclePanel;
            _settingsPanel = settingsPanel;
            _filterText = filterText;
            _refreshStatus = refreshStatus;
            RefreshFilterLabel();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                LlmQualitySettings.CycleMode();
                _refreshStatus?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                DynamicEventAutoPolicySettings.CyclePolicy();
                _refreshStatus?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                GoldenEventSettings.ToggleAutoPause();
                _refreshStatus?.Invoke();
            }

            if (DynamicEventManager.IsAwaitingManualChoice)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                    DynamicEventManager.TrySubmitManualChoice(0);

                if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                    DynamicEventManager.TrySubmitManualChoice(1);
            }

            if (Input.GetKeyDown(KeyCode.O))
                _settingsPanel?.Toggle();

            if (Input.GetKeyDown(KeyCode.C))
                _chroniclePanel?.Toggle();

            if (Input.GetKeyDown(KeyCode.R))
            {
                var state = ExplorationManager.GetCurrentState();
                if (state != null && state.IsExploring)
                    ExplorationManager.ReturnToGuild();
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                _logFeed?.CycleFilter();
                RefreshFilterLabel();
            }

            if (Input.GetKeyDown(KeyCode.B))
                _logFeed?.ToggleLastBookmark();

            if ((_chroniclePanel == null || !_chroniclePanel.IsVisible) &&
                (_settingsPanel == null || !_settingsPanel.IsVisible) &&
                Input.GetKeyDown(KeyCode.LeftBracket))
                _logFeed?.MovePage(older: true);

            if ((_chroniclePanel == null || !_chroniclePanel.IsVisible) &&
                (_settingsPanel == null || !_settingsPanel.IsVisible) &&
                Input.GetKeyDown(KeyCode.RightBracket))
                _logFeed?.MovePage(older: false);
        }

        private void RefreshFilterLabel()
        {
            if (_filterText == null || _logFeed == null)
                return;

            _filterText.text = $"로그 필터: {LogFeedFilterUtil.GetDisplayLabel(_logFeed.CurrentFilter)} (F)";
        }
    }
}
