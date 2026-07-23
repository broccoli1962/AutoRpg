using Backend.Object.UI;
using Backend.Util;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// ExplorationHudPanel 하위 상시 바인딩 View. UIManager 비관리, Presenter 로 비즈니스 로직 분리.
    /// </summary>
    public abstract class ExplorationHudSubview<TPresenter> : CachedMonobehaviour
        where TPresenter : UIPresenter, new()
    {
        protected TPresenter Presenter { get; private set; }

        /// <summary>외부 호출 시 Presenter 가 항상 준비된 상태를 보장한다.</summary>
        protected TPresenter ReadyPresenter
        {
            get
            {
                EnsurePresenterReady();
                return Presenter;
            }
        }

        /// <summary>Awake 이전 접근 시 Presenter 를 지연 생성한다.</summary>
        internal void EnsurePresenterReady()
        {
            if (Presenter != null)
                return;

            Presenter = new TPresenter();
            Presenter.AttachView(this);
        }

        protected virtual void Awake()
        {
            EnsurePresenterReady();
        }

        protected virtual void Start()
        {
            ReadyPresenter.OnOpen();
        }

        protected virtual void OnDestroy()
        {
            if (GameStateUtil.IsQuitting)
                return;

            Presenter?.OnClose();
        }
    }
}
