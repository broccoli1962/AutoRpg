using System;

namespace Backend.Meta.Achievements
{
    /// <summary>
    /// 업적 추적 카테고리 7종.
    /// </summary>
    public enum AchievementCategory
    {
        TotalKills = 0,
        HighestFloor = 1,
        EquipmentUpgrades = 2,
        SummonCount = 3,
        CollectionCompletion = 4,
        PrestigeCount = 5,
        CompendiumEntries = 6,
    }

    /// <summary>
    /// 카테고리별 진행도 누적 방식.
    /// </summary>
    public enum AchievementProgressMode
    {
        Additive = 0,
        Maximum = 1,
        Percentage = 2,
    }

    /// <summary>
    /// 단일 업적 단계 정의.
    /// </summary>
    [Serializable]
    public sealed class AchievementTierDefinition
    {
        public string TierId;
        public int TierIndex;
        public long TargetValue;
        public long BaseAbyssStoneReward;
        public string RemoteConfigKey;
    }

    /// <summary>
    /// 카테고리별 다단계 업적 정의.
    /// </summary>
    [Serializable]
    public sealed class AchievementCategoryDefinition
    {
        public AchievementCategory Category;
        public string CategoryId;
        public AchievementProgressMode ProgressMode;
        public AchievementTierDefinition[] Tiers;
    }
}
