using Backend.Object.UI;
using Backend.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// Phase 6 프로토타입 연대기(회차 회고록 + 즐겨찾기 순간 + 캐릭터별 일지) 런타임 패널.
    /// </summary>
    public sealed class ChronicleRuntimePanel : ExplorationOverlayView<ChronicleRuntimePresenter>
    {
        [SerializeField] private TextMeshProUGUI _contentText;
        [SerializeField] private TextMeshProUGUI _pageText;

        /// <summary>키보드 입력을 Presenter 에 위임한다.</summary>
        public void HandleInput() => ReadyPresenter.HandleKeyboardInput();

        public void Toggle()
        {
            if (IsVisible)
                Hide();
            else
                Show();
        }

        public void OpenTab(ChroniclePanelTab tab) => ReadyPresenter.OpenTab(tab);

        public void ClosePanel() => Hide();

        internal TextMeshProUGUI ContentText => _contentText;
        internal TextMeshProUGUI PageText => _pageText;

        public enum ChroniclePanelTab
        {
            Runs = ChronicleSystem.Tab.Runs,
            Favorites = ChronicleSystem.Tab.Favorites,
            CharacterJournal = ChronicleSystem.Tab.CharacterJournal,
            LoreCompendium = ChronicleSystem.Tab.LoreCompendium,
            MonsterCompendium = ChronicleSystem.Tab.MonsterCompendium
        }
    }

    public sealed class ChronicleRuntimePresenter : UIPresenter<ChronicleRuntimePanel>
    {
        private ChronicleSystem.Tab _tab = ChronicleSystem.Tab.Runs;
        private int _pageFromEnd;
        private int _characterIndex;

        public override void OnOpen()
        {
            _pageFromEnd = 0;
            RefreshContent();
        }

        /// <summary>키보드 입력을 처리한다.</summary>
        public void HandleKeyboardInput()
        {
            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha1, KeyCode.Keypad1))
                SelectTab(ChronicleSystem.Tab.Runs);

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha2, KeyCode.Keypad2))
                SelectTab(ChronicleSystem.Tab.Favorites);

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha3, KeyCode.Keypad3))
                SelectTab(ChronicleSystem.Tab.CharacterJournal);

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha4, KeyCode.Keypad4))
                SelectTab(ChronicleSystem.Tab.LoreCompendium);

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha5, KeyCode.Keypad5))
                SelectTab(ChronicleSystem.Tab.MonsterCompendium);

            if (_tab == ChronicleSystem.Tab.CharacterJournal &&
                KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Q, KeyCode.Comma))
                CycleCharacter(-1);

            if (_tab == ChronicleSystem.Tab.CharacterJournal &&
                KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.E, KeyCode.Period))
                CycleCharacter(1);

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.PageUp, KeyCode.LeftBracket))
                MovePage(older: true);

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.PageDown, KeyCode.RightBracket))
                MovePage(older: false);
        }

        /// <summary>지정 탭을 연다.</summary>
        public void OpenTab(ChronicleRuntimePanel.ChroniclePanelTab tab)
        {
            _tab = (ChronicleSystem.Tab)tab;
            _pageFromEnd = 0;
            View.Show();
        }

        private void SelectTab(ChronicleSystem.Tab tab)
        {
            _tab = tab;
            _pageFromEnd = 0;
            RefreshContent();
        }

        private void CycleCharacter(int delta)
        {
            var members = ExplorationSystem.GetCurrentState()?.Party?.Members;
            if (members == null || members.Count == 0)
                return;

            _characterIndex = (_characterIndex + delta + members.Count) % members.Count;
            _pageFromEnd = 0;
            RefreshContent();
        }

        private void MovePage(bool older)
        {
            var entryCount = ChronicleSystem.GetEntryCount(_tab, _characterIndex);
            var totalPages = ChronicleSystem.GetPageCount(entryCount);
            if (totalPages <= 1)
                return;

            if (older)
                _pageFromEnd = Mathf.Min(_pageFromEnd + 1, totalPages - 1);
            else
                _pageFromEnd = Mathf.Max(_pageFromEnd - 1, 0);

            RefreshContent();
        }

        private void RefreshContent()
        {
            var page = ChronicleSystem.BuildPage(_tab, _pageFromEnd, _characterIndex);
            if (View.ContentText != null)
                View.ContentText.text = page.ContentText;

            if (View.PageText != null)
                View.PageText.text = page.PageText;
        }
    }
}
