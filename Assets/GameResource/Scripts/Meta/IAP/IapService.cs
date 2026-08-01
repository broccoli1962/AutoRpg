using System;
using System.Collections.Generic;
using Backend.Meta.Shop;
using Cysharp.Threading.Tasks;

namespace Backend.Meta.IAP
{
    /// <summary>
    /// IAP 구매·검증·지급·복원·미지급 재처리를 조율한다.
    /// </summary>
    public sealed class IapService
    {
        private const string VALIDATION_FAILED = "Receipt validation failed.";
        private const string PRODUCT_NOT_FOUND = "Shop product not found for store id.";

        private readonly ShopService _shopService;
        private readonly ShopCatalogTable _catalog;
        private readonly IPurchaseValidator _validator;
        private readonly IIapStoreBridge _storeBridge;
        private readonly List<IapPendingTransaction> _pendingTransactions = new();

        public IapService(
            ShopService shopService,
            ShopCatalogTable catalog,
            IPurchaseValidator validator,
            IIapStoreBridge storeBridge)
        {
            _shopService = shopService ?? throw new ArgumentNullException(nameof(shopService));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _storeBridge = storeBridge ?? throw new ArgumentNullException(nameof(storeBridge));
        }

        /// <summary>
        /// 스토어 초기화 여부.
        /// </summary>
        public bool IsStoreInitialized => _storeBridge.IsInitialized;

        /// <summary>
        /// 검증 대기 트랜잭션 수.
        /// </summary>
        public int PendingTransactionCount => _pendingTransactions.Count;

        /// <summary>
        /// 스토어를 초기화한다.
        /// </summary>
        public UniTask<bool> InitializeStoreAsync()
        {
            var storeProductIds = CollectStoreProductIds();
            return _storeBridge.InitializeAsync(storeProductIds);
        }

        /// <summary>
        /// 상품 구매를 시작하고 검증·지급까지 처리한다.
        /// </summary>
        public async UniTask<ShopPurchaseResult> PurchaseAsync(string productId)
        {
            var product = _catalog.FindByProductId(productId);
            if (product == null)
                return ShopPurchaseResult.Failed(productId, PRODUCT_NOT_FOUND);

            if (!_shopService.CanPurchase(product))
                return ShopPurchaseResult.Failed(productId, "Product is not available for purchase.");

            var storeResult = await _storeBridge.PurchaseAsync(product.StoreProductId);
            if (!storeResult.Success)
                return ShopPurchaseResult.Failed(productId, storeResult.FailureReason);

            return await ProcessStorePurchaseAsync(product, storeResult);
        }

        /// <summary>
        /// iOS 등에서 구매를 복원한다.
        /// </summary>
        public async UniTask<ShopPurchaseResult[]> RestorePurchasesAsync()
        {
            var restoredStoreResults = await _storeBridge.RestorePurchasesAsync();
            var results = new List<ShopPurchaseResult>();

            foreach (var storeResult in restoredStoreResults)
            {
                if (!storeResult.Success)
                    continue;

                var product = _catalog.FindByStoreProductId(storeResult.StoreProductId);
                if (product == null)
                    continue;

                var result = await ProcessStorePurchaseAsync(product, storeResult);
                results.Add(result);
            }

            return results.ToArray();
        }

        /// <summary>
        /// 미지급 트랜잭션을 재처리한다.
        /// </summary>
        public async UniTask<int> ProcessPendingTransactionsAsync()
        {
            if (_pendingTransactions.Count == 0)
                return 0;

            var pendingCopy = _pendingTransactions.ToArray();
            var processedCount = 0;

            foreach (var pending in pendingCopy)
            {
                var product = _catalog.FindByStoreProductId(pending.StoreProductId);
                if (product == null)
                    continue;

                if (_shopService.IsTransactionProcessed(pending.TransactionId))
                {
                    RemovePendingInternal(pending.TransactionId);
                    _storeBridge.ConfirmPendingPurchase(pending.StoreProductId, pending.TransactionId);
                    continue;
                }

                var validation = await ValidateInternalAsync(pending);
                if (!validation.Success)
                    continue;

                var fulfillResult = _shopService.FulfillValidatedPurchase(
                    product,
                    pending.TransactionId,
                    validation.SubscriptionExpiryUtc);

                if (!fulfillResult.Success)
                    continue;

                RemovePendingInternal(pending.TransactionId);
                _storeBridge.ConfirmPendingPurchase(pending.StoreProductId, pending.TransactionId);
                processedCount++;
            }

            return processedCount;
        }

        /// <summary>
        /// IAP 세이브 스냅샷을 생성한다.
        /// </summary>
        public IapSaveData ToSaveData()
        {
            return new IapSaveData
            {
                PendingTransactions = _pendingTransactions.ToArray(),
            };
        }

        /// <summary>
        /// IAP 세이브 스냅샷을 복원한다.
        /// </summary>
        public void LoadSaveData(IapSaveData saveData)
        {
            _pendingTransactions.Clear();

            if (saveData?.PendingTransactions == null)
                return;

            foreach (var pending in saveData.PendingTransactions)
            {
                if (pending != null && !string.IsNullOrEmpty(pending.TransactionId))
                    _pendingTransactions.Add(pending);
            }
        }

        private async UniTask<ShopPurchaseResult> ProcessStorePurchaseAsync(
            ShopProductDefinition product,
            IapStorePurchaseResult storeResult)
        {
            if (_shopService.IsTransactionProcessed(storeResult.TransactionId))
            {
                _storeBridge.ConfirmPendingPurchase(storeResult.StoreProductId, storeResult.TransactionId);
                return ShopPurchaseResult.Succeeded(product.ProductId, 0);
            }

            var validation = await _validator.ValidateAsync(new PurchaseValidationRequest(
                storeResult.StoreProductId,
                storeResult.TransactionId,
                storeResult.Receipt,
                storeResult.Platform));

            if (!validation.Success)
            {
                QueuePendingInternal(storeResult);
                return ShopPurchaseResult.Failed(product.ProductId, validation.FailureReason ?? VALIDATION_FAILED);
            }

            var fulfillResult = _shopService.FulfillValidatedPurchase(
                product,
                storeResult.TransactionId,
                validation.SubscriptionExpiryUtc);

            if (!fulfillResult.Success)
                return fulfillResult;

            _storeBridge.ConfirmPendingPurchase(storeResult.StoreProductId, storeResult.TransactionId);
            return fulfillResult;
        }

        private UniTask<PurchaseValidationResult> ValidateInternalAsync(IapPendingTransaction pending)
        {
            return _validator.ValidateAsync(new PurchaseValidationRequest(
                pending.StoreProductId,
                pending.TransactionId,
                pending.Receipt,
                pending.Platform));
        }

        private void QueuePendingInternal(IapStorePurchaseResult storeResult)
        {
            if (string.IsNullOrEmpty(storeResult.TransactionId))
                return;

            foreach (var pending in _pendingTransactions)
            {
                if (pending.TransactionId == storeResult.TransactionId)
                    return;
            }

            _pendingTransactions.Add(new IapPendingTransaction
            {
                StoreProductId = storeResult.StoreProductId,
                TransactionId = storeResult.TransactionId,
                Receipt = storeResult.Receipt,
                Platform = storeResult.Platform,
                QueuedAtUtc = DateTimeOffset.UtcNow,
            });
        }

        private void RemovePendingInternal(string transactionId)
        {
            for (var i = _pendingTransactions.Count - 1; i >= 0; i--)
            {
                if (_pendingTransactions[i].TransactionId == transactionId)
                    _pendingTransactions.RemoveAt(i);
            }
        }

        private string[] CollectStoreProductIds()
        {
            if (_catalog.Products == null || _catalog.Products.Count == 0)
                return Array.Empty<string>();

            var ids = new List<string>();

            foreach (var product in _catalog.Products)
            {
                if (product != null && !string.IsNullOrEmpty(product.StoreProductId))
                    ids.Add(product.StoreProductId);
            }

            return ids.ToArray();
        }
    }
}
