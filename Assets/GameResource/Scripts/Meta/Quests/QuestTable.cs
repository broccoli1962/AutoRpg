using System;
using System.Collections.Generic;
using Backend.Meta.Currency;
using UnityEngine;

namespace Backend.Meta.Quests
{
    /// <summary>
    /// 일일 6종·주간 5종 퀘스트와 완료 상자 보상을 담는 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "QuestTable", menuName = "Abyss Chronicle/Quest Table")]
    public sealed class QuestTable : ScriptableObject
    {
        [SerializeField] private QuestDefinition[] _dailyQuests = Array.Empty<QuestDefinition>();
        [SerializeField] private QuestDefinition[] _weeklyQuests = Array.Empty<QuestDefinition>();
        [SerializeField] private QuestRewardEntry[] _dailyCompletionRewards = Array.Empty<QuestRewardEntry>();
        [SerializeField] private QuestRewardEntry[] _weeklyCompletionRewards = Array.Empty<QuestRewardEntry>();

        public IReadOnlyList<QuestDefinition> DailyQuests => _dailyQuests;
        public IReadOnlyList<QuestDefinition> WeeklyQuests => _weeklyQuests;
        public IReadOnlyList<QuestRewardEntry> DailyCompletionRewards => _dailyCompletionRewards;
        public IReadOnlyList<QuestRewardEntry> WeeklyCompletionRewards => _weeklyCompletionRewards;

        /// <summary>
        /// 기간별 퀘스트 정의를 조회한다.
        /// </summary>
        public IReadOnlyList<QuestDefinition> GetQuests(QuestPeriod period)
        {
            return period == QuestPeriod.Daily ? _dailyQuests : _weeklyQuests;
        }

        /// <summary>
        /// 퀘스트 ID로 정의를 조회한다.
        /// </summary>
        public QuestDefinition FindQuest(string questId)
        {
            if (string.IsNullOrEmpty(questId))
                return null;

            foreach (var quest in _dailyQuests)
            {
                if (quest != null && quest.QuestId == questId)
                    return quest;
            }

            foreach (var quest in _weeklyQuests)
            {
                if (quest != null && quest.QuestId == questId)
                    return quest;
            }

            return null;
        }

        /// <summary>
        /// spec 기본값으로 직렬화 필드를 채운다.
        /// </summary>
        public void ApplySpecDefaults()
        {
            _dailyQuests = new[]
            {
                CreateQuest("daily_login", QuestPeriod.Daily, QuestObjectiveType.Login, 1,
                    Reward(CurrencyType.Gold, 500)),
                CreateQuest("daily_kill_50", QuestPeriod.Daily, QuestObjectiveType.KillEnemies, 50,
                    Reward(CurrencyType.Gold, 1_000)),
                CreateQuest("daily_clear_5", QuestPeriod.Daily, QuestObjectiveType.ClearFloors, 5,
                    Reward(CurrencyType.Gold, 1_500)),
                CreateQuest("daily_upgrade_3", QuestPeriod.Daily, QuestObjectiveType.UpgradeEquipment, 3,
                    Reward(CurrencyType.ManaShard, 20)),
                CreateQuest("daily_summon_1", QuestPeriod.Daily, QuestObjectiveType.Summon, 1,
                    Reward(CurrencyType.AbyssStone, 5)),
                CreateQuest("daily_offline_1", QuestPeriod.Daily, QuestObjectiveType.CollectOfflineReward, 1,
                    Reward(CurrencyType.Gold, 800)),
            };

            _weeklyQuests = new[]
            {
                CreateQuest("weekly_kill_500", QuestPeriod.Weekly, QuestObjectiveType.KillEnemies, 500,
                    Reward(CurrencyType.Gold, 5_000)),
                CreateQuest("weekly_clear_50", QuestPeriod.Weekly, QuestObjectiveType.ClearFloors, 50,
                    Reward(CurrencyType.ManaShard, 100)),
                CreateQuest("weekly_summon_10", QuestPeriod.Weekly, QuestObjectiveType.Summon, 10,
                    Reward(CurrencyType.SummonTicket, 1)),
                CreateQuest("weekly_upgrade_20", QuestPeriod.Weekly, QuestObjectiveType.UpgradeEquipment, 20,
                    Reward(CurrencyType.RelicFragment, 50)),
                CreateQuest("weekly_daily_all_5", QuestPeriod.Weekly, QuestObjectiveType.CompleteAllDailyQuests, 5,
                    Reward(CurrencyType.AbyssStone, 30)),
            };

            _dailyCompletionRewards = new[]
            {
                Reward(CurrencyType.Gold, 5_000),
                Reward(CurrencyType.AbyssStone, 10),
            };

            _weeklyCompletionRewards = new[]
            {
                Reward(CurrencyType.Gold, 30_000),
                Reward(CurrencyType.SummonTicket, 1),
            };
        }

        private static QuestDefinition CreateQuest(
            string id,
            QuestPeriod period,
            QuestObjectiveType objective,
            int target,
            params QuestRewardEntry[] rewards)
        {
            return new QuestDefinition
            {
                QuestId = id,
                Period = period,
                ObjectiveType = objective,
                TargetCount = target,
                Rewards = rewards,
            };
        }

        private static QuestRewardEntry Reward(CurrencyType type, long amount)
        {
            return new QuestRewardEntry
            {
                CurrencyType = type,
                Amount = amount,
            };
        }
    }
}
