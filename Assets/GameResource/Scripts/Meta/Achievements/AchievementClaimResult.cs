namespace Backend.Meta.Achievements
{
    /// <summary>
    /// 업적 단계 보상 수령 결과.
    /// </summary>
    public sealed class AchievementClaimResult
    {
        private AchievementClaimResult(
            bool success,
            string tierId,
            AchievementCategory category,
            long rewardAmount,
            string failureReason)
        {
            Success = success;
            TierId = tierId;
            Category = category;
            RewardAmount = rewardAmount;
            FailureReason = failureReason;
        }

        public bool Success { get; }
        public string TierId { get; }
        public AchievementCategory Category { get; }
        public long RewardAmount { get; }
        public string FailureReason { get; }

        /// <summary>
        /// 성공 결과를 생성한다.
        /// </summary>
        public static AchievementClaimResult Succeeded(
            string tierId,
            AchievementCategory category,
            long rewardAmount)
        {
            return new AchievementClaimResult(true, tierId, category, rewardAmount, null);
        }

        /// <summary>
        /// 실패 결과를 생성한다.
        /// </summary>
        public static AchievementClaimResult Failed(
            string tierId,
            AchievementCategory category,
            string reason)
        {
            return new AchievementClaimResult(false, tierId, category, 0L, reason);
        }
    }
}
