using System;
using System.Globalization;

namespace Backend.Util.Localization
{
    /// <summary>
    /// 현재 게임 언어에 맞는 숫자·날짜·재화 표기 유틸.
    /// </summary>
    public static class LocaleFormatUtil
    {
        /// <summary>
        /// 현재 언어의 CultureInfo 를 반환한다.
        /// </summary>
        public static CultureInfo CurrentCulture => LocalizationService.CurrentCulture;

        /// <summary>
        /// 정수를 로케일 구분 기호로 포맷한다.
        /// </summary>
        public static string FormatNumber(long value)
        {
            return value.ToString("N0", CurrentCulture);
        }

        /// <summary>
        /// 실수를 로케일 구분 기호로 포맷한다.
        /// </summary>
        public static string FormatNumber(double value, int decimalPlaces = 0)
        {
            var format = decimalPlaces <= 0 ? "N0" : $"N{decimalPlaces}";
            return value.ToString(format, CurrentCulture);
        }

        /// <summary>
        /// 대형 숫자를 축약 단위로 포맷한다.
        /// </summary>
        public static string FormatCompactNumber(long value, int decimalPlaces = 1)
        {
            return LargeNumberFormatter.Format(value, decimalPlaces);
        }

        /// <summary>
        /// 재화 금액을 로케일에 맞게 포맷한다.
        /// </summary>
        public static string FormatCurrency(long amount, string currencySymbol = null)
        {
            var symbol = currencySymbol ?? ResolveCurrencySymbol();
            return $"{symbol}{FormatNumber(amount)}";
        }

        /// <summary>
        /// 날짜·시간을 로케일에 맞게 포맷한다.
        /// </summary>
        public static string FormatDateTime(DateTime dateTime, bool includeTime = true)
        {
            var format = includeTime ? "g" : "d";
            return dateTime.ToString(format, CurrentCulture);
        }

        /// <summary>
        /// 경과 시간을 사람이 읽기 쉬운 문자열로 포맷한다.
        /// </summary>
        public static string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalDays >= 1d)
            {
                return LocalizationService.Get(
                    "format.duration.days_hours",
                    (int)duration.TotalDays,
                    duration.Hours);
            }

            if (duration.TotalHours >= 1d)
            {
                return LocalizationService.Get(
                    "format.duration.hours_minutes",
                    (int)duration.TotalHours,
                    duration.Minutes);
            }

            return LocalizationService.Get(
                "format.duration.minutes",
                Math.Max(1, (int)Math.Ceiling(duration.TotalMinutes)));
        }

        private static string ResolveCurrencySymbol()
        {
            return LocalizationService.CurrentLanguage switch
            {
                GameLanguage.Korean => "₩",
                GameLanguage.Japanese => "¥",
                _ => "$",
            };
        }
    }
}
