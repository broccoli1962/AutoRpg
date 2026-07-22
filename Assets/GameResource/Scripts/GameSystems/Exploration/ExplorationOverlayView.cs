using Backend.Object.UI;
using UnityEngine;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// ExplorationHudPanel 하위 오버레이 View. UIView 계약을 따르되 별도 Canvas 루트를 토글한다.
    /// </summary>
    public abstract class ExplorationOverlayView : UIView
    {
        [SerializeField] private GameObject _overlayRoot;
        protected GameObject OverlayRoot => _overlayRoot != null ? _overlayRoot : CachedGameObject;
        protected bool IsOverlayVisible { get; private set; }

        /// <summary>오버레이 표시 여부.</summary>
        public bool IsVisible => IsOverlayVisible;

        /// <summary>오버레이를 표시한다.</summary>
        public new void Show()
        {
            OnBeforeShow();
            if (OverlayRoot != null)
                OverlayRoot.SetActive(true);

            IsOverlayVisible = true;
            OnShow();
        }

        /// <summary>오버레이를 숨긴다.</summary>
        public new void Hide()
        {
            if (OverlayRoot != null)
                OverlayRoot.SetActive(false);

            IsOverlayVisible = false;
            OnHide();
        }

        /// <summary>표시 직전 갱신 훅.</summary>
        protected virtual void OnBeforeShow() { }
    }
}
