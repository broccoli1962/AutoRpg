using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

#if UNITY_PURCHASING
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
#endif

namespace Backend.Meta.IAP
{
    /// <summary>
    /// Unity IAP 기반 스토어 연동.
    /// </summary>
    public sealed class UnityIapStoreBridge :
#if UNITY_PURCHASING
        MonoBehaviour,
        IStoreListener,
#endif
        IIapStoreBridge
    {
#if UNITY_PURCHASING
        private IStoreController _storeController;
        private IExtensionProvider _extensionProvider;
#endif
        private readonly Queue<IapStorePurchaseResult> _completedPurchases = new();
        private UniTaskCompletionSource<bool> _initializeCompletion;
        private UniTaskCompletionSource<IapStorePurchaseResult> _purchaseCompletion;
        private UniTaskCompletionSource<IapStorePurchaseResult[]> _restoreCompletion;

        public bool IsInitialized { get; private set; }

#if UNITY_PURCHASING
        /// <summary>
        /// MonoBehaviour 라이프사이클에서 DontDestroyOnLoad 를 적용한다.
        /// </summary>
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Unity IAP 를 초기화한다.
        /// </summary>
        public async UniTask<bool> InitializeAsync(string[] storeProductIds)
        {
            if (IsInitialized)
                return true;

            _initializeCompletion = new UniTaskCompletionSource<bool>();

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            if (storeProductIds != null)
            {
                foreach (var productId in storeProductIds)
                {
                    if (string.IsNullOrEmpty(productId))
                        continue;

                    builder.AddProduct(productId, ResolveProductType(productId));
                }
            }

            UnityPurchasing.Initialize(this, builder);
            return await _initializeCompletion.Task;
        }

        /// <summary>
        /// 상품 구매를 시작한다.
        /// </summary>
        public async UniTask<IapStorePurchaseResult> PurchaseAsync(string storeProductId)
        {
            if (!IsInitialized || _storeController == null)
                return IapStorePurchaseResult.Failed(storeProductId, "Store is not initialized.");

            _purchaseCompletion = new UniTaskCompletionSource<IapStorePurchaseResult>();
            _storeController.InitiatePurchase(storeProductId);
            return await _purchaseCompletion.Task;
        }

        /// <summary>
        /// iOS 등에서 구매를 복원한다.
        /// </summary>
        public async UniTask<IapStorePurchaseResult[]> RestorePurchasesAsync()
        {
            if (!IsInitialized || _extensionProvider == null)
                return Array.Empty<IapStorePurchaseResult>();

            _restoreCompletion = new UniTaskCompletionSource<IapStorePurchaseResult[]>();
            _completedPurchases.Clear();

            var appleExtensions = _extensionProvider.GetExtension<IAppleExtensions>();
            if (appleExtensions != null)
            {
                appleExtensions.RestoreTransactions(OnRestoreFinished);
                return await _restoreCompletion.Task;
            }

            var googleExtensions = _extensionProvider.GetExtension<IGooglePlayStoreExtensions>();
            if (googleExtensions != null)
            {
                googleExtensions.RestoreTransactions(OnRestoreFinished);
                return await _restoreCompletion.Task;
            }

            return Array.Empty<IapStorePurchaseResult>();
        }

        /// <summary>
        /// 처리 완료된 트랜잭션을 확인한다.
        /// </summary>
        public void ConfirmPendingPurchase(string storeProductId, string transactionId)
        {
            if (_storeController == null)
                return;

            if (string.IsNullOrEmpty(storeProductId))
                return;

            _storeController.ConfirmPendingPurchase(storeProductId);
        }

        /// <summary>
        /// Unity IAP 초기화 성공 콜백.
        /// </summary>
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _storeController = controller;
            _extensionProvider = extensions;
            IsInitialized = true;
            _initializeCompletion?.TrySetResult(true);
        }

        /// <summary>
        /// Unity IAP 초기화 실패 콜백.
        /// </summary>
        public void OnInitializeFailed(InitializationFailureReason error)
        {
            IsInitialized = false;
            _initializeCompletion?.TrySetResult(false);
        }

        /// <summary>
        /// Unity IAP 초기화 실패 콜백(상세).
        /// </summary>
        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            IsInitialized = false;
            _initializeCompletion?.TrySetResult(false);
        }

        /// <summary>
        /// Unity IAP 구매 처리 콜백. 검증은 IapService 가 담당하므로 Pending 으로 반환한다.
        /// </summary>
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            var product = purchaseEvent.purchasedProduct;
            var result = IapStorePurchaseResult.Succeeded(
                product.definition.id,
                product.transactionID,
                product.receipt,
                Application.platform.ToString());

            if (_restoreCompletion != null)
            {
                _completedPurchases.Enqueue(result);
                return PurchaseProcessingResult.Pending;
            }

            _purchaseCompletion?.TrySetResult(result);
            return PurchaseProcessingResult.Pending;
        }

        /// <summary>
        /// Unity IAP 구매 실패 콜백.
        /// </summary>
        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            var productId = product?.definition?.id;
            _purchaseCompletion?.TrySetResult(
                IapStorePurchaseResult.Failed(productId, failureReason.ToString()));
        }

        private void OnRestoreFinished(bool success, string error)
        {
            if (!success)
            {
                _restoreCompletion?.TrySetResult(Array.Empty<IapStorePurchaseResult>());
                return;
            }

            var restored = _completedPurchases.ToArray();
            _completedPurchases.Clear();
            _restoreCompletion?.TrySetResult(restored);
        }

        private static ProductType ResolveProductType(string storeProductId)
        {
            if (storeProductId.Contains("monthly_contract", StringComparison.Ordinal))
                return ProductType.Subscription;

            if (storeProductId.Contains("abyss_stone", StringComparison.Ordinal))
                return ProductType.Consumable;

            return ProductType.NonConsumable;
        }
#else
        /// <summary>
        /// UNITY_PURCHASING 미정의 시 초기화 실패.
        /// </summary>
        public UniTask<bool> InitializeAsync(string[] storeProductIds)
        {
            return UniTask.FromResult(false);
        }

        /// <summary>
        /// UNITY_PURCHASING 미정의 시 구매 실패.
        /// </summary>
        public UniTask<IapStorePurchaseResult> PurchaseAsync(string storeProductId)
        {
            return UniTask.FromResult(
                IapStorePurchaseResult.Failed(storeProductId, "Unity Purchasing is unavailable."));
        }

        /// <summary>
        /// UNITY_PURCHASING 미정의 시 빈 배열 반환.
        /// </summary>
        public UniTask<IapStorePurchaseResult[]> RestorePurchasesAsync()
        {
            return UniTask.FromResult(Array.Empty<IapStorePurchaseResult>());
        }

        /// <summary>
        /// UNITY_PURCHASING 미정의 시 no-op.
        /// </summary>
        public void ConfirmPendingPurchase(string storeProductId, string transactionId)
        {
        }
#endif
    }
}
