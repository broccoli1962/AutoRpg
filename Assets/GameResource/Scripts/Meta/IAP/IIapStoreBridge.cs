using System;
using Cysharp.Threading.Tasks;

namespace Backend.Meta.IAP
{
    /// <summary>
    /// 스토어 구매 결과.
    /// </summary>
    public readonly struct IapStorePurchaseResult
    {
        public bool Success { get; }
        public string StoreProductId { get; }
        public string TransactionId { get; }
        public string Receipt { get; }
        public string Platform { get; }
        public string FailureReason { get; }
        public bool IsRestored { get; }

        private IapStorePurchaseResult(
            bool success,
            string storeProductId,
            string transactionId,
            string receipt,
            string platform,
            string failureReason,
            bool isRestored)
        {
            Success = success;
            StoreProductId = storeProductId;
            TransactionId = transactionId;
            Receipt = receipt;
            Platform = platform;
            FailureReason = failureReason;
            IsRestored = isRestored;
        }

        /// <summary>
        /// 구매 성공 결과를 생성한다.
        /// </summary>
        public static IapStorePurchaseResult Succeeded(
            string storeProductId,
            string transactionId,
            string receipt,
            string platform,
            bool isRestored = false)
        {
            return new IapStorePurchaseResult(
                true,
                storeProductId,
                transactionId,
                receipt,
                platform,
                null,
                isRestored);
        }

        /// <summary>
        /// 구매 실패 결과를 생성한다.
        /// </summary>
        public static IapStorePurchaseResult Failed(string storeProductId, string reason)
        {
            return new IapStorePurchaseResult(
                false,
                storeProductId,
                null,
                null,
                null,
                reason,
                false);
        }
    }

    /// <summary>
    /// Unity IAP·에디터 스텁 등 스토어 연동 추상화.
    /// </summary>
    public interface IIapStoreBridge
    {
        /// <summary>
        /// 스토어 초기화 완료 여부.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 스토어를 초기화한다.
        /// </summary>
        UniTask<bool> InitializeAsync(string[] storeProductIds);

        /// <summary>
        /// 상품 구매를 시작한다.
        /// </summary>
        UniTask<IapStorePurchaseResult> PurchaseAsync(string storeProductId);

        /// <summary>
        /// iOS 등에서 구매를 복원한다.
        /// </summary>
        UniTask<IapStorePurchaseResult[]> RestorePurchasesAsync();

        /// <summary>
        /// 처리 완료된 트랜잭션을 스토어에 확인한다.
        /// </summary>
        void ConfirmPendingPurchase(string storeProductId, string transactionId);
    }
}
