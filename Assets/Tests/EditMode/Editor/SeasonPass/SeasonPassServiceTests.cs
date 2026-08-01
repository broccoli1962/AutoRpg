using System;
using Backend.Meta.Achievements;
using Backend.GameSystems.Offline;
using Backend.Meta.Currency;
using Backend.Meta.Mailbox;
using Backend.Meta.Quests;
using Backend.Meta.Retention;
using NUnit.Framework;
using UnityEngine;

namespace Backend.Meta.SeasonPass.Tests
{
    public class SeasonPassServiceTests
    {
        private static readonly DateTimeOffset SeasonOneMidUtc =
            new DateTimeOffset(2026, 8, 14, 12, 0, 0, TimeSpan.Zero);

        private static readonly DateTimeOffset SeasonTwoMidUtc =
            new DateTimeOffset(2026, 9, 15, 12, 0, 0, TimeSpan.Zero);

        private TransactionLedger _ledger;
        private Wallet _wallet;
        private MailboxService _mailbox;
        private SeasonPassTable _table;
        private DateTimeOffset _nowUtc;
        private SeasonPassService _service;
        private RecordingPremiumSync _premiumSync;
        private RecordingPushNotifier _pushNotifier;

        [SetUp]
        public void SetUp()
        {
            _nowUtc = SeasonOneMidUtc;
            _ledger = new TransactionLedger();
            _wallet = new Wallet(_ledger);
            _mailbox = new MailboxService(
                _wallet,
                new FixedServerTimeProvider(() => _nowUtc),
                () => _nowUtc);
            _table = ScriptableObject.CreateInstance<SeasonPassTable>();
            _table.ApplySpecDefaults();
            _premiumSync = new RecordingPremiumSync();
            _pushNotifier = new RecordingPushNotifier();

            _service = new SeasonPassService(
                _wallet,
                new FixedServerTimeProvider(() => _nowUtc),
                () => _nowUtc,
                _premiumSync,
                _pushNotifier);
            _service.RefreshSeason(_table, _mailbox);
        }

        [TearDown]
        public void TearDown()
        {
            MetaGameplayEvents.ClearSubscribers();
            MetaRetentionEvents.ClearSubscribers();

            if (_table != null)
                UnityEngine.Object.DestroyImmediate(_table);
        }

        [Test]
        public void SeasonPassTable_DefinesFiftyTiersWithFreeAndPremiumTracks()
        {
            Assert.AreEqual(50, _table.Tiers.Count);
            Assert.AreEqual(300, _table.PointConfig.DailyEarnCap);

            foreach (var tier in _table.Tiers)
            {
                Assert.IsNotNull(tier);
                Assert.IsNotEmpty(tier.FreeRewards);
                Assert.IsNotEmpty(tier.PremiumRewards);
            }

            Assert.AreEqual(100, _table.FindTier(1).RequiredPoints);
            Assert.AreEqual(5000, _table.FindTier(50).RequiredPoints);
        }

        [Test]
        public void DailyEarnCap_BlocksExcessPointGain()
        {
            var cap = _table.PointConfig.DailyEarnCap;

            for (var i = 0; i < 20; i++)
                _service.ReportBossKill(_table);

            Assert.AreEqual(cap, _service.DailyPointsEarned);
            Assert.AreEqual(cap, _service.SeasonPoints);

            var extra = _service.ReportDailyQuestComplete(_table);
            Assert.AreEqual(0, extra);
            Assert.AreEqual(cap, _service.SeasonPoints);
        }

        [Test]
        public void UnlockPremium_GrantsRetroactiveRewardsForReachedTiers()
        {
            EarnSeasonPoints(500);

            var unlock = _service.UnlockPremium(_table);

            Assert.IsTrue(unlock.Success);
            Assert.AreEqual(5, unlock.RetroactiveTierCount);
            Assert.IsTrue(_service.IsPremiumUnlocked);
            Assert.IsTrue(_service.IsPremiumTierClaimed(1));
            Assert.IsTrue(_service.IsPremiumTierClaimed(5));
            Assert.IsFalse(_service.IsPremiumTierClaimed(6));
            Assert.IsTrue(_premiumSync.LastPersistedPremium);
            Assert.Greater(_wallet.GetBalance(CurrencyType.AbyssStone), 0L);

            var duplicate = _service.UnlockPremium(_table);
            Assert.IsFalse(duplicate.Success);
        }

        [Test]
        public void SeasonEnd_MigratesUnclaimedRewardsToMailbox_AndResetsPoints()
        {
            EarnSeasonPoints(250);
            _service.TryClaimFreeTier(1, _table);

            _nowUtc = SeasonTwoMidUtc;
            _service.RefreshSeason(_table, _mailbox);

            Assert.AreEqual(2, _service.SeasonNumber);
            Assert.AreEqual(0, _service.SeasonPoints);
            Assert.AreEqual(1, _mailbox.Mails.Count);

            var mail = _mailbox.Mails[0];
            Assert.AreEqual(MailType.Reward, mail.Type);
            Assert.IsTrue(mail.Title.Contains("Season Pass"));
            Assert.Greater(mail.Rewards.Length, 0);
        }

        [Test]
        public void TryClaimFreeTier_CreditsWallet_AndBlocksDuplicateClaim()
        {
            EarnSeasonPoints(100);

            var first = _service.TryClaimFreeTier(1, _table);
            var second = _service.TryClaimFreeTier(1, _table);

            Assert.IsTrue(first.Success);
            Assert.IsFalse(second.Success);
            Assert.Greater(_wallet.GetBalance(CurrencyType.Gold), 0L);
            Assert.AreEqual(CurrencyReasonCodes.SeasonPassFreeReward, _ledger.Entries[0].ReasonCode);
        }

        [Test]
        public void EventBridge_AwardsPointsFromGameplayAndQuestEvents()
        {
            using var bridge = new SeasonPassEventBridge(_service, _table);

            MetaGameplayEvents.ReportFloorReached(10);
            MetaGameplayEvents.ReportFloorReached(8);
            MetaGameplayEvents.ReportBossKill(2);
            MetaRetentionEvents.ReportQuestRewardClaimed(QuestPeriod.Daily);
            MetaRetentionEvents.ReportQuestRewardClaimed(QuestPeriod.Weekly);

            var expected =
                _table.GetPointsForSource(SeasonPointSource.FloorReached)
                + _table.GetPointsForSource(SeasonPointSource.BossKill) * 2
                + _table.GetPointsForSource(SeasonPointSource.DailyQuestComplete)
                + _table.GetPointsForSource(SeasonPointSource.WeeklyQuestComplete);

            Assert.AreEqual(expected, _service.SeasonPoints);
        }

        [Test]
        public void RefreshSeason_SchedulesSeasonEndPushHook()
        {
            Assert.AreEqual(1, _pushNotifier.ScheduleCount);
            Assert.AreEqual(1, _pushNotifier.LastScheduledSeasonNumber);
        }

        [Test]
        public void SaveAndLoad_PreservesSeasonPassState()
        {
            EarnSeasonPoints(200);
            _service.UnlockPremium(_table);
            _service.TryClaimFreeTier(1, _table);

            var restored = SeasonPassService.FromSaveData(
                _service.ToSaveData(),
                _wallet,
                new FixedServerTimeProvider(() => _nowUtc),
                () => _nowUtc,
                _premiumSync,
                _pushNotifier);

            Assert.AreEqual(200, restored.SeasonPoints);
            Assert.IsTrue(restored.IsPremiumUnlocked);
            Assert.IsTrue(restored.IsFreeTierClaimed(1));
            Assert.IsTrue(restored.IsPremiumTierClaimed(1));
        }

        private void EarnSeasonPoints(int targetPoints)
        {
            while (_service.SeasonPoints < targetPoints)
            {
                if (_service.DailyPointsEarned >= _table.PointConfig.DailyEarnCap)
                    _nowUtc = _nowUtc.AddDays(1);

                _service.ReportBossKill(_table);
            }
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

        private sealed class RecordingPremiumSync : ISeasonPassPremiumSync
        {
            public bool LastPersistedPremium { get; private set; }

            public void PersistPremiumUnlocked(int seasonNumber, bool isPremiumUnlocked)
            {
                LastPersistedPremium = isPremiumUnlocked;
            }

            public bool TryRestorePremiumUnlocked(int seasonNumber, out bool isPremiumUnlocked)
            {
                isPremiumUnlocked = false;
                return false;
            }
        }

        private sealed class RecordingPushNotifier : ISeasonPassPushNotifier
        {
            public int ScheduleCount { get; private set; }
            public int LastScheduledSeasonNumber { get; private set; }

            public void ScheduleSeasonEndReminder(int seasonNumber, DateTimeOffset seasonEndUtc)
            {
                ScheduleCount++;
                LastScheduledSeasonNumber = seasonNumber;
            }

            public void CancelSeasonEndReminder(int seasonNumber)
            {
            }
        }
    }
}
