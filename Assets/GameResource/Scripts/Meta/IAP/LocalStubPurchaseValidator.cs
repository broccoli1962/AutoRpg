using System;
using Cysharp.Threading.Tasks;

namespace Backend.Meta.IAP
{
    /// <summary>
    /// 개발 빌드용 로컬 스텁 검증기. 서버 없이 항상 성공한다.
    /// </summary>
    public sealed class LocalStubPurchaseValidator : IPurchaseValidator
    {
        private readonly Func<DateTimeOffset> _utcNow;

        public LocalStubPurchaseValidator(Func<DateTimeOffset> utcNow = null)
        {
            _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// 영수증을 로컬에서 즉시 승인한다.
        /// </summary>
        public UniTask<PurchaseValidationResult> ValidateAsync(PurchaseValidationRequest request)
        {
            if (string.IsNullOrEmpty(request.StoreProductId))
                return UniTask.FromResult(PurchaseValidationResult.Failed("Store product id is empty."));

            if (string.IsNullOrEmpty(request.TransactionId))
                return UniTask.FromResult(PurchaseValidationResult.Failed("Transaction id is empty."));

            var expiry = request.StoreProductId.Contains("monthly_contract", StringComparison.Ordinal)
                ? _utcNow().AddDays(30)
                : (DateTimeOffset?)null;

            return UniTask.FromResult(PurchaseValidationResult.Succeeded(expiry));
        }
    }
}
