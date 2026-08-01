using System;
using System.Collections.Generic;
using Backend.GameSystems.Offline;
using Backend.Meta.Characters;
using Backend.Meta.Currency;
using Backend.Meta.Retention;
using Backend.Meta.SeasonPass;
using Backend.Simulation;

namespace Backend.Meta.Shop
{
    /// <summary>
    /// 상점 카탈로그 조회·구매 지급·구독·소진 상태를 관리한다.
    /// </summary>
    public sealed class ShopService
    {
        private const string PRODUCT_NOT_FOUND = "Shop product not found.";
        private const string ALREADY_CONSUMED = "One-time product already consumed.";
        private const string FLOOR_NOT_REACHED = "Required floor not reached.";
        private const string DUPLICATE_TRANSACTION = "Transaction already processed.";
        private const string SUBSCRIPTION_DAILY_REASON = CurrencyReasonCodes.ShopSubscriptionDaily;

        private readonly Wallet _wallet;
        private readonly ExplorerCatalog _catalog;
        private readonly BalanceTable _balanceTable;
        private readonly IServerTimeProvider _serverTimeProvider;
        private readonly Func<DateTimeOffset> _localUtcNow;
        private readonly IShopPurchaseStateSync _purchaseStateSync;
        private readonly HashSet<string> _consumedOneTime = new();
        private readonly HashSet<string> _firstPurchaseBonusUsed = new();
        private readonly HashSet<string> _processedTransactions = new();

        private SeasonPassService _seasonPassService;
        private Func<int> _currentFloorProvider;
        private DateTimeOffset _monthlyContractExpiryUtc;
        private int _subscriptionDailyPeriodKey;
        private bool _hasPermanentAdRemoval;

        public ShopService(
            Wallet wallet,
            ExplorerCatalog catalog,
            BalanceTable balanceTable,
            IServerTimeProvider serverTimeProvider = null,
            Func<DateTimeOffset> localUtcNow = null,
            IShopPurchaseStateSync purchaseStateSync = null)
        {
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _balanceTable = balanceTable ?? throw new ArgumentNullException(nameof(balanceTable));
            _serverTimeProvider = serverTimeProvider;
            _localUtcNow = localUtcNow ?? (() => DateTimeOffset.UtcNow);
            _purchaseStateSync = purchaseStateSync ?? new LocalStubShopPurchaseStateSync();
        }

        /// <summary>
        /// 영구 광고 제거 보유 여부.
        /// </summary>
        public bool HasPermanentAdRemoval => _hasPermanentAdRemoval;

        /// <summary>
        /// 월간 심연 계약 활성 여부(서버 시간 기준).
        /// </summary>
        public bool HasActiveMonthlyContract => ResolveNowUtc() < _monthlyContractExpiryUtc;

        /// <summary>
        /// 전면 광고를 표시해야 하는지 여부.
        /// </summary>
        public bool ShouldShowInterstitialAds => !(_hasPermanentAdRemoval || HasActiveMonthlyContract);

        /// <summary>
        /// 시즌 패스 서비스를 연결한다.
        /// </summary>
        public void BindSeasonPassService(SeasonPassService seasonPassService)
        {
            _seasonPassService = seasonPassService;
        }

        /// <summary>
        /// 현재 층 조회자를 연결한다. 단계 성장 패키지 가용성 판정에 사용한다.
        /// </summary>
        public void BindCurrentFloorProvider(Func<int> currentFloorProvider)
        {
            _currentFloorProvider = currentFloorProvider;
        }

        /// <summary>
        /// 1회 한정 상품 소진 여부.
        /// </summary>
        public bool IsOneTimeProductConsumed(string productId)
        {
            return !string.IsNullOrEmpty(productId) && _consumedOneTime.Contains(productId);
        }

        /// <summary>
        /// 첫 구매 2배 보너스 사용 여부.
        /// </summary>
        public bool IsFirstPurchaseBonusUsed(string productId)
        {
            return !string.IsNullOrEmpty(productId) && _firstPurchaseBonusUsed.Contains(productId);
        }

        /// <summary>
        /// 트랜잭션이 이미 처리되었는지 확인한다.
        /// </summary>
        public bool IsTransactionProcessed(string transactionId)
        {
            return !string.IsNullOrEmpty(transactionId) && _processedTransactions.Contains(transactionId);
        }

        /// <summary>
        /// 상품 구매 가능 여부를 반환한다.
        /// </summary>
        public bool CanPurchase(ShopProductDefinition product)
        {
            if (product == null)
                return false;

            if (product.IsOneTimeLimited && IsOneTimeProductConsumed(product.ProductId))
                return false;

            if (product.Category == ShopProductCategory.TieredGrowthPack
                && product.RequiredFloor > 0
                && GetCurrentFloor() < product.RequiredFloor)
            {
                return false;
            }

            if (product.Category == ShopProductCategory.AdRemoval && _hasPermanentAdRemoval)
                return false;

            return true;
        }

        /// <summary>
        /// 검증 완료된 구매를 지급한다. 중복 트랜잭션은 거부한다.
        /// </summary>
        public ShopPurchaseResult FulfillValidatedPurchase(
            ShopProductDefinition product,
            string transactionId,
            DateTimeOffset? subscriptionExpiryUtc = null)
        {
            if (product == null)
                return ShopPurchaseResult.Failed(null, PRODUCT_NOT_FOUND);

            if (!string.IsNullOrEmpty(transactionId) && IsTransactionProcessed(transactionId))
                return ShopPurchaseResult.Failed(product.ProductId, DUPLICATE_TRANSACTION);

            if (product.IsOneTimeLimited && IsOneTimeProductConsumed(product.ProductId))
                return ShopPurchaseResult.Failed(product.ProductId, ALREADY_CONSUMED);

            if (product.Category == ShopProductCategory.TieredGrowthPack
                && product.RequiredFloor > 0
                && GetCurrentFloor() < product.RequiredFloor)
            {
                return ShopPurchaseResult.Failed(product.ProductId, FLOOR_NOT_REACHED);
            }

            var grantedCount = 0;

            if (product.HasFirstPurchaseBonus && !IsFirstPurchaseBonusUsed(product.ProductId))
            {
                var bonusAmount = product.BaseAbyssStoneAmount;
                if (bonusAmount > 0)
                {
                    _wallet.TryCredit(
                        CurrencyType.AbyssStone,
                        bonusAmount,
                        CurrencyReasonCodes.IapGrant);
                    grantedCount++;
                }

                _firstPurchaseBonusUsed.Add(product.ProductId);
            }

            if (product.Rewards != null)
            {
                foreach (var reward in product.Rewards)
                {
                    if (reward?.RewardType == ShopRewardType.Entitlement)
                        continue;

                    if (GrantRewardInternal(product, reward))
                        grantedCount++;
                }
            }

            ApplyEntitlementInternal(product, subscriptionExpiryUtc);

            if (product.IsOneTimeLimited)
                _consumedOneTime.Add(product.ProductId);

            if (!string.IsNullOrEmpty(transactionId))
                _processedTransactions.Add(transactionId);

            PersistStateInternal();
            return ShopPurchaseResult.Succeeded(product.ProductId, grantedCount);
        }

        /// <summary>
        /// 구독 일일 보상을 처리한다. KST 04:00 기준 1일 1회.
        /// </summary>
        public bool TryGrantSubscriptionDailyReward(ShopProductDefinition subscriptionProduct)
        {
            if (subscriptionProduct == null
                || subscriptionProduct.Category != ShopProductCategory.MonthlyAbyssContract
                || !HasActiveMonthlyContract)
            {
                return false;
            }

            RefreshSubscriptionDailyPeriodInternal();

            var periodKey = DailyResetClock.GetDayKey(ResolveNowUtc());
            if (_subscriptionDailyPeriodKey == periodKey)
                return false;

            var dailyAmount = subscriptionProduct.SubscriptionDailyAbyssStone;
            if (dailyAmount <= 0)
                return false;

            _wallet.TryCredit(
                CurrencyType.AbyssStone,
                dailyAmount,
                SUBSCRIPTION_DAILY_REASON);
            _subscriptionDailyPeriodKey = periodKey;
            PersistStateInternal();
            return true;
        }

        /// <summary>
        /// 서버 시간 기준 구독 만료 상태를 갱신한다.
        /// </summary>
        public void RefreshSubscriptionState()
        {
            if (_monthlyContractExpiryUtc != default
                && ResolveNowUtc() >= _monthlyContractExpiryUtc)
            {
                _monthlyContractExpiryUtc = default;
            }

            RefreshSubscriptionDailyPeriodInternal();
        }

        /// <summary>
        /// 세이브 스냅샷을 생성한다.
        /// </summary>
        public ShopSaveData ToSaveData()
        {
            return BuildSaveDataInternal();
        }

        /// <summary>
        /// 세이브 스냅샷에서 ShopService 를 복원한다.
        /// </summary>
        public static ShopService FromSaveData(
            ShopSaveData saveData,
            Wallet wallet,
            ExplorerCatalog catalog,
            BalanceTable balanceTable,
            IServerTimeProvider serverTimeProvider = null,
            Func<DateTimeOffset> localUtcNow = null,
            IShopPurchaseStateSync purchaseStateSync = null)
        {
            var service = new ShopService(
                wallet,
                catalog,
                balanceTable,
                serverTimeProvider,
                localUtcNow,
                purchaseStateSync);

            service.ApplySaveDataInternal(saveData);

            if (purchaseStateSync != null && purchaseStateSync.TryRestorePurchaseState(out var serverState))
                service.MergeServerStateInternal(serverState);

            service.RefreshSubscriptionState();
            return service;
        }

        private bool GrantRewardInternal(ShopProductDefinition product, ShopRewardEntry reward)
        {
            if (reward == null)
                return false;

            switch (reward.RewardType)
            {
                case ShopRewardType.Currency:
                    if (reward.Amount <= 0)
                        return false;

                    _wallet.TryCredit(
                        reward.CurrencyType,
                        reward.Amount,
                        BuildReasonCode(product));
                    return true;

                case ShopRewardType.Character:
                    if (string.IsNullOrEmpty(reward.CharacterId))
                        return false;

                    _catalog.TryAcquire(reward.CharacterId, reward.CharacterGrade, _balanceTable);
                    return true;

                case ShopRewardType.Entitlement:
                    return false;

                default:
                    return false;
            }
        }

        private void ApplyEntitlementInternal(
            ShopProductDefinition product,
            DateTimeOffset? subscriptionExpiryUtc)
        {
            if (product.Entitlement != ShopEntitlementType.None)
                ApplyEntitlementTypeInternal(product.Entitlement, subscriptionExpiryUtc);

            if (product.Rewards == null)
                return;

            foreach (var reward in product.Rewards)
            {
                if (reward?.RewardType == ShopRewardType.Entitlement)
                    ApplyEntitlementTypeInternal(reward.Entitlement, subscriptionExpiryUtc);
            }
        }

        private bool ApplyEntitlementTypeInternal(
            ShopEntitlementType entitlement,
            DateTimeOffset? subscriptionExpiryUtc)
        {
            switch (entitlement)
            {
                case ShopEntitlementType.PermanentAdRemoval:
                    _hasPermanentAdRemoval = true;
                    return true;

                case ShopEntitlementType.MonthlyContractActive:
                    var expiry = subscriptionExpiryUtc
                        ?? ResolveNowUtc().AddDays(30);
                    if (expiry > _monthlyContractExpiryUtc)
                        _monthlyContractExpiryUtc = expiry;
                    return true;

                case ShopEntitlementType.SeasonPassPremium:
                    return TryUnlockSeasonPassPremium();

                default:
                    return false;
            }
        }

        private bool TryUnlockSeasonPassPremium()
        {
            if (_seasonPassService == null)
                return false;

            return _seasonPassService.IsPremiumUnlocked
                || _seasonPassService.UnlockPremium(SeasonPassTableProvider.Get()).Success;
        }

        private int GetCurrentFloor()
        {
            return _currentFloorProvider?.Invoke() ?? 1;
        }

        private void RefreshSubscriptionDailyPeriodInternal()
        {
            var periodKey = DailyResetClock.GetDayKey(ResolveNowUtc());
            if (_subscriptionDailyPeriodKey != periodKey && !HasActiveMonthlyContract)
                _subscriptionDailyPeriodKey = periodKey;
        }

        private DateTimeOffset ResolveNowUtc()
        {
            if (_serverTimeProvider != null
                && _serverTimeProvider.TryGetServerTimeUtc(out var serverTime))
            {
                return serverTime;
            }

            return _localUtcNow();
        }

        private static string BuildReasonCode(ShopProductDefinition product)
        {
            return $"{CurrencyReasonCodes.IapGrant}:{product.ProductId}";
        }

        private void PersistStateInternal()
        {
            _purchaseStateSync.PersistPurchaseState(BuildSaveDataInternal());
        }

        private ShopSaveData BuildSaveDataInternal()
        {
            var consumed = new string[_consumedOneTime.Count];
            _consumedOneTime.CopyTo(consumed);

            var firstBonus = new string[_firstPurchaseBonusUsed.Count];
            _firstPurchaseBonusUsed.CopyTo(firstBonus);

            var transactions = new string[_processedTransactions.Count];
            _processedTransactions.CopyTo(transactions);

            return new ShopSaveData
            {
                ConsumedOneTimeProductIds = consumed,
                FirstPurchaseBonusUsedProductIds = firstBonus,
                ProcessedTransactionIds = transactions,
                MonthlyContractExpiryUtc = _monthlyContractExpiryUtc,
                SubscriptionDailyPeriodKey = _subscriptionDailyPeriodKey,
                HasPermanentAdRemoval = _hasPermanentAdRemoval,
            };
        }

        private void ApplySaveDataInternal(ShopSaveData saveData)
        {
            if (saveData == null)
                return;

            AddAll(_consumedOneTime, saveData.ConsumedOneTimeProductIds);
            AddAll(_firstPurchaseBonusUsed, saveData.FirstPurchaseBonusUsedProductIds);
            AddAll(_processedTransactions, saveData.ProcessedTransactionIds);
            _monthlyContractExpiryUtc = saveData.MonthlyContractExpiryUtc;
            _subscriptionDailyPeriodKey = saveData.SubscriptionDailyPeriodKey;
            _hasPermanentAdRemoval = saveData.HasPermanentAdRemoval;
        }

        private void MergeServerStateInternal(ShopSaveData serverState)
        {
            if (serverState == null)
                return;

            AddAll(_consumedOneTime, serverState.ConsumedOneTimeProductIds);
            AddAll(_firstPurchaseBonusUsed, serverState.FirstPurchaseBonusUsedProductIds);
            AddAll(_processedTransactions, serverState.ProcessedTransactionIds);

            if (serverState.MonthlyContractExpiryUtc > _monthlyContractExpiryUtc)
                _monthlyContractExpiryUtc = serverState.MonthlyContractExpiryUtc;

            if (serverState.HasPermanentAdRemoval)
                _hasPermanentAdRemoval = true;
        }

        private static void AddAll(HashSet<string> target, string[] values)
        {
            if (values == null)
                return;

            foreach (var value in values)
            {
                if (!string.IsNullOrEmpty(value))
                    target.Add(value);
            }
        }
    }
}
