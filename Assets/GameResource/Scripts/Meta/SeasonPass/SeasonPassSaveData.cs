using System;

namespace Backend.Meta.SeasonPass
{
    /// <summary>
    /// 시즌 패스 진행 세이브 스냅샷.
    /// </summary>
    [Serializable]
    public sealed class SeasonPassSaveData
    {
        public int SeasonNumber;
        public int SeasonPoints;
        public int DailyPointsEarned;
        public int DailyPeriodKey;
        public bool IsPremiumUnlocked;
        public int LastRewardedFloor;
        public int[] ClaimedFreeTierIndices = Array.Empty<int>();
        public int[] ClaimedPremiumTierIndices = Array.Empty<int>();
    }
}
