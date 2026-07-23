using TMPro;
using UnityEngine;

namespace Backend.Util
{
    /// <summary>
    /// 12_UIUX v2 타이포·컬러 토큰 (TextMeshPro).
    /// </summary>
    public static class ModernUiStyle
    {
        public static readonly Color TitleGold = Hex("#F5D673");
        public static readonly Color TitleMedium = Hex("#E8EDF5");
        public static readonly Color BodyText = Hex("#E0E6F0");
        public static readonly Color MutedText = Hex("#9AA8BC");
        public static readonly Color AccentCyan = Hex("#6ECFF5");
        public static readonly Color AccentGreen = Hex("#6EE08A");
        public static readonly Color DangerRed = Hex("#F26D5B");
        public static readonly Color ButtonPrimaryText = Hex("#1A1408");
        public static readonly Color AppBackground = Hex("#0B0E14");
        public static readonly Color PanelStroke = Hex("#3A4A66");
        public static readonly Color HpTrack = Hex("#1A2030");

        public static void ApplyTitleLarge(TMP_Text text)
        {
            if (text == null)
                return;

            text.font = RuntimeUiTmpFont.Get();
            text.fontSize = 26;
            text.fontStyle = FontStyles.Bold;
            text.color = TitleGold;
            text.richText = true;
        }

        public static void ApplyTitleMedium(TMP_Text text)
        {
            if (text == null)
                return;

            text.font = RuntimeUiTmpFont.Get();
            text.fontSize = 20;
            text.fontStyle = FontStyles.Bold;
            text.color = TitleMedium;
            text.richText = true;
        }

        public static void ApplyBody(TMP_Text text, int fontSize = 16)
        {
            if (text == null)
                return;

            text.font = RuntimeUiTmpFont.Get();
            text.fontSize = fontSize;
            text.color = BodyText;
            text.richText = true;
        }

        public static void ApplyMuted(TMP_Text text, int fontSize = 14)
        {
            if (text == null)
                return;

            text.font = RuntimeUiTmpFont.Get();
            text.fontSize = fontSize;
            text.color = MutedText;
            text.richText = true;
        }

        public static void ApplyButtonLabel(TMP_Text text, bool primary)
        {
            if (text == null)
                return;

            text.font = RuntimeUiTmpFont.Get();
            text.fontSize = 27;
            text.fontStyle = FontStyles.Bold;
            text.color = primary ? ButtonPrimaryText : BodyText;
            text.richText = false;
        }

        public static void ApplyTabLabel(TMP_Text text, bool active)
        {
            if (text == null)
                return;

            text.font = RuntimeUiTmpFont.Get();
            text.fontSize = 23;
            text.color = active ? TitleGold : MutedText;
            text.richText = false;
        }

        public static void ApplySectionHeader(TMP_Text text, string label)
        {
            ApplyTitleMedium(text);
            text.text = label;
        }

        public static void ApplyTitle(TMP_Text text, int fontSize = 26) => ApplyTitleLarge(text);

        private static Color Hex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out var color))
                return color;

            return Color.white;
        }
    }
}
