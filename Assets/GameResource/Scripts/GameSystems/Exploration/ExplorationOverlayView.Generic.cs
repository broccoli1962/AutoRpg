using Backend.Object.UI;
using UnityEngine;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// MVP Presenter 와 결합된 Exploration 오버레이 View 베이스.
    /// Show/Hide 시 Presenter OnOpen/OnClose 가 UIView 계약대로 호출된다.
    /// </summary>
    public abstract class ExplorationOverlayView<TPresenter> : UIView<TPresenter>
        where TPresenter : UIPresenter, new()
    {
        [SerializeField] private GameObject _overlayRoot;
        protected GameObject OverlayRoot => _overlayRoot;
        public bool IsVisible { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            HideInitialOverlay();
        }

        /// <summary>오버레이를 표시한다.</summary>
        public new void Show()
        {
            if (_overlayRoot == null)
                return;

            OnBeforeShow();
            _overlayRoot.SetActive(true);
            IsVisible = true;
            OnShow();
        }

        /// <summary>오버레이를 숨긴다.</summary>
        public new void Hide()
        {
            OnHide();
            if (_overlayRoot != null)
                _overlayRoot.SetActive(false);

            IsVisible = false;
        }

        private void HideInitialOverlay()
        {
            IsVisible = false;
            if (_overlayRoot != null)
                _overlayRoot.SetActive(false);
        }

        /// <summary>표시 직전 갱신 훅.</summary>
        protected virtual void OnBeforeShow() { }
    }
}
