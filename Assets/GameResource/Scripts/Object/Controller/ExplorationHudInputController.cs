using Backend.GameSystems.DynamicEvent;
using Backend.GameSystems.Exploration;
using Backend.GameSystems.Exploration.Narration;
using Backend.GameSystems.LLM;
using Backend.Object.UI.Exploration;
using Backend.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Object.Controller
{
    /// <summary>
    /// Exploration HUD 전역·오버레이 키보드 입력 Controller.
    /// </summary>
    public sealed class ExplorationHudInputController : CachedMonobehaviour
    {
        private ExplorationLogFeedView _logFeed;
        private ChronicleRuntimePanel _chroniclePanel;
        private ExplorationSettingsRuntimePanel _settingsPanel;
        private EnhanceRuntimePanel _enhancePanel;
        private GuildFacilityRuntimePanel _guildPanel;
        private CharacterDetailRuntimePanel _characterDetailPanel;
        private TextMeshProUGUI _filterText;
        private System.Action _refreshStatus;

        /// <summary>입력 대상 View/Presenter 를 등록한다.</summary>
        public void Initialize(
            ExplorationLogFeedView logFeed,
            ChronicleRuntimePanel chroniclePanel,
            ExplorationSettingsRuntimePanel settingsPanel,
            EnhanceRuntimePanel enhancePanel,
            GuildFacilityRuntimePanel guildPanel,
            CharacterDetailRuntimePanel characterDetailPanel,
            TextMeshProUGUI filterText,
            System.Action refreshStatus)
        {
            _logFeed = logFeed;
            _chroniclePanel = chroniclePanel;
            _settingsPanel = settingsPanel;
            _enhancePanel = enhancePanel;
            _guildPanel = guildPanel;
            _characterDetailPanel = characterDetailPanel;
            _filterText = filterText;
            _refreshStatus = refreshStatus;
            RefreshFilterLabel();
        }

        private void Update()
        {
            if (GameStateUtil.IsQuitting)
                return;

            if (TryHandleOverlayInput())
                return;

            HandleGlobalShortcuts();
        }

        private bool TryHandleOverlayInput()
        {
            if (_characterDetailPanel != null && _characterDetailPanel.IsVisible)
            {
                _characterDetailPanel.HandleInput();
                return true;
            }

            if (_settingsPanel != null && _settingsPanel.IsVisible)
            {
                _settingsPanel.HandleInput();
                return true;
            }

            if (_enhancePanel != null && _enhancePanel.IsVisible)
            {
                _enhancePanel.HandleInput();
                return true;
            }

            if (_guildPanel != null && _guildPanel.IsVisible)
            {
                _guildPanel.HandleInput();
                return true;
            }

            if (_chroniclePanel != null && _chroniclePanel.IsVisible)
            {
                _chroniclePanel.HandleInput();
                return true;
            }

            return false;
        }

        private void HandleGlobalShortcuts()
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

            if (DynamicEventSystem.IsAwaitingManualChoice)
            {
                if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha1, KeyCode.Keypad1))
                    DynamicEventSystem.TrySubmitManualChoice(0);

                if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha2, KeyCode.Keypad2))
                    DynamicEventSystem.TrySubmitManualChoice(1);
            }

            if (KeyboardInputUtil.WasKeyPressedThisFrame(KeyCode.O))
                _settingsPanel?.Toggle();

            if (KeyboardInputUtil.WasKeyPressedThisFrame(KeyCode.C))
                _chroniclePanel?.Toggle();

            if (KeyboardInputUtil.WasKeyPressedThisFrame(KeyCode.I) && !IsOtherPanelVisible())
                _characterDetailPanel?.ShowMember(0);

            if (KeyboardInputUtil.WasKeyPressedThisFrame(KeyCode.R))
            {
                var state = ExplorationSystem.GetCurrentState();
                if (state != null && state.IsExploring)
                    ExplorationSystem.ReturnToGuild();
            }

            if (KeyboardInputUtil.WasKeyPressedThisFrame(KeyCode.F))
            {
                _logFeed?.CycleFilter();
                RefreshFilterLabel();
            }

            if (KeyboardInputUtil.WasKeyPressedThisFrame(KeyCode.B))
                _logFeed?.ToggleLastBookmark();

            if (!IsOtherPanelVisible() && KeyboardInputUtil.WasKeyPressedThisFrame(KeyCode.LeftBracket))
                _logFeed?.MovePage(older: true);

            if (!IsOtherPanelVisible() && KeyboardInputUtil.WasKeyPressedThisFrame(KeyCode.RightBracket))
                _logFeed?.MovePage(older: false);
        }

        private bool IsOtherPanelVisible()
        {
            if (_chroniclePanel != null && _chroniclePanel.IsVisible)
                return true;

            if (_settingsPanel != null && _settingsPanel.IsVisible)
                return true;

            if (_characterDetailPanel != null && _characterDetailPanel.IsVisible)
                return true;

            return false;
        }

        private void RefreshFilterLabel()
        {
            if (_filterText == null || _logFeed == null)
                return;

            _filterText.text = $"로그 필터: {LogFeedFilterUtil.GetDisplayLabel(_logFeed.CurrentFilter)} (F)";
        }
    }
}
