using System;

namespace Backend.GameSystems.Offline
{
    /// <summary>
    /// 여관 시설·월간 계약에 따른 오프라인 상한을 계산한다.
    /// </summary>
    public static class OfflineCapCalculator
    {
        /// <summary>
        /// 기본 4h + 여관(최대 4h) + 월간 계약(4h) → 최대 12h 상한을 반환한다.
        /// </summary>
        public static TimeSpan GetCap(int innFacilityLevel, bool hasActiveMonthlyContract)
        {
            var hours = OfflinePolicy.BaseCapHours;
            hours += Math.Min(Math.Max(innFacilityLevel, 0), OfflinePolicy.MaxInnBonusHours);
            if (hasActiveMonthlyContract)
                hours += OfflinePolicy.MonthlyContractBonusHours;

            hours = Math.Min(hours, OfflinePolicy.AbsoluteMaxCapHours);
            return TimeSpan.FromHours(hours);
        }
    }
}
