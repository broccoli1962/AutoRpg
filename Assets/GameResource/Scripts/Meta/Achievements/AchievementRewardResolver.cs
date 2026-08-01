namespace Backend.Meta.Achievements
{
    /// <summary>
    /// AchievementTable 기본값과 Remote Config 오버레이를 합쳐 보상량을 계산한다.
    /// </summary>
    public sealed class AchievementRewardResolver : IAchievementRewardResolver
    {
        private readonly AchievementRemoteConfigOverlay _overlay;

        public AchievementRewardResolver(AchievementRemoteConfigOverlay overlay = null)
        {
            _overlay = overlay ?? new AchievementRemoteConfigOverlay();
        }

        /// <summary>
        /// 연결된 Remote Config 오버레이를 반환한다.
        /// </summary>
        public AchievementRemoteConfigOverlay Overlay => _overlay;

        /// <summary>
        /// 단계 정의에 대한 최종 심연석 보상량을 반환한다.
        /// </summary>
        public long ResolveReward(AchievementTierDefinition tier)
        {
            return _overlay.ResolveReward(tier);
        }
    }
}
