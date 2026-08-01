namespace Backend.Meta.Achievements
{
    /// <summary>
    /// 업적 단계 보상량을 결정한다.
    /// </summary>
    public interface IAchievementRewardResolver
    {
        /// <summary>
        /// 단계 정의에 대한 최종 심연석 보상량을 반환한다.
        /// </summary>
        long ResolveReward(AchievementTierDefinition tier);
    }
}
