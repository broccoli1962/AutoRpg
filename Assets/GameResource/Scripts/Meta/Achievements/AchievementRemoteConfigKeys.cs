namespace Backend.Meta.Achievements
{
    /// <summary>
    /// Firebase Remote Config 등과 매핑할 업적 보상 키 상수.
    /// </summary>
    public static class AchievementRemoteConfigKeys
    {
        public const string GlobalRewardMultiplier = "achievement_reward_multiplier_global";

        public static string TierReward(string categoryId, int tierIndex) =>
            $"achievement_reward_{categoryId}_tier_{tierIndex}";
    }
}
