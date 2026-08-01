using System;
using Backend.Meta.Currency;

namespace Backend.Meta.SeasonPass
{
    /// <summary>
    /// 시즌 포인트 획득 경로.
    /// </summary>
    public enum SeasonPointSource
    {
        DailyQuestComplete = 0,
        WeeklyQuestComplete = 1,
        FloorReached = 2,
        BossKill = 3,
    }

    /// <summary>
    /// 시즌 정의.
    /// </summary>
    [Serializable]
    public sealed class SeasonDefinition
    {
        public int SeasonNumber;
        public long StartUtcTicks;
        public long EndUtcTicks;
    }

    /// <summary>
    /// 시즌 포인트 획득량과 일일 상한.
    /// </summary>
    [Serializable]
    public sealed class SeasonPassPointConfig
    {
        public int DailyQuestCompletePoints = 40;
        public int WeeklyQuestCompletePoints = 120;
        public int FloorReachedPoints = 15;
        public int BossKillPoints = 25;
        public int DailyEarnCap = 300;
    }

    /// <summary>
    /// 단일 재화 보상 항목.
    /// </summary>
    [Serializable]
    public struct SeasonPassRewardEntry
    {
        public CurrencyType CurrencyType;
        public long Amount;
    }

    /// <summary>
    /// 시즌 패스 단계 정의.
    /// </summary>
    [Serializable]
    public sealed class SeasonPassTierDefinition
    {
        public int TierIndex;
        public int RequiredPoints;
        public SeasonPassRewardEntry[] FreeRewards = Array.Empty<SeasonPassRewardEntry>();
        public SeasonPassRewardEntry[] PremiumRewards = Array.Empty<SeasonPassRewardEntry>();
    }
}
