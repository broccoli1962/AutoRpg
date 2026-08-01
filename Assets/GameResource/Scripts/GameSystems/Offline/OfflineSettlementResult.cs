using System;
using System.Collections.Generic;

namespace Backend.GameSystems.Offline
{
    /// <summary>
    /// 오프라인 정산 결과.
    /// </summary>
    public sealed class OfflineSettlementResult
    {
        public DateTimeOffset LastSettlementUtc { get; set; }
        public DateTimeOffset CurrentTimeUtc { get; set; }
        public TimeSpan RawElapsed { get; set; }
        public TimeSpan SettledDuration { get; set; }
        public TimeSpan Cap { get; set; }
        public bool UsedLocalFallback { get; set; }
        public bool TimeManipulationBlocked { get; set; }
        public long GoldReward { get; set; }
        public IReadOnlyList<string> Highlights { get; set; } = Array.Empty<string>();
        public bool ShouldShowSummary { get; set; }
        public bool AppliedToWallet { get; set; }

        /// <summary>
        /// 보상이 없는 최초 진입·짧은 부재 결과.
        /// </summary>
        public static OfflineSettlementResult Empty(DateTimeOffset currentTimeUtc)
        {
            return new OfflineSettlementResult
            {
                CurrentTimeUtc = currentTimeUtc,
                LastSettlementUtc = currentTimeUtc,
                RawElapsed = TimeSpan.Zero,
                SettledDuration = TimeSpan.Zero,
                Cap = OfflinePolicy.BaseCap,
            };
        }
    }
}
