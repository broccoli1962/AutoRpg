namespace Backend.Meta.SeasonPass
{
    /// <summary>
    /// 시즌 패스 단계 보상 수령 결과.
    /// </summary>
    public readonly struct SeasonPassClaimResult
    {
        public bool Success { get; }
        public int TierIndex { get; }
        public bool IsPremiumTrack { get; }
        public string FailureReason { get; }

        private SeasonPassClaimResult(
            bool success,
            int tierIndex,
            bool isPremiumTrack,
            string failureReason)
        {
            Success = success;
            TierIndex = tierIndex;
            IsPremiumTrack = isPremiumTrack;
            FailureReason = failureReason;
        }

        /// <summary>
        /// 성공 결과를 생성한다.
        /// </summary>
        public static SeasonPassClaimResult Succeeded(int tierIndex, bool isPremiumTrack)
        {
            return new SeasonPassClaimResult(true, tierIndex, isPremiumTrack, null);
        }

        /// <summary>
        /// 실패 결과를 생성한다.
        /// </summary>
        public static SeasonPassClaimResult Failed(
            int tierIndex,
            bool isPremiumTrack,
            string reason)
        {
            return new SeasonPassClaimResult(false, tierIndex, isPremiumTrack, reason);
        }
    }

    /// <summary>
    /// 프리미엄 해금 및 소급 지급 결과.
    /// </summary>
    public readonly struct SeasonPassUnlockResult
    {
        public bool Success { get; }
        public int RetroactiveTierCount { get; }
        public string FailureReason { get; }

        private SeasonPassUnlockResult(bool success, int retroactiveTierCount, string failureReason)
        {
            Success = success;
            RetroactiveTierCount = retroactiveTierCount;
            FailureReason = failureReason;
        }

        /// <summary>
        /// 성공 결과를 생성한다.
        /// </summary>
        public static SeasonPassUnlockResult Succeeded(int retroactiveTierCount)
        {
            return new SeasonPassUnlockResult(true, retroactiveTierCount, null);
        }

        /// <summary>
        /// 실패 결과를 생성한다.
        /// </summary>
        public static SeasonPassUnlockResult Failed(string reason)
        {
            return new SeasonPassUnlockResult(false, 0, reason);
        }
    }

    /// <summary>
    /// 시즌 종료 우편함 이관 결과.
    /// </summary>
    public readonly struct SeasonPassEndMigrationResult
    {
        public int MigratedTierCount { get; }
        public int TotalRewardEntries { get; }

        public SeasonPassEndMigrationResult(int migratedTierCount, int totalRewardEntries)
        {
            MigratedTierCount = migratedTierCount;
            TotalRewardEntries = totalRewardEntries;
        }
    }
}
