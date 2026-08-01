using System;
using System.Globalization;

namespace Backend.Meta.Retention
{
    /// <summary>
    /// 서버 UTC 기준 일일·주간 갱신 경계. KST 04:00 를 게임 일자/주차 시작으로 사용한다.
    /// </summary>
    public static class DailyResetClock
    {
        public const int RESET_HOUR_KST = 4;

        private static readonly TimeSpan KstOffset = TimeSpan.FromHours(9);

        /// <summary>
        /// UTC 시각을 KST 04:00 경계가 적용된 게임 일자로 변환한다.
        /// </summary>
        public static DateTime GetGameDate(DateTimeOffset utcNow)
        {
            var kst = utcNow.ToOffset(KstOffset);
            var adjusted = kst.AddHours(-RESET_HOUR_KST);
            return adjusted.Date;
        }

        /// <summary>
        /// 게임 일자 키(yyyyMMdd)를 반환한다.
        /// </summary>
        public static int GetDayKey(DateTimeOffset utcNow)
        {
            var date = GetGameDate(utcNow);
            return date.Year * 10_000 + date.Month * 100 + date.Day;
        }

        /// <summary>
        /// ISO 주차 키(yyyyWW)를 반환한다. 주 시작은 월요일 04:00 KST.
        /// </summary>
        public static int GetWeekKey(DateTimeOffset utcNow)
        {
            var gameDate = GetGameDate(utcNow);
            GetIsoWeekOfYear(gameDate, out var isoYear, out var isoWeek);
            return isoYear * 100 + isoWeek;
        }

        /// <summary>
        /// 게임 월 키(yyyyMM)를 반환한다.
        /// </summary>
        public static int GetMonthKey(DateTimeOffset utcNow)
        {
            var gameDate = GetGameDate(utcNow);
            return gameDate.Year * 100 + gameDate.Month;
        }

        /// <summary>
        /// 월간 출석 캘린더 일차(1~28)를 반환한다. 29~31일은 28일 슬롯으로 취급한다.
        /// </summary>
        public static int GetMonthlyCalendarDay(DateTimeOffset utcNow)
        {
            var day = GetGameDate(utcNow).Day;
            return day > 28 ? 28 : day;
        }

        /// <summary>
        /// 두 UTC 시각이 같은 게임 일자인지 판정한다.
        /// </summary>
        public static bool IsSameGameDay(DateTimeOffset leftUtc, DateTimeOffset rightUtc)
        {
            return GetDayKey(leftUtc) == GetDayKey(rightUtc);
        }

        /// <summary>
        /// 직전 게임 일자 키를 반환한다.
        /// </summary>
        public static int GetPreviousDayKey(DateTimeOffset utcNow)
        {
            var previousDate = GetGameDate(utcNow).AddDays(-1);
            return previousDate.Year * 10_000 + previousDate.Month * 100 + previousDate.Day;
        }

        private static void GetIsoWeekOfYear(DateTime date, out int isoYear, out int isoWeek)
        {
            var calendar = CultureInfo.InvariantCulture.Calendar;
            isoWeek = calendar.GetWeekOfYear(
                date,
                CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);

            isoYear = date.Year;

            if (date.Month == 1 && isoWeek >= 52)
                isoYear = date.Year - 1;
            else if (date.Month == 12 && isoWeek == 1)
                isoYear = date.Year + 1;
        }
    }
}
