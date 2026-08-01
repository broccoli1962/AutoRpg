using System;
using Backend.GameSystems.Offline;
using NUnit.Framework;

namespace Backend.Meta.Retention.Tests
{
    public class DailyResetClockTests
    {
        [Test]
        public void GetDayKey_BeforeKstReset_UsesPreviousGameDay()
        {
            // KST 2026-08-01 03:59 → game date 2026-07-31
            var utc = new DateTimeOffset(2026, 7, 31, 18, 59, 0, TimeSpan.Zero);
            Assert.AreEqual(20_260_731, DailyResetClock.GetDayKey(utc));
        }

        [Test]
        public void GetDayKey_AtKstReset_StartsNewGameDay()
        {
            // KST 2026-08-01 04:00 → game date 2026-08-01
            var utc = new DateTimeOffset(2026, 7, 31, 19, 0, 0, TimeSpan.Zero);
            Assert.AreEqual(20_260_801, DailyResetClock.GetDayKey(utc));
        }

        [Test]
        public void GetDayKey_UsesServerUtc_NotLocalOffset()
        {
            var utc = new DateTimeOffset(2026, 7, 31, 19, 30, 0, TimeSpan.Zero);
            Assert.AreEqual(20_260_801, DailyResetClock.GetDayKey(utc));
        }

        [Test]
        public void GetMonthlyCalendarDay_CapsAt28()
        {
            // KST 2026-01-31 12:00
            var utc = new DateTimeOffset(2026, 1, 31, 3, 0, 0, TimeSpan.Zero);
            Assert.AreEqual(28, DailyResetClock.GetMonthlyCalendarDay(utc));
        }

        [Test]
        public void GetWeekKey_DiffersAcrossMultipleWeeks()
        {
            var earlyYear = new DateTimeOffset(2026, 1, 5, 19, 0, 0, TimeSpan.Zero);
            var midYear = new DateTimeOffset(2026, 5, 18, 19, 0, 0, TimeSpan.Zero);

            Assert.AreNotEqual(
                DailyResetClock.GetWeekKey(earlyYear),
                DailyResetClock.GetWeekKey(midYear));
        }

        [Test]
        public void GetWeekKey_IsConsistentWithinSameGameWeek()
        {
            var monday = new DateTimeOffset(2026, 5, 18, 19, 0, 0, TimeSpan.Zero);
            var wednesday = new DateTimeOffset(2026, 5, 20, 19, 0, 0, TimeSpan.Zero);

            Assert.AreEqual(
                DailyResetClock.GetWeekKey(monday),
                DailyResetClock.GetWeekKey(wednesday));
        }

        [Test]
        public void IsSameGameDay_RespectsKstResetBoundary()
        {
            var beforeReset = new DateTimeOffset(2026, 7, 31, 18, 59, 0, TimeSpan.Zero);
            var afterReset = new DateTimeOffset(2026, 7, 31, 19, 0, 0, TimeSpan.Zero);

            Assert.IsTrue(DailyResetClock.IsSameGameDay(beforeReset, beforeReset));
            Assert.IsFalse(DailyResetClock.IsSameGameDay(beforeReset, afterReset));
        }
    }
}
