using System;
using System.Globalization;

namespace Backend.Util.Localization
{
    /// <summary>
    /// 방치형 RPG 스케일용 대형 숫자 축약(K/M/B/T 및 이후 단위) 포맷터.
    /// </summary>
    public static class LargeNumberFormatter
    {
        private static readonly string[] Suffixes =
        {
            string.Empty,
            "K",
            "M",
            "B",
            "T",
            "Qa",
            "Qi",
            "Sx",
            "Sp",
            "Oc",
            "No",
            "Dc",
        };

        /// <summary>
        /// 값을 축약 단위로 포맷한다. 1,000 미만은 정수 그대로 반환한다.
        /// </summary>
        public static string Format(double value, int decimalPlaces = 1)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                return "0";

            var sign = value < 0 ? "-" : string.Empty;
            var absolute = Math.Abs(value);

            if (absolute < 1_000d)
                return sign + Math.Round(absolute).ToString(CultureInfo.InvariantCulture);

            var tier = 0;
            var scaled = absolute;

            while (scaled >= 1_000d && tier < Suffixes.Length - 1)
            {
                scaled /= 1_000d;
                tier++;
            }

            var format = decimalPlaces <= 0
                ? "0"
                : "0." + new string('#', decimalPlaces);

            return sign + scaled.ToString(format, CultureInfo.InvariantCulture) + Suffixes[tier];
        }

        /// <summary>
        /// long 값을 축약 단위로 포맷한다.
        /// </summary>
        public static string Format(long value, int decimalPlaces = 1)
        {
            return Format((double)value, decimalPlaces);
        }
    }
}
