using UnityEngine;
using UnityEngine.UI;

namespace Backend.Util
{
    /// <summary>
    /// 12_UIUX v2 타이포·컬러 토큰.
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

        public static void ApplyTitleLarge(Text text)
        {
            if (text == null)
                return;

            text.font = RuntimeUiFont.Get();
            text.fontSize = 24;
            text.fontStyle = FontStyle.Bold;
            text.color = TitleGold;
            text.supportRichText = true;
        }

        public static void ApplyTitleMedium(Text text)
        {
            if (text == null)
                return;

            text.font = RuntimeUiFont.Get();
            text.fontSize = 18;
            text.fontStyle = FontStyle.Bold;
            text.color = TitleMedium;
            text.supportRichText = true;
        }

        public static void ApplyBody(Text text, int fontSize = 15)
        {
            if (text == null)
                return;

            text.font = RuntimeUiFont.Get();
            text.fontSize = fontSize;
            text.color = BodyText;
            text.supportRichText = true;
        }

        public static void ApplyMuted(Text text, int fontSize = 13)
        {
            if (text == null)
                return;

            text.font = RuntimeUiFont.Get();
            text.fontSize = fontSize;
            text.color = MutedText;
            text.supportRichText = true;
        }

        public static void ApplyButtonLabel(Text text, bool primary)
        {
            if (text == null)
                return;

            text.font = RuntimeUiFont.Get();
            text.fontSize = 16;
            text.fontStyle = FontStyle.Bold;
            text.color = primary ? ButtonPrimaryText : BodyText;
            text.supportRichText = false;
        }

        public static void ApplyTabLabel(Text text, bool active)
        {
            if (text == null)
                return;

            text.font = RuntimeUiFont.Get();
            text.fontSize = 11;
            text.color = active ? TitleGold : MutedText;
            text.supportRichText = false;
        }

        public static void ApplySectionHeader(Text text, string label)
        {
            ApplyTitleMedium(text);
            text.text = label;
        }

        public static void ApplyTitle(Text text, int fontSize = 24) => ApplyTitleLarge(text);

        private static Color Hex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out var color))
                return color;

            return Color.white;
        }
    }
}
