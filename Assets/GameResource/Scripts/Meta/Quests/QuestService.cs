using System;
using System.Collections.Generic;
using Backend.GameSystems.Offline;
using Backend.Meta.Currency;
using Backend.Meta.Retention;

namespace Backend.Meta.Quests
{
    /// <summary>
    /// 일일·주간 퀘스트 진행도 추적, 보상 수령, 완료 상자를 담당한다.
    /// </summary>
    public sealed class QuestService
    {
        private const string QUEST_NOT_FOUND = "Quest not found.";
        private const string QUEST_NOT_COMPLETE = "Quest is not complete.";
        private const string QUEST_ALREADY_CLAIMED = "Quest reward already claimed.";
        private const string CHEST_NOT_READY = "Completion chest requirements not met.";
        private const string CHEST_ALREADY_CLAIMED = "Completion chest already claimed.";

        private readonly Wallet _wallet;
        private readonly IServerTimeProvider _serverTimeProvider;
        private readonly Func<DateTimeOffset> _localUtcNow;

        private int _dailyPeriodKey;
        private int _weeklyPeriodKey;
        private readonly Dictionary<string, int> _progress = new();
        private readonly HashSet<string> _claimedQuestIds = new();
        private bool _dailyCompletionChestClaimed;
        private bool _weeklyCompletionChestClaimed;

        public QuestService(
            Wallet wallet,
            IServerTimeProvider serverTimeProvider = null,
            Func<DateTimeOffset> localUtcNow = null)
        {
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _serverTimeProvider = serverTimeProvider;
            _localUtcNow = localUtcNow;
        }

        /// <summary>
        /// 현재 일일 갱신 키를 반환한다.
        /// </summary>
        public int DailyPeriodKey => _dailyPeriodKey;

        /// <summary>
        /// 현재 주간 갱신 키를 반환한다.
        /// </summary>
        public int WeeklyPeriodKey => _weeklyPeriodKey;

        /// <summary>
        /// 퀘스트 진행도를 반환한다.
        /// </summary>
        public int GetProgress(string questId)
        {
            return !string.IsNullOrEmpty(questId) && _progress.TryGetValue(questId, out var count)
                ? count
                : 0;
        }

        /// <summary>
        /// 퀘스트 보상 수령 여부를 반환한다.
        /// </summary>
        public bool IsQuestClaimed(string questId)
        {
            return !string.IsNullOrEmpty(questId) && _claimedQuestIds.Contains(questId);
        }

        /// <summary>
        /// 완료 상자 수령 여부를 반환한다.
        /// </summary>
        public bool IsCompletionChestClaimed(QuestPeriod period)
        {
            return period == QuestPeriod.Daily
                ? _dailyCompletionChestClaimed
                : _weeklyCompletionChestClaimed;
        }

        /// <summary>
        /// 서버 시간 기준으로 갱신 경계를 확인하고 상태를 리셋한다.
        /// </summary>
        public void RefreshPeriods()
        {
            var now = ResolveNowUtc();
            RefreshPeriodsInternal(now);
        }

        /// <summary>
        /// 목표 유형에 맞는 퀘스트 진행도를 누적한다.
        /// </summary>
        public void ReportProgress(QuestObjectiveType objectiveType, int amount, QuestTable table)
        {
            if (amount <= 0 || table == null)
                return;

            RefreshPeriods();
            ApplyProgress(table.DailyQuests, objectiveType, amount);
            ApplyProgress(table.WeeklyQuests, objectiveType, amount);
        }

        /// <summary>
        /// 로그인 목표를 1회 보고한다.
        /// </summary>
        public void ReportLogin(QuestTable table)
        {
            ReportProgress(QuestObjectiveType.Login, 1, table);
        }

        /// <summary>
        /// 개별 퀘스트 보상을 수령한다.
        /// </summary>
        public QuestClaimResult TryClaimQuest(string questId, QuestTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            RefreshPeriods();

            var quest = table.FindQuest(questId);
            if (quest == null)
                return QuestClaimResult.Failed(questId, QuestPeriod.Daily, QUEST_NOT_FOUND);

            if (_claimedQuestIds.Contains(questId))
                return QuestClaimResult.Failed(questId, quest.Period, QUEST_ALREADY_CLAIMED);

            var progress = GetProgress(questId);
            if (progress < quest.TargetCount)
                return QuestClaimResult.Failed(questId, quest.Period, QUEST_NOT_COMPLETE);

            if (!CreditRewards(quest.Rewards, CurrencyReasonCodes.QuestReward))
                return QuestClaimResult.Failed(questId, quest.Period, "Failed to credit reward.");

            _claimedQuestIds.Add(questId);
            MetaRetentionEvents.ReportQuestRewardClaimed(quest.Period);

            return QuestClaimResult.Succeeded(questId, quest.Period);
        }

        /// <summary>
        /// 전량 완료 상자 보상을 수령한다.
        /// </summary>
        public QuestClaimResult TryClaimCompletionChest(QuestPeriod period, QuestTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            RefreshPeriods();

            if (period == QuestPeriod.Daily)
            {
                if (_dailyCompletionChestClaimed)
                    return QuestClaimResult.Failed(null, period, CHEST_ALREADY_CLAIMED, isCompletionChest: true);

                if (!AreAllDailyQuestsClaimed(table))
                    return QuestClaimResult.Failed(null, period, CHEST_NOT_READY, isCompletionChest: true);

                if (!CreditRewards(table.DailyCompletionRewards, CurrencyReasonCodes.QuestCompletionChest))
                    return QuestClaimResult.Failed(null, period, "Failed to credit reward.", isCompletionChest: true);

                _dailyCompletionChestClaimed = true;
                IncrementWeeklyDailyCompletion(table);
                return QuestClaimResult.Succeeded(null, period, isCompletionChest: true);
            }

            if (_weeklyCompletionChestClaimed)
                return QuestClaimResult.Failed(null, period, CHEST_ALREADY_CLAIMED, isCompletionChest: true);

            if (!AreAllWeeklyQuestsClaimed(table))
                return QuestClaimResult.Failed(null, period, CHEST_NOT_READY, isCompletionChest: true);

            if (!CreditRewards(table.WeeklyCompletionRewards, CurrencyReasonCodes.QuestCompletionChest))
                return QuestClaimResult.Failed(null, period, "Failed to credit reward.", isCompletionChest: true);

            _weeklyCompletionChestClaimed = true;
            return QuestClaimResult.Succeeded(null, period, isCompletionChest: true);
        }

        /// <summary>
        /// 세이브용 스냅샷을 생성한다.
        /// </summary>
        public QuestSaveData ToSaveData()
        {
            var progressEntries = new QuestProgressEntry[_progress.Count];
            var index = 0;

            foreach (var pair in _progress)
            {
                progressEntries[index++] = new QuestProgressEntry
                {
                    QuestId = pair.Key,
                    CurrentCount = pair.Value,
                };
            }

            var claimed = new string[_claimedQuestIds.Count];
            _claimedQuestIds.CopyTo(claimed);

            return new QuestSaveData
            {
                DailyPeriodKey = _dailyPeriodKey,
                WeeklyPeriodKey = _weeklyPeriodKey,
                ProgressEntries = progressEntries,
                ClaimedQuestIds = claimed,
                DailyCompletionChestClaimed = _dailyCompletionChestClaimed,
                WeeklyCompletionChestClaimed = _weeklyCompletionChestClaimed,
            };
        }

        /// <summary>
        /// 세이브 스냅샷에서 QuestService 를 복원한다.
        /// </summary>
        public static QuestService FromSaveData(
            QuestSaveData saveData,
            Wallet wallet,
            IServerTimeProvider serverTimeProvider = null,
            Func<DateTimeOffset> localUtcNow = null)
        {
            var service = new QuestService(wallet, serverTimeProvider, localUtcNow);

            if (saveData == null)
            {
                service.RefreshPeriods();
                return service;
            }

            service._dailyPeriodKey = saveData.DailyPeriodKey;
            service._weeklyPeriodKey = saveData.WeeklyPeriodKey;
            service._dailyCompletionChestClaimed = saveData.DailyCompletionChestClaimed;
            service._weeklyCompletionChestClaimed = saveData.WeeklyCompletionChestClaimed;

            if (saveData.ProgressEntries != null)
            {
                foreach (var entry in saveData.ProgressEntries)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.QuestId))
                        continue;

                    service._progress[entry.QuestId] = Math.Max(0, entry.CurrentCount);
                }
            }

            if (saveData.ClaimedQuestIds != null)
            {
                foreach (var questId in saveData.ClaimedQuestIds)
                {
                    if (!string.IsNullOrEmpty(questId))
                        service._claimedQuestIds.Add(questId);
                }
            }

            service.RefreshPeriods();
            return service;
        }

        private void RefreshPeriodsInternal(DateTimeOffset nowUtc)
        {
            var dayKey = DailyResetClock.GetDayKey(nowUtc);
            var weekKey = DailyResetClock.GetWeekKey(nowUtc);

            if (_dailyPeriodKey != 0 && dayKey != _dailyPeriodKey)
                ResetDailyState();

            if (_weeklyPeriodKey != 0 && weekKey != _weeklyPeriodKey)
                ResetWeeklyState();

            _dailyPeriodKey = dayKey;
            _weeklyPeriodKey = weekKey;
        }

        private void ResetDailyState()
        {
            RemoveProgressForPeriod(QuestPeriod.Daily);
            RemoveClaimedForPeriod(QuestPeriod.Daily);
            _dailyCompletionChestClaimed = false;
        }

        private void ResetWeeklyState()
        {
            RemoveProgressForPeriod(QuestPeriod.Weekly);
            RemoveClaimedForPeriod(QuestPeriod.Weekly);
            _weeklyCompletionChestClaimed = false;
        }

        private void RemoveProgressForPeriod(QuestPeriod period)
        {
            var prefix = period == QuestPeriod.Daily ? "daily_" : "weekly_";
            var toRemove = new List<string>();

            foreach (var questId in _progress.Keys)
            {
                if (questId.StartsWith(prefix, StringComparison.Ordinal))
                    toRemove.Add(questId);
            }

            foreach (var questId in toRemove)
                _progress.Remove(questId);
        }

        private void RemoveClaimedForPeriod(QuestPeriod period)
        {
            var prefix = period == QuestPeriod.Daily ? "daily_" : "weekly_";
            var toRemove = new List<string>();

            foreach (var questId in _claimedQuestIds)
            {
                if (questId.StartsWith(prefix, StringComparison.Ordinal))
                    toRemove.Add(questId);
            }

            foreach (var questId in toRemove)
                _claimedQuestIds.Remove(questId);
        }

        private void ApplyProgress(IReadOnlyList<QuestDefinition> quests, QuestObjectiveType objectiveType, int amount)
        {
            foreach (var quest in quests)
            {
                if (quest == null || quest.ObjectiveType != objectiveType)
                    continue;

                var current = GetProgress(quest.QuestId);
                var next = Math.Min(current + amount, quest.TargetCount);
                _progress[quest.QuestId] = next;
            }
        }

        private bool AreAllDailyQuestsClaimed(QuestTable table)
        {
            foreach (var quest in table.DailyQuests)
            {
                if (quest == null)
                    continue;

                if (!_claimedQuestIds.Contains(quest.QuestId))
                    return false;
            }

            return table.DailyQuests.Count > 0;
        }

        private bool AreAllWeeklyQuestsClaimed(QuestTable table)
        {
            foreach (var quest in table.WeeklyQuests)
            {
                if (quest == null)
                    continue;

                if (!_claimedQuestIds.Contains(quest.QuestId))
                    return false;
            }

            return table.WeeklyQuests.Count > 0;
        }

        private void IncrementWeeklyDailyCompletion(QuestTable table)
        {
            foreach (var quest in table.WeeklyQuests)
            {
                if (quest == null || quest.ObjectiveType != QuestObjectiveType.CompleteAllDailyQuests)
                    continue;

                var current = GetProgress(quest.QuestId);
                _progress[quest.QuestId] = Math.Min(current + 1, quest.TargetCount);
            }
        }

        private bool CreditRewards(IReadOnlyList<QuestRewardEntry> rewards, string reasonCode)
        {
            if (rewards == null)
                return true;

            foreach (var reward in rewards)
            {
                if (reward.Amount <= 0L)
                    continue;

                var result = _wallet.TryCredit(reward.CurrencyType, reward.Amount, reasonCode);
                if (!result.Success)
                    return false;
            }

            return true;
        }

        private DateTimeOffset ResolveNowUtc()
        {
            if (_serverTimeProvider != null
                && _serverTimeProvider.TryGetServerTimeUtc(out var serverTimeUtc))
            {
                return serverTimeUtc;
            }

            return _localUtcNow != null ? _localUtcNow() : DateTimeOffset.UtcNow;
        }
    }
}
