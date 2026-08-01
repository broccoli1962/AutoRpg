using System;
using Backend.GameSystems.Offline;
using Backend.Meta.Currency;
using Backend.Meta.Mailbox;
using NUnit.Framework;

namespace Backend.Meta.Mailbox.Tests
{
    public class MailboxServiceTests
    {
        private static readonly DateTimeOffset FixedNow =
            new DateTimeOffset(2026, 8, 1, 12, 0, 0, TimeSpan.Zero);

        private TransactionLedger _ledger;
        private Wallet _wallet;
        private DateTimeOffset _nowUtc;
        private MailboxService _service;

        [SetUp]
        public void SetUp()
        {
            _nowUtc = FixedNow;
            _ledger = new TransactionLedger();
            _wallet = new Wallet(_ledger);
            _service = new MailboxService(
                _wallet,
                new FixedServerTimeProvider(() => _nowUtc),
                () => _nowUtc);
        }

        [Test]
        public void TryClaim_CreditsWallet_AndBlocksDuplicate()
        {
            var mail = _service.AddRewardMail(
                "Test",
                "Body",
                new[] { new CurrencyRewardEntry { CurrencyType = CurrencyType.Gold, Amount = 100L } },
                _nowUtc.AddDays(7));

            var first = _service.TryClaim(mail.MailId);
            var second = _service.TryClaim(mail.MailId);

            Assert.IsTrue(first.Success);
            Assert.IsFalse(second.Success);
            Assert.AreEqual(100L, _wallet.GetBalance(CurrencyType.Gold));
            Assert.AreEqual(CurrencyReasonCodes.MailboxReward, _ledger.Entries[0].ReasonCode);
        }

        [Test]
        public void TryClaimAll_ClaimsMultipleMails()
        {
            _service.AddRewardMail(
                "A",
                "Body",
                new[] { new CurrencyRewardEntry { CurrencyType = CurrencyType.Gold, Amount = 100L } },
                _nowUtc.AddDays(7));
            _service.AddRewardMail(
                "B",
                "Body",
                new[] { new CurrencyRewardEntry { CurrencyType = CurrencyType.Gold, Amount = 50L } },
                _nowUtc.AddDays(7));
            _service.AddNoticeMail("Notice", "No reward", _nowUtc.AddDays(7));

            var result = _service.TryClaimAll();

            Assert.AreEqual(2, result.ClaimedCount);
            Assert.AreEqual(150L, _wallet.GetBalance(CurrencyType.Gold));
        }

        [Test]
        public void PurgeExpired_RemovesExpiredMails()
        {
            var mail = _service.AddRewardMail(
                "Expired",
                "Body",
                new[] { new CurrencyRewardEntry { CurrencyType = CurrencyType.Gold, Amount = 100L } },
                _nowUtc.AddHours(-1));

            Assert.AreEqual(1, _service.PurgeExpired());
            Assert.AreEqual(0, _service.Mails.Count);

            var claim = _service.TryClaim(mail.MailId);
            Assert.IsFalse(claim.Success);
        }

        [Test]
        public void TryClaim_FailsForExpiredMail()
        {
            var mail = _service.AddRewardMail(
                "Soon",
                "Body",
                new[] { new CurrencyRewardEntry { CurrencyType = CurrencyType.Gold, Amount = 100L } },
                _nowUtc.AddHours(1));

            _nowUtc = _nowUtc.AddHours(2);

            var result = _service.TryClaim(mail.MailId);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(0L, _wallet.GetBalance(CurrencyType.Gold));
        }

        [Test]
        public void SaveAndLoad_PreservesMailboxState()
        {
            var mail = _service.AddRewardMail(
                "Save",
                "Body",
                new[] { new CurrencyRewardEntry { CurrencyType = CurrencyType.AbyssStone, Amount = 5L } },
                _nowUtc.AddDays(3));

            _service.TryClaim(mail.MailId);

            var restored = MailboxService.FromSaveData(
                _service.ToSaveData(),
                _wallet,
                new FixedServerTimeProvider(() => _nowUtc),
                () => _nowUtc);

            Assert.AreEqual(1, restored.Mails.Count);
            Assert.IsTrue(restored.Mails[0].IsClaimed);
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
