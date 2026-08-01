using System;
using Backend.Meta.Currency;

namespace Backend.Meta.Quests
{
    /// <summary>
    /// 퀘스트 갱신 주기.
    /// </summary>
    public enum QuestPeriod
    {
        Daily = 0,
        Weekly = 1,
    }

    /// <summary>
    /// 퀘스트 목표 유형.
    /// </summary>
    public enum QuestObjectiveType
    {
        Login = 0,
        KillEnemies = 1,
        ClearFloors = 2,
        UpgradeEquipment = 3,
        Summon = 4,
        CollectOfflineReward = 5,
        CompleteAllDailyQuests = 6,
    }

    /// <summary>
    /// 단일 재화 보상 항목.
    /// </summary>
    [Serializable]
    public struct QuestRewardEntry
    {
        public CurrencyType CurrencyType;
        public long Amount;
    }

    /// <summary>
    /// 퀘스트 정의 한 건.
    /// </summary>
    [Serializable]
    public sealed class QuestDefinition
    {
        public string QuestId;
        public QuestPeriod Period;
        public QuestObjectiveType ObjectiveType;
        public int TargetCount;
        public QuestRewardEntry[] Rewards;
    }
}
