using System;

namespace Backend.Meta.Ads
{
    /// <summary>
    /// 광고 일일 상한·버프 상태 세이브 스냅샷.
    /// </summary>
    [Serializable]
    public sealed class AdSaveData
    {
        public int DailyPeriodKey;
        public int TotalRewardedToday;
        public int InterstitialDailyCount;
        public int InterstitialSessionCount;
        public int OfflineDoubleCount;
        public int InstantProgressCount;
        public int FreeSummonCount;
        public int InstantRetryCount;
        public int EnhancementBoostCount;
        public int EventReserveCount;
        public bool HasInstantRetryToken;
        public int EnhancementDiscountRemainingUses;
        public int EnhancementDiscountPercent;
    }
}
