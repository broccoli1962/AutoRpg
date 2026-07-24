using Backend.Object.UI;
using Backend.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 하단 탭 '길드시설' — Prefab(ActionButtonBar) 버튼에 업그레이드 로직만 연결한다.
    /// </summary>
    public sealed class GuildFacilityRuntimePanel : ExplorationOverlayView<GuildFacilityRuntimePresenter>
    {
        [SerializeField] private TextMeshProUGUI _contentText;
        [SerializeField] private Button _scriptoriumButton;
        [SerializeField] private Button _trainingButton;
        [SerializeField] private Button _blacksmithButton;
        [SerializeField] private Button _innButton;
        [SerializeField] private Button _bookshopButton;

        public void Configure(System.Action onChanged)
        {
            EnsurePresenterReady();
            Presenter.Configure(onChanged);
        }

        public void HandleInput() => ReadyPresenter.HandleKeyboardInput();

        /// <summary>프리팹 ActionButtonBar 를 찾아 버튼 참조를 채운다.</summary>
        public void ResolvePrefabBindings()
        {
            var root = OverlayRoot != null ? OverlayRoot.transform : transform;
            var bar = root.Find("ActionButtonBar");
            if (bar == null)
                return;

            if (_scriptoriumButton == null)
                _scriptoriumButton = ResolveButton(bar, "서고", 0);
            if (_trainingButton == null)
                _trainingButton = ResolveButton(bar, "훈련소", 1);
            if (_blacksmithButton == null)
                _blacksmithButton = ResolveButton(bar, "대장간", 2);
            if (_innButton == null)
                _innButton = ResolveButton(bar, "여관", 3);
            if (_bookshopButton == null)
                _bookshopButton = ResolveButton(bar, "서점", 4);
        }

        private static Button ResolveButton(Transform bar, string childName, int siblingIndex)
        {
            var named = bar.Find(childName);
            if (named != null && named.TryGetComponent<Button>(out var byName))
                return byName;

            if (siblingIndex >= 0 && siblingIndex < bar.childCount &&
                bar.GetChild(siblingIndex).TryGetComponent<Button>(out var byIndex))
                return byIndex;

            return null;
        }

        internal TextMeshProUGUI ContentText => _contentText;
        internal Button ScriptoriumButton => _scriptoriumButton;
        internal Button TrainingButton => _trainingButton;
        internal Button BlacksmithButton => _blacksmithButton;
        internal Button InnButton => _innButton;
        internal Button BookshopButton => _bookshopButton;
    }

    public sealed class GuildFacilityRuntimePresenter : UIPresenter<GuildFacilityRuntimePanel>
    {
        private System.Action _onChanged;
        private string _lastFeedback;

        public void Configure(System.Action onChanged) => _onChanged = onChanged;

        public override void OnOpen()
        {
            View.ResolvePrefabBindings();
            Bind(View.ScriptoriumButton, () => Execute(ScriptoriumSystem.TryUpgrade));
            Bind(View.TrainingButton, () => Execute(TrainingGroundSystem.TryUpgrade));
            Bind(View.BlacksmithButton, () => Execute(BlacksmithSystem.TryUpgrade));
            Bind(View.InnButton, () => Execute(InnSystem.TryUpgrade));
            Bind(View.BookshopButton, () => Execute(BookshopSystem.TryUpgrade));
            RefreshContent();
        }

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

        private static void Bind(Button button, System.Action action)
        {
            if (button == null || action == null)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action());
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
            var ok = action(out var message);
            _lastFeedback = ok
                ? $"<color=#6EE08A>{message}</color>"
                : $"<color=#F26D5B>{message}</color>";
            Debug.Log($"[GuildFacilityRuntimePanel] {message}");
            RefreshContent();
            _onChanged?.Invoke();
        }

        private void RefreshContent()
        {
            if (View.ContentText == null)
                return;

            var body = ExplorationHudStatusFormatter.BuildFacilityPanelText();
            if (!string.IsNullOrEmpty(_lastFeedback))
                body = $"{body}\n\n{_lastFeedback}";

            View.ContentText.text = body;
        }

        private delegate bool TryActionDelegate(out string message);
    }
}
