using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Backend.Meta.Currency.Tests
{
    public class WalletTests
    {
        private static readonly DateTimeOffset FixedNow =
            new DateTimeOffset(2026, 8, 1, 12, 0, 0, TimeSpan.Zero);

        private TransactionLedger _ledger;
        private Wallet _wallet;

        [SetUp]
        public void SetUp()
        {
            _ledger = new TransactionLedger(
                TransactionLedger.DEFAULT_MAX_ENTRIES,
                () => FixedNow);
            _wallet = new Wallet(_ledger);
        }

        [Test]
        public void CurrencyType_DefinesSevenCurrencies()
        {
            var values = Enum.GetValues(typeof(CurrencyType));
            Assert.AreEqual(7, values.Length);
            CollectionAssert.AreEquivalent(
                new[]
                {
                    CurrencyType.Gold,
                    CurrencyType.ManaShard,
                    CurrencyType.RelicFragment,
                    CurrencyType.AbyssStone,
                    CurrencyType.SummonTicket,
                    CurrencyType.LegacyPoint,
                    CurrencyType.Reputation,
                },
                values.Cast<CurrencyType>());
        }

        [Test]
        public void GetBalance_ReturnsZero_ForUnsetCurrency()
        {
            Assert.AreEqual(0L, _wallet.GetBalance(CurrencyType.Gold));
        }

        [Test]
        public void TryCredit_IncreasesBalance_AndRecordsLedger()
        {
            var result = _wallet.TryCredit(
                CurrencyType.Gold,
                100L,
                CurrencyReasonCodes.CombatReward);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(100L, _wallet.GetBalance(CurrencyType.Gold));
            Assert.AreEqual(100L, result.BalanceAfter);
            Assert.AreEqual(100L, result.Delta);

            Assert.AreEqual(1, _ledger.Entries.Count);
            var entry = _ledger.Entries[0];
            Assert.AreEqual(CurrencyReasonCodes.CombatReward, entry.ReasonCode);
            Assert.AreEqual(CurrencyType.Gold, entry.CurrencyType);
            Assert.AreEqual(100L, entry.Delta);
            Assert.AreEqual(100L, entry.BalanceAfter);
            Assert.AreEqual(FixedNow.UtcTicks, entry.TimestampUtcTicks);
        }

        [Test]
        public void TryDebit_DecreasesBalance_AndRecordsNegativeDelta()
        {
            _wallet.TryCredit(CurrencyType.Gold, 200L, CurrencyReasonCodes.CombatReward);

            var result = _wallet.TryDebit(
                CurrencyType.Gold,
                80L,
                CurrencyReasonCodes.UpgradeCost);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(120L, _wallet.GetBalance(CurrencyType.Gold));
            Assert.AreEqual(-80L, result.Delta);

            var entry = _ledger.Entries[^1];
            Assert.AreEqual(-80L, entry.Delta);
            Assert.AreEqual(120L, entry.BalanceAfter);
        }

        [Test]
        public void TryDebit_DoesNotGoNegative_WhenInsufficient()
        {
            _wallet.TryCredit(CurrencyType.Gold, 50L, CurrencyReasonCodes.CombatReward);

            var result = _wallet.TryDebit(
                CurrencyType.Gold,
                100L,
                CurrencyReasonCodes.UpgradeCost);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(50L, _wallet.GetBalance(CurrencyType.Gold));
            Assert.AreEqual(50L, result.BalanceAfter);
            Assert.IsNotNull(result.FailureReason);
            Assert.AreEqual(1, _ledger.Entries.Count);
        }

        [Test]
        public void TryDebit_ReturnsFailure_NotException_WhenInsufficient()
        {
            Assert.DoesNotThrow(() =>
            {
                var result = _wallet.TryDebit(
                    CurrencyType.AbyssStone,
                    1L,
                    CurrencyReasonCodes.SummonCost);
                Assert.IsFalse(result.Success);
            });
        }

        [Test]
        public void TryCredit_RejectsNonPositiveAmount_WithoutChangingBalance()
        {
            var result = _wallet.TryCredit(
                CurrencyType.Gold,
                0L,
                CurrencyReasonCodes.CombatReward);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(0L, _wallet.GetBalance(CurrencyType.Gold));
            Assert.AreEqual(0, _ledger.Entries.Count);
        }

        [Test]
        public void TryDebit_RejectsNonPositiveAmount_WithoutChangingBalance()
        {
            _wallet.TryCredit(CurrencyType.Gold, 10L, CurrencyReasonCodes.CombatReward);

            var result = _wallet.TryDebit(
                CurrencyType.Gold,
                0L,
                CurrencyReasonCodes.UpgradeCost);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(10L, _wallet.GetBalance(CurrencyType.Gold));
            Assert.AreEqual(1, _ledger.Entries.Count);
        }

        [Test]
        public void CanAfford_ReturnsFalse_ForZeroOrNegativeAmount()
        {
            _wallet.TryCredit(CurrencyType.Gold, 10L, CurrencyReasonCodes.CombatReward);

            Assert.IsFalse(_wallet.CanAfford(CurrencyType.Gold, 0L));
            Assert.IsFalse(_wallet.CanAfford(CurrencyType.Gold, -1L));
        }

        [Test]
        public void CanAfford_MatchesBalance()
        {
            _wallet.TryCredit(CurrencyType.ManaShard, 30L, CurrencyReasonCodes.QuestReward);

            Assert.IsTrue(_wallet.CanAfford(CurrencyType.ManaShard, 30L));
            Assert.IsTrue(_wallet.CanAfford(CurrencyType.ManaShard, 10L));
            Assert.IsFalse(_wallet.CanAfford(CurrencyType.ManaShard, 31L));
        }

        [Test]
        public void TransactionLedger_KeepsOnlyRecentMaxEntries()
        {
            var ledger = new TransactionLedger(maxEntries: 3, () => FixedNow);
            var wallet = new Wallet(ledger);

            for (var i = 1; i <= 5; i++)
            {
                wallet.TryCredit(
                    CurrencyType.Gold,
                    1L,
                    $"reason_{i}");
            }

            Assert.AreEqual(3, ledger.Entries.Count);
            Assert.AreEqual("reason_3", ledger.Entries[0].ReasonCode);
            Assert.AreEqual("reason_5", ledger.Entries[^1].ReasonCode);
        }

        [Test]
        public void SaveAndLoad_PreservesBalancesAndLedger()
        {
            _wallet.TryCredit(CurrencyType.Gold, 100L, CurrencyReasonCodes.CombatReward);
            _wallet.TryDebit(CurrencyType.Gold, 40L, CurrencyReasonCodes.UpgradeCost);
            _wallet.TryCredit(CurrencyType.AbyssStone, 5L, CurrencyReasonCodes.IapGrant);

            var saveData = _wallet.ToSaveData();
            var restored = Wallet.FromSaveData(saveData, () => FixedNow);

            Assert.AreEqual(60L, restored.GetBalance(CurrencyType.Gold));
            Assert.AreEqual(5L, restored.GetBalance(CurrencyType.AbyssStone));
            Assert.AreEqual(3, restored.Ledger.Entries.Count);
            Assert.AreEqual(-40L, restored.Ledger.Entries[^2].Delta);
        }

        [Test]
        public void Wallet_HasNoHardToSoftExchangeApi()
        {
            var forbiddenNames = new[]
            {
                "Exchange",
                "Convert",
                "PurchaseSoft",
                "BuySoft",
                "Swap",
            };

            var methods = typeof(Wallet)
                .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
                .Select(method => method.Name);

            foreach (var forbidden in forbiddenNames)
            {
                CollectionAssert.DoesNotContain(methods, forbidden);
            }
        }

        [Test]
        public void CurrencyPolicy_ClassifiesHardAndSoftCurrencies()
        {
            Assert.IsTrue(CurrencyPolicy.IsSoftCurrency(CurrencyType.Gold));
            Assert.IsTrue(CurrencyPolicy.IsSoftCurrency(CurrencyType.ManaShard));
            Assert.IsTrue(CurrencyPolicy.IsSoftCurrency(CurrencyType.RelicFragment));

            Assert.IsTrue(CurrencyPolicy.IsHardCurrency(CurrencyType.AbyssStone));
            Assert.IsTrue(CurrencyPolicy.IsHardCurrency(CurrencyType.SummonTicket));

            Assert.IsFalse(CurrencyPolicy.IsHardCurrency(CurrencyType.Gold));
            Assert.IsFalse(CurrencyPolicy.IsSoftCurrency(CurrencyType.AbyssStone));
        }
    }
}
