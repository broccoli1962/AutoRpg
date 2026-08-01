namespace Backend.Meta.Ads
{
    /// <summary>
    /// 보상형 광고 시청·보상 지급 결과.
    /// </summary>
    public readonly struct AdRewardResult
    {
        public bool Success { get; }
        public AdShowOutcome Outcome { get; }
        public RewardedAdPlacement Placement { get; }
        public string FailureReason { get; }
        public long GrantedGold { get; }
        public long GrantedSummonTickets { get; }

        private AdRewardResult(
            bool success,
            AdShowOutcome outcome,
            RewardedAdPlacement placement,
            string failureReason,
            long grantedGold,
            long grantedSummonTickets)
        {
            Success = success;
            Outcome = outcome;
            Placement = placement;
            FailureReason = failureReason;
            GrantedGold = grantedGold;
            GrantedSummonTickets = grantedSummonTickets;
        }

        /// <summary>
        /// 보상 지급 성공 결과를 생성한다.
        /// </summary>
        public static AdRewardResult Succeeded(
            RewardedAdPlacement placement,
            long grantedGold = 0L,
            long grantedSummonTickets = 0L)
        {
            return new AdRewardResult(
                true,
                AdShowOutcome.Completed,
                placement,
                null,
                grantedGold,
                grantedSummonTickets);
        }

        /// <summary>
        /// 광고 미완료·차단 결과를 생성한다. 게임 진행은 막히지 않는다.
        /// </summary>
        public static AdRewardResult NotRewarded(
            RewardedAdPlacement placement,
            AdShowOutcome outcome,
            string failureReason = null)
        {
            return new AdRewardResult(
                false,
                outcome,
                placement,
                failureReason,
                0L,
                0L);
        }
    }
}
