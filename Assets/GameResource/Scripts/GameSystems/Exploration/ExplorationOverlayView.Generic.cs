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
            EnsurePresenterReady();
            if (_overlayRoot == null)
            {
                Debug.LogError($"[{GetType().Name}] _overlayRoot is null — wire Overlays/*Panel in prefab.");
                return;
            }

            OnBeforeShow();
            SetSharedDimVisible(true);
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
            if (!AnySiblingOverlayVisible())
                SetSharedDimVisible(false);
        }

        private void SetSharedDimVisible(bool visible)
        {
            var overlays = transform.Find("Overlays");
            if (overlays == null)
                overlays = _overlayRoot != null ? _overlayRoot.transform.parent : null;
            if (overlays == null)
                return;

            var dim = overlays.Find("DimBackdrop");
            if (dim == null)
                return;

            dim.gameObject.SetActive(visible);
            if (!visible)
                return;

            if (!dim.TryGetComponent<UnityEngine.UI.Button>(out var button))
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(Hide);
        }

        private bool AnySiblingOverlayVisible()
        {
            var overlays = _overlayRoot != null ? _overlayRoot.transform.parent : transform.Find("Overlays");
            if (overlays == null)
                return false;

            for (var i = 0; i < overlays.childCount; i++)
            {
                var child = overlays.GetChild(i);
                if (child.name == "DimBackdrop")
                    continue;
                if (child.gameObject.activeSelf)
                    return true;
            }

            return false;
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
