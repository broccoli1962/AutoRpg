using Backend.Object.UI;
using Backend.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 하단 탭 '강화/장비' — 전직·장비 강화 (09_성장과경제.md).
    /// </summary>
    public sealed class EnhanceRuntimePanel : ExplorationOverlayView<EnhanceRuntimePresenter>
    {
        [SerializeField] private TextMeshProUGUI _contentText;

        public void HandleInput() => ReadyPresenter.HandleKeyboardInput();

        internal TextMeshProUGUI ContentText => _contentText;
    }

    public sealed class EnhanceRuntimePresenter : UIPresenter<EnhanceRuntimePanel>
    {
        public override void OnOpen() => RefreshContent();

        public void HandleKeyboardInput()
        {
            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha1, KeyCode.Keypad1))
                TryAndLog(CharacterTierSystem.TryPromoteLeader);

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha2, KeyCode.Keypad2))
                TryAndLog(EquipmentEnhanceSystem.TryEnhanceLeaderWeapon);

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha3, KeyCode.Keypad3))
                TryAndLog(EquipmentEnhanceSystem.TryEnhanceLeaderArmor);
        }

        private void TryAndLog(TryActionDelegate action)
        {
            action(out var message);
            Debug.Log($"[EnhanceRuntimePanel] {message}");
            RefreshContent();
        }

        private void RefreshContent()
        {
            if (View.ContentText != null)
                View.ContentText.text = ExplorationHudStatusFormatter.BuildEnhancePanelText();
        }

        private delegate bool TryActionDelegate(out string message);
    }
}
