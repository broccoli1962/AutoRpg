using Backend.Meta.Currency;
using NUnit.Framework;
using UnityEngine;

namespace Backend.Meta.Achievements.Tests
{
    public class AchievementServiceTests
    {
        private TransactionLedger _ledger;
        private Wallet _wallet;
        private AchievementTable _table;
        private AchievementService _service;
        private AchievementRewardResolver _rewardResolver;

        [SetUp]
        public void SetUp()
        {
            _ledger = new TransactionLedger();
            _wallet = new Wallet(_ledger);
            _table = ScriptableObject.CreateInstance<AchievementTable>();
            _table.ApplySpecDefaults();
            _service = new AchievementService(_wallet);
            _rewardResolver = new AchievementRewardResolver();
        }

        [TearDown]
        public void TearDown()
        {
            MetaGameplayEvents.ClearSubscribers();

            if (_table != null)
                UnityEngine.Object.DestroyImmediate(_table);
        }

        [Test]
        public void AchievementTable_DefinesSevenCategoriesWithThreeTiersEach()
        {
            Assert.AreEqual(7, _table.Categories.Count);

            foreach (var category in _table.Categories)
            {
                Assert.IsNotNull(category);
                Assert.AreEqual(3, category.Tiers.Length);
            }

            Assert.IsNotNull(_table.FindTier("total_kills_tier_0"));
            Assert.IsNotNull(_table.FindTier("compendium_entries_tier_2"));
        }

        [Test]
        public void ReportProgress_CompletesTier_WhenTargetReached()
        {
            _service.ReportProgress(AchievementCategory.TotalKills, 150L, _table);

            Assert.IsTrue(_service.IsTierComplete("total_kills_tier_0", _table));
            Assert.IsFalse(_service.IsTierComplete("total_kills_tier_1", _table));
        }

        [Test]
        public void ReportHighestFloor_UsesMaximumProgress()
        {
            _service.ReportHighestFloor(40, _table);
            _service.ReportHighestFloor(30, _table);

            Assert.AreEqual(40L, _service.GetProgress(AchievementCategory.HighestFloor));
            Assert.IsTrue(_service.IsTierComplete("highest_floor_tier_0", _table));
            Assert.IsFalse(_service.IsTierComplete("highest_floor_tier_1", _table));
        }

        [Test]
        public void TryClaimTier_CreditsWallet_AndBlocksDuplicateClaim()
        {
            _service.ReportProgress(AchievementCategory.SummonCount, 10L, _table);

            var first = _service.TryClaimTier("summon_count_tier_0", _table, _rewardResolver);
            var second = _service.TryClaimTier("summon_count_tier_0", _table, _rewardResolver);

            Assert.IsTrue(first.Success);
            Assert.IsFalse(second.Success);
            Assert.AreEqual(10L, first.RewardAmount);
            Assert.AreEqual(10L, _wallet.GetBalance(CurrencyType.AbyssStone));
            Assert.AreEqual(CurrencyReasonCodes.AchievementReward, _ledger.Entries[0].ReasonCode);
        }

        [Test]
        public void PrestigeSimulation_DoesNotResetAchievementProgress()
        {
            _service.ReportProgress(AchievementCategory.TotalKills, 500L, _table);
            _service.ReportProgress(AchievementCategory.PrestigeCount, 1L, _table);
            _service.TryClaimTier("total_kills_tier_0", _table, _rewardResolver);

            var beforePrestige = _service.ToSaveData();

            _service.ReportProgress(AchievementCategory.PrestigeCount, 1L, _table);

            Assert.AreEqual(500L, _service.GetProgress(AchievementCategory.TotalKills));
            Assert.AreEqual(2L, _service.GetProgress(AchievementCategory.PrestigeCount));
            Assert.IsTrue(_service.IsTierClaimed("total_kills_tier_0"));

            var restored = AchievementService.FromSaveData(beforePrestige, _wallet);
            Assert.AreEqual(500L, restored.GetProgress(AchievementCategory.TotalKills));
            Assert.IsTrue(restored.IsTierClaimed("total_kills_tier_0"));
        }

        [Test]
        public void RemoteConfigOverlay_OverridesTierRewardAmount()
        {
            _service.ReportProgress(AchievementCategory.EquipmentUpgrades, 10L, _table);

            var overlay = new AchievementRemoteConfigOverlay();
            overlay.SetTierRewardOverride(
                AchievementRemoteConfigKeys.TierReward("equipment_upgrades", 0),
                99L);

            var resolver = new AchievementRewardResolver(overlay);
            var result = _service.TryClaimTier("equipment_upgrades_tier_0", _table, resolver);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(99L, result.RewardAmount);
            Assert.AreEqual(99L, _wallet.GetBalance(CurrencyType.AbyssStone));
        }

        [Test]
        public void EventBridge_SubscribesToGameplayEvents()
        {
            using var bridge = new AchievementEventBridge(_service, _table);

            MetaGameplayEvents.ReportEnemyKills(120);
            MetaGameplayEvents.ReportFloorReached(60);
            MetaGameplayEvents.ReportCollectionProgress(3, 6);
            MetaGameplayEvents.ReportPrestige();

            Assert.AreEqual(120L, _service.GetProgress(AchievementCategory.TotalKills));
            Assert.AreEqual(60L, _service.GetProgress(AchievementCategory.HighestFloor));
            Assert.AreEqual(50L, _service.GetProgress(AchievementCategory.CollectionCompletion));
            Assert.AreEqual(1L, _service.GetProgress(AchievementCategory.PrestigeCount));
        }

        [Test]
        public void SaveAndLoad_PreservesAchievementState()
        {
            _service.ReportProgress(AchievementCategory.CompendiumEntries, 8L, _table);
            _service.TryClaimTier("compendium_entries_tier_0", _table, _rewardResolver);

            var restored = AchievementService.FromSaveData(_service.ToSaveData(), _wallet);

            Assert.AreEqual(8L, restored.GetProgress(AchievementCategory.CompendiumEntries));
            Assert.IsTrue(restored.IsTierClaimed("compendium_entries_tier_0"));
        }
    }
}
