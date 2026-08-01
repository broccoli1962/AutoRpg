using System;
using Cysharp.Threading.Tasks;

namespace Backend.Meta.IAP
{
    /// <summary>
    /// IAP 영수증 검증 요청.
    /// </summary>
    public readonly struct PurchaseValidationRequest
    {
        public string StoreProductId { get; }
        public string TransactionId { get; }
        public string Receipt { get; }
        public string Platform { get; }

        public PurchaseValidationRequest(
            string storeProductId,
            string transactionId,
            string receipt,
            string platform)
        {
            StoreProductId = storeProductId ?? string.Empty;
            TransactionId = transactionId ?? string.Empty;
            Receipt = receipt ?? string.Empty;
            Platform = platform ?? string.Empty;
        }
    }

    /// <summary>
    /// IAP 영수증 검증 결과.
    /// </summary>
    public readonly struct PurchaseValidationResult
    {
        public bool Success { get; }
        public string FailureReason { get; }
        public DateTimeOffset? SubscriptionExpiryUtc { get; }

        private PurchaseValidationResult(
            bool success,
            string failureReason,
            DateTimeOffset? subscriptionExpiryUtc)
        {
            Success = success;
            FailureReason = failureReason;
            SubscriptionExpiryUtc = subscriptionExpiryUtc;
        }

        /// <summary>
        /// 검증 성공 결과를 생성한다.
        /// </summary>
        public static PurchaseValidationResult Succeeded(DateTimeOffset? subscriptionExpiryUtc = null)
        {
            return new PurchaseValidationResult(true, null, subscriptionExpiryUtc);
        }

        /// <summary>
        /// 검증 실패 결과를 생성한다.
        /// </summary>
        public static PurchaseValidationResult Failed(string reason)
        {
            return new PurchaseValidationResult(false, reason, null);
        }
    }

    /// <summary>
    /// IAP 영수증 검증 인터페이스. 검증 성공 전에는 재화를 지급하지 않는다.
    /// </summary>
    public interface IPurchaseValidator
    {
        /// <summary>
        /// 영수증을 검증한다.
        /// </summary>
        UniTask<PurchaseValidationResult> ValidateAsync(PurchaseValidationRequest request);
    }
}
