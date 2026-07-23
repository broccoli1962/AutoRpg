using Backend.Object.UI;
using Backend.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 하단 탭 '길드시설' — 시설 레벨 요약 및 업그레이드 (12_UIUX.md).
    /// </summary>
    public sealed class GuildFacilityRuntimePanel : ExplorationOverlayView<GuildFacilityRuntimePresenter>
    {
        [SerializeField] private TextMeshProUGUI _contentText;

        public void Configure(System.Action onChanged)
        {
            EnsurePresenterReady();
            Presenter.Configure(onChanged);
        }

        public void HandleInput() => ReadyPresenter.HandleKeyboardInput();

        internal TextMeshProUGUI ContentText => _contentText;
    }

    public sealed class GuildFacilityRuntimePresenter : UIPresenter<GuildFacilityRuntimePanel>
    {
        private System.Action _onChanged;

        public void Configure(System.Action onChanged) => _onChanged = onChanged;

        public override void OnOpen() => RefreshContent();

        public void HandleKeyboardInput()
        {
            if (TryExecute(KeyCode.Alpha6, KeyCode.Keypad6, ScriptoriumSystem.TryUpgrade))
                return;

            if (TryExecute(KeyCode.Alpha7, KeyCode.Keypad7, TrainingGroundSystem.TryUpgrade))
                return;

            if (TryExecute(KeyCode.Alpha8, KeyCode.Keypad8, BlacksmithSystem.TryUpgrade))
                return;

            if (TryExecute(KeyCode.Alpha9, KeyCode.Keypad9, InnSystem.TryUpgrade))
                return;

            if (TryExecute(KeyCode.Alpha0, KeyCode.Keypad0, BookshopSystem.TryUpgrade))
                return;

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Minus, KeyCode.KeypadMinus))
                Execute(SkillTreeSystem.TryUpgradeLeaderRole);
        }

        private bool TryExecute(KeyCode primary, KeyCode keypad, TryActionDelegate action)
        {
            if (!KeyboardInputUtil.WasAnyKeyPressedThisFrame(primary, keypad))
                return false;

            Execute(action);
            return true;
        }

        private void Execute(TryActionDelegate action)
        {
            action(out var message);
            Debug.Log($"[GuildFacilityRuntimePanel] {message}");
            RefreshContent();
            _onChanged?.Invoke();
        }

        private void RefreshContent()
        {
            if (View.ContentText != null)
                View.ContentText.text = ExplorationHudStatusFormatter.BuildFacilityPanelText();
        }

        private delegate bool TryActionDelegate(out string message);
    }
}
