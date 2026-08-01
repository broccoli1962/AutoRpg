using System;

namespace Backend.GameSystems.Offline
{
    /// <summary>
    /// spec.md 2.3 오프라인 진행 상수.
    /// </summary>
    public static class OfflinePolicy
    {
        public const int BaseCapHours = 4;
        public const int MaxInnBonusHours = 4;
        public const int MonthlyContractBonusHours = 4;
        public const int AbsoluteMaxCapHours = 12;

        public const double DefaultEfficiency = 0.7d;

        /// <summary>
        /// 온라인 대비 1초당 처치 수 (10초/처치).
        /// </summary>
        public const double OnlineKillsPerSecond = 0.1d;

        public static readonly TimeSpan BaseCap = TimeSpan.FromHours(BaseCapHours);
        public static readonly TimeSpan AbsoluteMaxCap = TimeSpan.FromHours(AbsoluteMaxCapHours);

        public const int MinHighlightCount = 3;
        public const int MaxHighlightCount = 5;

        /// <summary>
        /// 요약 모달을 띄울 최소 정산 경과 시간(초).
        /// </summary>
        public const double MinSummaryElapsedSeconds = 60d;
    }
}
