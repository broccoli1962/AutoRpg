using Backend.Meta.Currency;
using Backend.Util.Management;
using Cysharp.Threading.Tasks;

namespace Backend.Meta.Tutorial
{
    /// <summary>
    /// FTUE 튜토리얼 런타임 매니저.
    /// </summary>
    public sealed class TutorialManager : SingletonGameObject<TutorialManager>
    {
        private TutorialService _service;
        private TutorialEventBridge _eventBridge;
        private TutorialServiceGate _gate;
        private bool _isBootstrapped;

        /// <summary>
        /// 튜토리얼 서비스를 반환한다.
        /// </summary>
        public TutorialService Service
        {
            get
            {
                EnsureBootstrapped();
                return _service;
            }
        }

        /// <summary>
        /// 광고·상점 차단 게이트를 반환한다.
        /// </summary>
        public TutorialServiceGate Gate
        {
            get
            {
                EnsureBootstrapped();
                return _gate;
            }
        }

        /// <summary>
        /// 튜토리얼 서비스를 초기화한다.
        /// </summary>
        public UniTask<bool> InitializeAsync(
            TutorialService service = null,
            Wallet wallet = null,
            TutorialTable table = null,
            TutorialSaveData saveData = null)
        {
            if (GameStateUtil.IsQuitting)
                return UniTask.FromResult(false);

            wallet ??= MetaRuntimeProvider.Wallet;
            table ??= TutorialTableProvider.Get();

            _service = service ?? TutorialService.FromSaveData(saveData, wallet);
            _service.Bootstrap(table);
            _service.ResumeSession(table);

            _gate = new TutorialServiceGate(_service);
            _eventBridge?.Unsubscribe();
            _eventBridge = new TutorialEventBridge(_service, table);
            _eventBridge.Subscribe();

            _isBootstrapped = true;
            return UniTask.FromResult(true);
        }

        /// <summary>
        /// 튜토리얼 서비스를 반환한다. 초기화 전이면 null.
        /// </summary>
        public static TutorialService TryGetService()
        {
            if (GameStateUtil.IsQuitting)
                return null;

            var manager = FindExistingInstance();
            return manager?._service;
        }

        /// <summary>
        /// 튜토리얼 게이트를 반환한다. 초기화 전이면 null.
        /// </summary>
        public static TutorialServiceGate TryGetGate()
        {
            if (GameStateUtil.IsQuitting)
                return null;

            var manager = FindExistingInstance();
            return manager?._gate;
        }

        /// <summary>
        /// 테스트용 서비스를 주입한다.
        /// </summary>
        public static void SetForTests(
            TutorialService service,
            TutorialEventBridge eventBridge = null,
            TutorialServiceGate gate = null)
        {
            if (Instance == null)
                return;

            Instance._eventBridge?.Unsubscribe();
            Instance._service = service;
            Instance._gate = gate ?? (service != null ? new TutorialServiceGate(service) : null);
            Instance._eventBridge = eventBridge;
            Instance._isBootstrapped = service != null;
        }

        private static TutorialManager FindExistingInstance()
        {
            var instances = UnityEngine.Object.FindObjectsByType<TutorialManager>(UnityEngine.FindObjectsSortMode.None);
            return instances.Length > 0 ? instances[0] : null;
        }

        private void EnsureBootstrapped()
        {
            if (_service != null)
                return;

            if (!_isBootstrapped)
                InitializeAsync().Forget();
        }

        private void OnDestroy()
        {
            _eventBridge?.Unsubscribe();
        }
    }
}
