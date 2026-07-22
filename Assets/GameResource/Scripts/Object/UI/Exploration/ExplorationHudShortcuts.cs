using Backend.GameSystems.DynamicEvent;
using Backend.GameSystems.Exploration;
using Backend.Util;
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
            if (KeyboardInputUtil.WasKeyPressedThisFrame(KeyCode.L))
            {
                LlmQualitySettings.CycleMode();
                _refreshStatus?.Invoke();
            }

            if (KeyboardInputUtil.WasKeyPressedThisFrame(KeyCode.A))
            {
                DynamicEventAutoPolicySettings.CyclePolicy();
                _refreshStatus?.Invoke();
            }

            if (KeyboardInputUtil.WasKeyPressedThisFrame(KeyCode.G))
            {
                GoldenEventSettings.ToggleAutoPause();
                _refreshStatus?.Invoke();
            }

            if (DynamicEventManager.IsAwaitingManualChoice)
            {
                if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha1, KeyCode.Keypad1))
                    DynamicEventManager.TrySubmitManualChoice(0);

                if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha2, KeyCode.Keypad2))
                    DynamicEventManager.TrySubmitManualChoice(1);
            }

            if (KeyboardInputUtil.WasKeyPressedThisFrame(KeyCode.O))
                _settingsPanel?.Toggle();

            if (KeyboardInputUtil.WasKeyPressedThisFrame(KeyCode.C))
                _chroniclePanel?.Toggle();

            if (KeyboardInputUtil.WasKeyPressedThisFrame(KeyCode.R))
            {
                var state = ExplorationManager.GetCurrentState();
                if (state != null && state.IsExploring)
                    ExplorationManager.ReturnToGuild();
            }

            if (KeyboardInputUtil.WasKeyPressedThisFrame(KeyCode.F))
            {
                _logFeed?.CycleFilter();
                RefreshFilterLabel();
            }

            if (KeyboardInputUtil.WasKeyPressedThisFrame(KeyCode.B))
                _logFeed?.ToggleLastBookmark();

            if ((_chroniclePanel == null || !_chroniclePanel.IsVisible) &&
                (_settingsPanel == null || !_settingsPanel.IsVisible) &&
                KeyboardInputUtil.WasKeyPressedThisFrame(KeyCode.LeftBracket))
                _logFeed?.MovePage(older: true);

            if ((_chroniclePanel == null || !_chroniclePanel.IsVisible) &&
                (_settingsPanel == null || !_settingsPanel.IsVisible) &&
                KeyboardInputUtil.WasKeyPressedThisFrame(KeyCode.RightBracket))
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
