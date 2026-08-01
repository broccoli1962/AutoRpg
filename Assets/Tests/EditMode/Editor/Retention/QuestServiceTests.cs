using System;
using Backend.GameSystems.Offline;
using Backend.Meta.Currency;
using Backend.Meta.Quests;
using Backend.Meta.Retention;
using NUnit.Framework;
using UnityEngine;

namespace Backend.Meta.Quests.Tests
{
    public class QuestServiceTests
    {
        private static readonly DateTimeOffset DayOneUtc =
            new DateTimeOffset(2026, 7, 31, 19, 30, 0, TimeSpan.Zero);

        private static readonly DateTimeOffset DayTwoUtc =
            new DateTimeOffset(2026, 8, 1, 19, 30, 0, TimeSpan.Zero);

        private TransactionLedger _ledger;
        private Wallet _wallet;
        private QuestTable _table;
        private DateTimeOffset _nowUtc;
        private QuestService _service;

        [SetUp]
        public void SetUp()
        {
            _nowUtc = DayOneUtc;
            _ledger = new TransactionLedger();
            _wallet = new Wallet(_ledger);
            _table = ScriptableObject.CreateInstance<QuestTable>();
            _table.ApplySpecDefaults();

            _service = new QuestService(
                _wallet,
                new FixedServerTimeProvider(() => _nowUtc),
                () => _nowUtc);
            _service.RefreshPeriods();
        }

        [TearDown]
        public void TearDown()
        {
            if (_table != null)
                UnityEngine.Object.DestroyImmediate(_table);
        }

        [Test]
        public void QuestTable_DefinesSixDailyAndFiveWeeklyQuests()
        {
            Assert.AreEqual(6, _table.DailyQuests.Count);
            Assert.AreEqual(5, _table.WeeklyQuests.Count);
            Assert.IsNotNull(_table.FindQuest("daily_login"));
            Assert.IsNotNull(_table.FindQuest("weekly_kill_500"));
        }

        [Test]
        public void ReportProgress_AccumulatesMatchingObjectives()
        {
            _service.ReportProgress(QuestObjectiveType.KillEnemies, 20, _table);
            _service.ReportProgress(QuestObjectiveType.KillEnemies, 40, _table);

            Assert.AreEqual(50, _service.GetProgress("daily_kill_50"));
            Assert.AreEqual(60, _service.GetProgress("weekly_kill_500"));
        }

        [Test]
        public void TryClaimQuest_CreditsWallet_AndBlocksDuplicateClaim()
        {
            _service.ReportProgress(QuestObjectiveType.Login, 1, _table);

            var first = _service.TryClaimQuest("daily_login", _table);
            var second = _service.TryClaimQuest("daily_login", _table);

            Assert.IsTrue(first.Success);
            Assert.IsFalse(second.Success);
            Assert.AreEqual(500L, _wallet.GetBalance(CurrencyType.Gold));
            Assert.AreEqual(1, _ledger.Entries.Count);
            Assert.AreEqual(CurrencyReasonCodes.QuestReward, _ledger.Entries[0].ReasonCode);
        }

        [Test]
        public void TryClaimCompletionChest_RequiresAllDailyQuestsClaimed()
        {
            CompleteAndClaimAllDailyQuests();

            var chest = _service.TryClaimCompletionChest(QuestPeriod.Daily, _table);
            var duplicate = _service.TryClaimCompletionChest(QuestPeriod.Daily, _table);

            Assert.IsTrue(chest.Success);
            Assert.IsFalse(duplicate.Success);
            Assert.IsTrue(_service.IsCompletionChestClaimed(QuestPeriod.Daily));

            var chestCredits = 0;
            foreach (var entry in _ledger.Entries)
            {
                if (entry.ReasonCode == CurrencyReasonCodes.QuestCompletionChest)
                    chestCredits++;
            }

            Assert.AreEqual(2, chestCredits);
        }

        [Test]
        public void RefreshPeriods_ResetsDailyProgress_OnNextGameDay()
        {
            _service.ReportProgress(QuestObjectiveType.KillEnemies, 50, _table);
            Assert.AreEqual(50, _service.GetProgress("daily_kill_50"));

            _nowUtc = DayTwoUtc;
            _service.RefreshPeriods();

            Assert.AreEqual(0, _service.GetProgress("daily_kill_50"));
            Assert.AreEqual(DailyResetClock.GetDayKey(DayTwoUtc), _service.DailyPeriodKey);
        }

        [Test]
        public void DailyCompletionChest_IncrementsWeeklyDailyCompletionQuest()
        {
            CompleteAndClaimAllDailyQuests();
            _service.TryClaimCompletionChest(QuestPeriod.Daily, _table);

            Assert.AreEqual(1, _service.GetProgress("weekly_daily_all_5"));
        }

        [Test]
        public void SaveAndLoad_PreservesQuestState()
        {
            _service.ReportProgress(QuestObjectiveType.Login, 1, _table);
            _service.TryClaimQuest("daily_login", _table);

            var saveData = _service.ToSaveData();
            var restored = QuestService.FromSaveData(
                saveData,
                _wallet,
                new FixedServerTimeProvider(() => _nowUtc),
                () => _nowUtc);

            Assert.IsTrue(restored.IsQuestClaimed("daily_login"));
            Assert.AreEqual(1, restored.GetProgress("daily_login"));
        }

        private void CompleteAndClaimAllDailyQuests()
        {
            _service.ReportProgress(QuestObjectiveType.Login, 1, _table);
            _service.ReportProgress(QuestObjectiveType.KillEnemies, 50, _table);
            _service.ReportProgress(QuestObjectiveType.ClearFloors, 5, _table);
            _service.ReportProgress(QuestObjectiveType.UpgradeEquipment, 3, _table);
            _service.ReportProgress(QuestObjectiveType.Summon, 1, _table);
            _service.ReportProgress(QuestObjectiveType.CollectOfflineReward, 1, _table);

            foreach (var quest in _table.DailyQuests)
                _service.TryClaimQuest(quest.QuestId, _table);
        }

        private sealed class FixedServerTimeProvider : IServerTimeProvider
        {
            private readonly Func<DateTimeOffset> _nowProvider;

            public FixedServerTimeProvider(Func<DateTimeOffset> nowProvider)
            {
                _nowProvider = nowProvider;
            }

            public bool TryGetServerTimeUtc(out DateTimeOffset serverTimeUtc)
            {
                serverTimeUtc = _nowProvider();
                return true;
            }
        }
    }
}
