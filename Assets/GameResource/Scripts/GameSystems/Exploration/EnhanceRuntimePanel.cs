using Backend.Object.UI;
using Backend.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 하단 탭 '강화/장비' — Prefab(ActionButtonBar) 버튼에 로직만 연결한다.
    /// </summary>
    public sealed class EnhanceRuntimePanel : ExplorationOverlayView<EnhanceRuntimePresenter>
    {
        [SerializeField] private TextMeshProUGUI _contentText;
        [SerializeField] private Button _promoteButton;
        [SerializeField] private Button _enhanceWeaponButton;
        [SerializeField] private Button _enhanceArmorButton;

        public void HandleInput() => ReadyPresenter.HandleKeyboardInput();

        /// <summary>프리팹 ActionButtonBar 를 찾아 버튼 참조를 채운다.</summary>
        public void ResolvePrefabBindings()
        {
            var root = OverlayRoot != null ? OverlayRoot.transform : transform;
            var bar = root.Find("ActionButtonBar");
            if (bar == null)
            {
                Debug.LogError("[EnhanceRuntimePanel] ActionButtonBar missing in prefab.");
                return;
            }

            _promoteButton = ResolveButton(bar, _promoteButton, "전직", 0);
            _enhanceWeaponButton = ResolveButton(bar, _enhanceWeaponButton, "무기 강화", 1);
            _enhanceArmorButton = ResolveButton(bar, _enhanceArmorButton, "방어구 강화", 2);
        }

        private static Button ResolveButton(Transform bar, Button current, string childName, int siblingIndex)
        {
            if (current != null)
                return current;

            var named = bar.Find(childName);
            if (named != null && named.TryGetComponent<Button>(out var byName))
                return byName;

            if (siblingIndex >= 0 && siblingIndex < bar.childCount &&
                bar.GetChild(siblingIndex).TryGetComponent<Button>(out var byIndex))
                return byIndex;

            Debug.LogError($"[EnhanceRuntimePanel] Button '{childName}' (index {siblingIndex}) not found.");
            return null;
        }

        internal TextMeshProUGUI ContentText => _contentText;
        internal Button PromoteButton => _promoteButton;
        internal Button EnhanceWeaponButton => _enhanceWeaponButton;
        internal Button EnhanceArmorButton => _enhanceArmorButton;
    }

    public sealed class EnhanceRuntimePresenter : UIPresenter<EnhanceRuntimePanel>
    {
        private string _lastFeedback;

        public override void OnOpen()
        {
            View.ResolvePrefabBindings();
            Bind(View.PromoteButton, () => TryAndLog(CharacterTierSystem.TryPromoteLeader));
            Bind(View.EnhanceWeaponButton, () => TryAndLog(EquipmentEnhanceSystem.TryEnhanceLeaderWeapon));
            Bind(View.EnhanceArmorButton, () => TryAndLog(EquipmentEnhanceSystem.TryEnhanceLeaderArmor));
            RefreshContent();

            if (View.PromoteButton == null && View.EnhanceWeaponButton == null && View.EnhanceArmorButton == null)
                Debug.LogError("[EnhanceRuntimePanel] No action buttons bound — enhance tab will not respond to touch.");
        }

        public void HandleKeyboardInput()
        {
            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha1, KeyCode.Keypad1))
                TryAndLog(CharacterTierSystem.TryPromoteLeader);

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha2, KeyCode.Keypad2))
                TryAndLog(EquipmentEnhanceSystem.TryEnhanceLeaderWeapon);

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha3, KeyCode.Keypad3))
                TryAndLog(EquipmentEnhanceSystem.TryEnhanceLeaderArmor);
        }

        private static void Bind(Button button, System.Action action)
        {
            if (button == null || action == null)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action());
        }

        private void TryAndLog(TryActionDelegate action)
        {
            var ok = action(out var message);
            _lastFeedback = ok
                ? $"<color=#6EE08A>{message}</color>"
                : $"<color=#F26D5B>{message}</color>";
            Debug.Log($"[EnhanceRuntimePanel] {message}");
            RefreshContent();
        }

        private void RefreshContent()
        {
            if (View.ContentText == null)
                return;

            var body = ExplorationHudStatusFormatter.BuildEnhancePanelText();
            if (!string.IsNullOrEmpty(_lastFeedback))
                body = $"{body}\n\n{_lastFeedback}";

            View.ContentText.text = body;
        }

        private delegate bool TryActionDelegate(out string message);
    }
}
