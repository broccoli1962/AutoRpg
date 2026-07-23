using Backend.GameSystems.Character;
using Backend.GameSystems.Exploration.Data;
using Backend.Object.UI;
using R3;
using UnityEngine;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// v2 좌측 파티 카드 4장 갱신. UI 구조는 프리팹에 고정.
    /// </summary>
    public sealed class PartyRuntimePanel : ExplorationHudSubview<PartyRuntimePresenter>
    {
        public static float PanelWidthPx => ExplorationHudLayoutMetrics.PartyMemberCardWidth;
    }

    public sealed class PartyRuntimePresenter : UIPresenter<PartyRuntimePanel>
    {
        private PartyMemberCardView[] _cards;
        private CompositeDisposable _disposables;

        public override void OnOpen()
        {
            BindCards();
            RelationshipSystem.EnsureInitialized();
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            ExplorationChannels.OnStateChanged
                .Subscribe(Refresh)
                .AddTo(_disposables);

            Refresh(ExplorationSystem.GetCurrentState());
        }

        public override void OnClose()
        {
            _disposables?.Dispose();
            _disposables = null;
        }

        private void BindCards()
        {
            var content = FindHudTransform("Body/PartyRow/PartyScroll/Viewport/Content");
            _cards = content == null
                ? View.GetComponentsInChildren<PartyMemberCardView>(true)
                : content.GetComponentsInChildren<PartyMemberCardView>(true);
        }

        private Transform FindHudTransform(string relativePath)
        {
            var hudPanel = View.GetComponent<ExplorationHudPanel>() ?? View.GetComponentInParent<ExplorationHudPanel>();
            return hudPanel == null ? null : hudPanel.transform.Find(relativePath);
        }

        private void Refresh(ExplorationState state)
        {
            if (_cards == null || _cards.Length == 0)
                return;

            var members = state?.Party?.Members;
            for (var i = 0; i < _cards.Length; i++)
            {
                var member = members != null && i < members.Count ? members[i] : null;
                _cards[i].Bind(member, i == 0);
            }
        }
    }
}
