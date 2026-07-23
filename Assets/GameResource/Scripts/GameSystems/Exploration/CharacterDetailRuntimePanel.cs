using Backend.GameSystems.Character;
using Backend.GameSystems.Exploration.Data;
using Backend.Object.UI;
using Backend.Util;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 12_UIUX 캐릭터 카드 클릭 대응 — 스탯·장비·스킬·관계·기억 상세 패널.
    /// </summary>
    public sealed class CharacterDetailRuntimePanel : ExplorationOverlayView<CharacterDetailRuntimePresenter>
    {
        [SerializeField] private TextMeshProUGUI _contentText;
        [SerializeField] private TextMeshProUGUI _hintText;

        /// <summary>키보드 입력을 Presenter 에 위임한다.</summary>
        public void HandleInput() => ReadyPresenter.HandleUpdateInput();

        /// <summary>지정 인덱스 멤버 상세를 연다.</summary>
        public void ShowMember(int index) => ReadyPresenter.ShowMember(index);

        protected override void OnBeforeShow() => ReadyPresenter.PrepareShow();

        internal TextMeshProUGUI ContentText => _contentText;
        internal TextMeshProUGUI HintText => _hintText;
    }

    public sealed class CharacterDetailRuntimePresenter : UIPresenter<CharacterDetailRuntimePanel>
    {
        private int _memberIndex;
        private CompositeDisposable _disposables;

        protected override void OnAttached()
        {
            base.OnAttached();
            CharacterMemorySystem.EnsureInitialized();
        }

        public override void OnOpen()
        {
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();
            ExplorationChannels.OnStateChanged
                .Subscribe(_ => RefreshIfVisible())
                .AddTo(_disposables);
        }

        public override void OnClose()
        {
            _disposables?.Dispose();
            _disposables = null;
        }

        /// <summary>표시 직전 콘텐츠를 준비한다.</summary>
        public void PrepareShow()
        {
            var members = ExplorationSystem.GetCurrentState()?.Party?.Members;
            if (members == null || members.Count == 0)
                return;

            _memberIndex = Mathf.Clamp(_memberIndex, 0, members.Count - 1);
            RefreshContent(members[_memberIndex]);
        }

        /// <summary>Update 입력을 처리한다.</summary>
        public void HandleUpdateInput()
        {
            if (IsOtherPanelVisible())
                return;

            if (KeyboardInputUtil.WasKeyPressedThisFrame(KeyCode.I))
            {
                if (View.IsVisible)
                    View.Hide();
                else
                    ShowMember(0);
            }

            if (!View.IsVisible)
                return;

            if (KeyboardInputUtil.WasKeyPressedThisFrame(KeyCode.Escape))
            {
                View.Hide();
                return;
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Q, KeyCode.Comma))
                CycleMember(-1);

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.E, KeyCode.Period))
                CycleMember(1);
        }

        /// <summary>지정 인덱스 멤버 상세를 연다.</summary>
        public void ShowMember(int index)
        {
            var members = ExplorationSystem.GetCurrentState()?.Party?.Members;
            if (members == null || members.Count == 0)
                return;

            _memberIndex = Mathf.Clamp(index, 0, members.Count - 1);
            View.Show();
        }

        private void RefreshIfVisible()
        {
            if (!View.IsVisible)
                return;

            var members = ExplorationSystem.GetCurrentState()?.Party?.Members;
            if (members == null || members.Count == 0)
            {
                View.Hide();
                return;
            }

            _memberIndex = Mathf.Clamp(_memberIndex, 0, members.Count - 1);
            RefreshContent(members[_memberIndex]);
        }

        private void CycleMember(int delta)
        {
            var members = ExplorationSystem.GetCurrentState()?.Party?.Members;
            if (members == null || members.Count == 0)
                return;

            _memberIndex = (_memberIndex + delta + members.Count) % members.Count;
            RefreshContent(members[_memberIndex]);
        }

        private void RefreshContent(CharacterState member)
        {
            if (View.ContentText == null || member == null)
                return;

            var party = ExplorationSystem.GetCurrentState()?.Party;
            var members = party?.Members;
            var memberCount = members?.Count ?? 0;

            View.ContentText.text = ExplorationHudStatusFormatter.BuildCharacterDetailText(member, party);
            View.HintText.text = memberCount > 1
                ? $"I:닫기  Q/E:캐릭터 {_memberIndex + 1}/{memberCount}  Esc:닫기"
                : "I 또는 Esc:닫기";
        }

        private bool IsOtherPanelVisible()
        {
            if (View.GetComponent<ChronicleRuntimePanel>()?.IsVisible == true)
                return true;

            if (View.GetComponent<ExplorationSettingsRuntimePanel>()?.IsVisible == true)
                return true;

            if (View.GetComponent<EnhanceRuntimePanel>()?.IsVisible == true)
                return true;

            return View.GetComponent<GuildFacilityRuntimePanel>()?.IsVisible == true;
        }
    }
}
