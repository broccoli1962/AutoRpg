using System;

namespace Backend.GameSystems.Offline
{
    /// <summary>
    /// 서버·로컬 시각 기반 오프라인 경과 시간 계산. 단말 시간 조작을 무효화한다.
    /// </summary>
    public static class OfflineTimeResolver
    {
        /// <summary>
        /// 정산 기준 현재 시각과 로컬 폴백 여부를 반환한다.
        /// </summary>
        public static DateTimeOffset ResolveCurrentTimeUtc(
            IServerTimeProvider serverTimeProvider,
            Func<DateTimeOffset> localUtcNow)
        {
            if (serverTimeProvider != null
                && serverTimeProvider.TryGetServerTimeUtc(out var serverTimeUtc))
            {
                return serverTimeUtc;
            }

            return localUtcNow != null ? localUtcNow() : DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// 마지막 정산 시각 대비 경과 시간을 계산한다.
        /// 로컬 폴백 사용 중 현재 시각이 과거로 되돌아간 경우 0을 반환한다.
        /// </summary>
        public static TimeSpan CalculateElapsed(
            DateTimeOffset lastSettlementUtc,
            DateTimeOffset currentTimeUtc,
            bool usedLocalFallback)
        {
            if (usedLocalFallback && currentTimeUtc < lastSettlementUtc)
                return TimeSpan.Zero;

            var elapsed = currentTimeUtc - lastSettlementUtc;
            return elapsed > TimeSpan.Zero ? elapsed : TimeSpan.Zero;
        }

        /// <summary>
        /// 현재 시각을 결정하고 경과 시간·로컬 폴백 여부를 함께 반환한다.
        /// </summary>
        public static OfflineElapsedSnapshot ResolveElapsed(
            DateTimeOffset lastSettlementUtc,
            IServerTimeProvider serverTimeProvider,
            Func<DateTimeOffset> localUtcNow)
        {
            var usedLocalFallback = true;
            DateTimeOffset currentTimeUtc;

            if (serverTimeProvider != null
                && serverTimeProvider.TryGetServerTimeUtc(out currentTimeUtc))
            {
                usedLocalFallback = false;
            }
            else
            {
                currentTimeUtc = localUtcNow != null ? localUtcNow() : DateTimeOffset.UtcNow;
            }

            var elapsed = CalculateElapsed(lastSettlementUtc, currentTimeUtc, usedLocalFallback);
            return new OfflineElapsedSnapshot(currentTimeUtc, elapsed, usedLocalFallback);
        }
    }

    /// <summary>
    /// 경과 시간 계산 결과 스냅샷.
    /// </summary>
    public readonly struct OfflineElapsedSnapshot
    {
        public OfflineElapsedSnapshot(
            DateTimeOffset currentTimeUtc,
            TimeSpan elapsed,
            bool usedLocalFallback)
        {
            CurrentTimeUtc = currentTimeUtc;
            Elapsed = elapsed;
            UsedLocalFallback = usedLocalFallback;
        }

        public DateTimeOffset CurrentTimeUtc { get; }
        public TimeSpan Elapsed { get; }
        public bool UsedLocalFallback { get; }
    }
}
