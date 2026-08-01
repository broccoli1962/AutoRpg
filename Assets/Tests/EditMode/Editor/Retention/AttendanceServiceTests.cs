using System;
using Backend.GameSystems.Offline;
using Backend.Meta.Attendance;
using Backend.Meta.Currency;
using Backend.Meta.Retention;
using NUnit.Framework;
using UnityEngine;

namespace Backend.Meta.Attendance.Tests
{
    public class AttendanceServiceTests
    {
        private static readonly DateTimeOffset DayOneUtc =
            new DateTimeOffset(2026, 7, 31, 19, 30, 0, TimeSpan.Zero);

        private static readonly DateTimeOffset DayTwoUtc =
            new DateTimeOffset(2026, 8, 1, 19, 30, 0, TimeSpan.Zero);

        private static readonly DateTimeOffset DayFourUtc =
            new DateTimeOffset(2026, 8, 3, 19, 30, 0, TimeSpan.Zero);

        private TransactionLedger _ledger;
        private Wallet _wallet;
        private AttendanceTable _table;
        private DateTimeOffset _nowUtc;
        private AttendanceService _service;

        [SetUp]
        public void SetUp()
        {
            _nowUtc = DayOneUtc;
            _ledger = new TransactionLedger();
            _wallet = new Wallet(_ledger);
            _table = ScriptableObject.CreateInstance<AttendanceTable>();
            _table.ApplySpecDefaults();

            _service = new AttendanceService(
                _wallet,
                new FixedServerTimeProvider(() => _nowUtc),
                () => _nowUtc);
        }

        [TearDown]
        public void TearDown()
        {
            if (_table != null)
                UnityEngine.Object.DestroyImmediate(_table);
        }

        [Test]
        public void TryCheckIn_GrantsNewPlayerAndMonthlyRewards()
        {
            var result = _service.TryCheckIn(_table);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1_300L, _wallet.GetBalance(CurrencyType.Gold));
            Assert.AreEqual(2, _service.NewPlayerNextDay);
            Assert.IsTrue(_service.IsMonthlyDayClaimed(1));
        }

        [Test]
        public void TryCheckIn_BlocksDuplicateClaim_OnSameGameDay()
        {
            Assert.IsTrue(_service.TryCheckIn(_table).Success);

            var duplicate = _service.TryCheckIn(_table);
            Assert.IsFalse(duplicate.Success);
            Assert.AreEqual(1_300L, _wallet.GetBalance(CurrencyType.Gold));
        }

        [Test]
        public void TryCheckIn_AdvancesNewPlayerTrack_OnConsecutiveDays()
        {
            Assert.IsTrue(_service.TryCheckIn(_table).Success);

            _nowUtc = DayTwoUtc;
            Assert.IsTrue(_service.TryCheckIn(_table).Success);
            Assert.AreEqual(3, _service.NewPlayerNextDay);
        }

        [Test]
        public void TryCheckIn_ResetsNewPlayerStreak_WhenDayMissed()
        {
            Assert.IsTrue(_service.TryCheckIn(_table).Success);
            Assert.AreEqual(2, _service.NewPlayerNextDay);

            _nowUtc = DayFourUtc;
            Assert.IsTrue(_service.TryCheckIn(_table).Success);
            Assert.AreEqual(2, _service.NewPlayerNextDay);
        }

        [Test]
        public void TryCheckIn_CompletesNewPlayerTrack_AfterSevenDays()
        {
            for (var day = 0; day < 7; day++)
            {
                _nowUtc = DayOneUtc.AddDays(day);
                Assert.IsTrue(_service.TryCheckIn(_table).Success);
            }

            Assert.IsTrue(_service.IsNewPlayerTrackCompleted);
            Assert.AreEqual(8, _service.NewPlayerNextDay);
        }

        [Test]
        public void MonthlyCalendar_ResetsOnNewMonth()
        {
            Assert.IsTrue(_service.TryCheckIn(_table).Success);
            Assert.IsTrue(_service.IsMonthlyDayClaimed(1));
            var goldAfterAugust = _wallet.GetBalance(CurrencyType.Gold);

            _nowUtc = new DateTimeOffset(2026, 8, 31, 19, 30, 0, TimeSpan.Zero);
            Assert.IsTrue(_service.TryCheckIn(_table).Success);
            Assert.IsTrue(_service.IsMonthlyDayClaimed(1));
            Assert.Greater(_wallet.GetBalance(CurrencyType.Gold), goldAfterAugust);
        }

        [Test]
        public void AllRewards_UseWalletAndLedger()
        {
            _service.TryCheckIn(_table);

            Assert.Greater(_ledger.Entries.Count, 0);
            foreach (var entry in _ledger.Entries)
            {
                Assert.AreEqual(CurrencyReasonCodes.AttendanceReward, entry.ReasonCode);
                Assert.Greater(entry.Delta, 0L);
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
    }
}
