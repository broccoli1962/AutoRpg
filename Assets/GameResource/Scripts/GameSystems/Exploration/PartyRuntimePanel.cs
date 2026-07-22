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
    public sealed class PartyRuntimePanel : ExplorationOverlayView
    {
        private PartyMemberCardView[] _cards;
        private CompositeDisposable _disposables;

        public static float PanelWidthPx => ExplorationHudLayoutMetrics.PartyMemberCardWidth;

        private void Start()
        {
            BindCards();
            RelationshipManager.EnsureInitialized();
            _disposables = new CompositeDisposable();

            ExplorationChannels.OnStateChanged
                .Subscribe(Refresh)
                .AddTo(_disposables);

            Refresh(ExplorationManager.GetCurrentState());
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }

        private void BindCards()
        {
            var content = FindHudTransform("Body/PartyRow/PartyScroll/Viewport/Content");
            if (content == null)
            {
                _cards = GetComponentsInChildren<PartyMemberCardView>(true);
                return;
            }

            _cards = content.GetComponentsInChildren<PartyMemberCardView>(true);
        }

        private Transform FindHudTransform(string relativePath)
        {
            var hudPanel = GetComponent<ExplorationHudPanel>() ?? GetComponentInParent<ExplorationHudPanel>();
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
