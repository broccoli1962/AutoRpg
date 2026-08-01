using System;

namespace Backend.Meta.Shop
{
    /// <summary>
    /// 상점 구매·구독·소진 상태 세이브.
    /// </summary>
    [Serializable]
    public sealed class ShopSaveData
    {
        public string[] ConsumedOneTimeProductIds = Array.Empty<string>();
        public string[] FirstPurchaseBonusUsedProductIds = Array.Empty<string>();
        public string[] ProcessedTransactionIds = Array.Empty<string>();
        public DateTimeOffset MonthlyContractExpiryUtc;
        public int SubscriptionDailyPeriodKey;
        public bool HasPermanentAdRemoval;
    }
}
