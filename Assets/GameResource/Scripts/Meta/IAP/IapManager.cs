using Backend.Meta.Shop;
using Backend.Meta.Characters;
using Backend.Meta.Currency;
using Backend.Simulation;
using Backend.Util.Management;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Backend.Meta.IAP
{
    /// <summary>
    /// Unity IAP 런타임 매니저. 초기화·구매·복원·미지급 재처리 진입점.
    /// </summary>
    public sealed class IapManager : SingletonGameObject<IapManager>
    {
        private IapService _iapService;
        private ShopService _shopService;
        private bool _isBootstrapped;

        /// <summary>
        /// IAP 서비스를 반환한다.
        /// </summary>
        public IapService Service
        {
            get
            {
                EnsureBootstrapped();
                return _iapService;
            }
        }

        /// <summary>
        /// 상점 서비스를 반환한다.
        /// </summary>
        public ShopService Shop
        {
            get
            {
                EnsureBootstrapped();
                return _shopService;
            }
        }

        /// <summary>
        /// IAP 와 상점 서비스를 초기화한다.
        /// </summary>
        public async UniTask<bool> InitializeAsync(
            ShopService shopService = null,
            IPurchaseValidator validator = null,
            IIapStoreBridge storeBridge = null)
        {
            if (GameStateUtil.IsQuitting)
                return false;

            _shopService = shopService ?? BuildDefaultShopService();
            var catalog = ShopCatalogTableProvider.Get();

            validator ??= new LocalStubPurchaseValidator();
            storeBridge ??= CreateDefaultStoreBridge();

            _iapService = new IapService(_shopService, catalog, validator, storeBridge);
            _isBootstrapped = true;

            var initialized = await _iapService.InitializeStoreAsync();
            if (initialized)
                await _iapService.ProcessPendingTransactionsAsync();

            return initialized;
        }

        /// <summary>
        /// 상품 구매를 시작한다.
        /// </summary>
        public static UniTask<ShopPurchaseResult> PurchaseAsync(string productId)
        {
            if (GameStateUtil.IsQuitting || Instance == null)
                return UniTask.FromResult(ShopPurchaseResult.Failed(productId, "IAP manager unavailable."));

            return Instance.Service.PurchaseAsync(productId);
        }

        /// <summary>
        /// 구매를 복원한다.
        /// </summary>
        public static UniTask<ShopPurchaseResult[]> RestorePurchasesAsync()
        {
            if (GameStateUtil.IsQuitting || Instance == null)
                return UniTask.FromResult(System.Array.Empty<ShopPurchaseResult>());

            return Instance.Service.RestorePurchasesAsync();
        }

        /// <summary>
        /// 상점 서비스를 반환한다. 초기화 전이면 null.
        /// </summary>
        public static ShopService TryGetShop()
        {
            if (GameStateUtil.IsQuitting)
                return null;

            var manager = FindExistingInstance();
            return manager?._shopService;
        }

        private static IapManager FindExistingInstance()
        {
            var instances = UnityEngine.Object.FindObjectsByType<IapManager>(FindObjectsSortMode.None);
            return instances.Length > 0 ? instances[0] : null;
        }

        /// <summary>
        /// 미지급 트랜잭션을 재처리한다.
        /// </summary>
        public static UniTask<int> ProcessPendingTransactionsAsync()
        {
            if (GameStateUtil.IsQuitting || Instance == null)
                return UniTask.FromResult(0);

            return Instance.Service.ProcessPendingTransactionsAsync();
        }

        /// <summary>
        /// 테스트용 서비스를 주입한다.
        /// </summary>
        public static void SetForTests(IapService iapService, ShopService shopService)
        {
            if (Instance == null)
                return;

            Instance._iapService = iapService;
            Instance._shopService = shopService;
            Instance._isBootstrapped = iapService != null;
        }

        private void EnsureBootstrapped()
        {
            if (_iapService != null)
                return;

            if (!_isBootstrapped)
                InitializeAsync().Forget();
        }

        private static ShopService BuildDefaultShopService()
        {
            var ledger = new TransactionLedger();
            var wallet = new Wallet(ledger);
            var catalog = new ExplorerCatalog();
            var balance = BalanceTableProvider.Get();

            return new ShopService(wallet, catalog, balance);
        }

        private static IIapStoreBridge CreateDefaultStoreBridge()
        {
#if UNITY_EDITOR
            return new SimulatedIapStoreBridge();
#else
            var bridgeObject = new GameObject(nameof(UnityIapStoreBridge));
            Object.DontDestroyOnLoad(bridgeObject);
            return bridgeObject.AddComponent<UnityIapStoreBridge>();
#endif
        }
    }
}
