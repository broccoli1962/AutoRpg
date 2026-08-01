using Backend.GameSystems.Offline;
using Backend.Meta;
using Backend.Meta.Currency;
using Backend.Meta.IAP;
using Backend.Meta.Shop;
using Backend.Simulation;
using Backend.Util.Management;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Backend.Meta.Ads
{
    /// <summary>
    /// 광고 SDK·일일 상한·보상 지급 런타임 매니저.
    /// </summary>
    public sealed class AdManager : SingletonGameObject<AdManager>
    {
        private AdRewardService _rewardService;
        private ShopService _shopService;
        private bool _isBootstrapped;

        /// <summary>
        /// 광고 보상 서비스를 반환한다.
        /// </summary>
        public AdRewardService Service
        {
            get
            {
                EnsureBootstrapped();
                return _rewardService;
            }
        }

        /// <summary>
        /// 광고 서비스를 초기화한다.
        /// </summary>
        public async UniTask<bool> InitializeAsync(
            AdRewardService rewardService = null,
            ShopService shopService = null,
            IAdService adService = null)
        {
            if (GameStateUtil.IsQuitting)
                return false;

            _shopService = shopService ?? IapManager.TryGetShop() ?? BuildDefaultShopService();
            var config = AdConfigTableProvider.Get();
            adService ??= CreateDefaultAdService();

            if (rewardService == null)
            {
                var grantor = new AdRewardGrantor(
                    MetaRuntimeProvider.Wallet,
                    BalanceTableProvider.Get(),
                    () => OfflineRuntimeProvider.Service.CurrentFloor);

                rewardService = new AdRewardService(
                    adService,
                    config,
                    grantor,
                    shouldShowInterstitialAds: () => _shopService == null || _shopService.ShouldShowInterstitialAds);
            }

            _rewardService = rewardService;
            _isBootstrapped = true;
            _rewardService.ResetSessionCounters();

            return await _rewardService.InitializeAsync();
        }

        /// <summary>
        /// 보상형 광고를 시청한다.
        /// </summary>
        public static UniTask<AdRewardResult> ShowRewardedAsync(RewardedAdPlacement placement)
        {
            if (GameStateUtil.IsQuitting || Instance == null)
            {
                return UniTask.FromResult(AdRewardResult.NotRewarded(
                    placement,
                    AdShowOutcome.Failed,
                    "Ad manager unavailable."));
            }

            return Instance.Service.TryShowRewardedAsync(placement);
        }

        /// <summary>
        /// 전면 광고를 시도한다.
        /// </summary>
        public static UniTask<AdShowOutcome> ShowInterstitialAsync(InterstitialTrigger trigger)
        {
            if (GameStateUtil.IsQuitting || Instance == null)
                return UniTask.FromResult(AdShowOutcome.Failed);

            return Instance.Service.TryShowInterstitialAsync(trigger);
        }

        /// <summary>
        /// 테스트용 서비스를 주입한다.
        /// </summary>
        public static void SetForTests(AdRewardService rewardService, ShopService shopService = null)
        {
            if (Instance == null)
                return;

            Instance._rewardService = rewardService;
            Instance._shopService = shopService;
            Instance._isBootstrapped = rewardService != null;
        }

        private void EnsureBootstrapped()
        {
            if (_rewardService != null)
                return;

            if (!_isBootstrapped)
                InitializeAsync().Forget();
        }

        private static IAdService CreateDefaultAdService()
        {
#if UNITY_EDITOR
            return new SimulatedAdService();
#else
            var bridgeObject = new GameObject(nameof(AdMobAdService));
            Object.DontDestroyOnLoad(bridgeObject);
            return bridgeObject.AddComponent<AdMobAdService>();
#endif
        }

        private static ShopService BuildDefaultShopService()
        {
            var ledger = new TransactionLedger();
            var wallet = new Wallet(ledger);
            var catalog = new Characters.ExplorerCatalog();
            var balance = BalanceTableProvider.Get();
            return new ShopService(wallet, catalog, balance);
        }

    }
}
