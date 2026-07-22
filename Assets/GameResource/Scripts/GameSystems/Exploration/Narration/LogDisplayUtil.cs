using Backend.GameSystems.Exploration.Data;
using UnityEngine;

namespace Backend.GameSystems.Exploration.Narration
{
    /// <summary>
    /// 로그 피드의 색상·아이콘·리치 텍스트 포맷을 일원화한다.
    /// </summary>
    public static class LogDisplayUtil
    {
        private static readonly Color MilestoneAccent = new(0.75f, 0.55f, 0.98f);
        private static readonly Color SignificantAccent = new(1f, 0.92f, 0.72f);

        public static Color GetCategoryColor(LogCategory category)
        {
            return category switch
            {
                LogCategory.Move => new Color(0.65f, 0.65f, 0.65f),
                LogCategory.Combat => new Color(0.85f, 0.35f, 0.35f),
                LogCategory.Discovery => new Color(0.9f, 0.75f, 0.25f),
                LogCategory.Status => new Color(0.35f, 0.65f, 0.9f),
                LogCategory.Milestone => new Color(0.7f, 0.45f, 0.9f),
                _ => Color.white
            };
        }

        public static Color GetDisplayColor(LogCategory category, SalienceGrade salience)
        {
            var baseColor = GetCategoryColor(category);
            return salience switch
            {
                SalienceGrade.Milestone => Blend(baseColor, MilestoneAccent, 0.45f),
                SalienceGrade.Significant => Brighten(baseColor, 1.1f),
                SalienceGrade.Notable => baseColor,
                _ => Dim(baseColor, 0.78f)
            };
        }

        public static string GetCategoryIcon(LogCategory category)
        {
            return category switch
            {
                LogCategory.Move => "▸ ",
                LogCategory.Combat => "⚔ ",
                LogCategory.Discovery => "✦ ",
                LogCategory.Status => "♥ ",
                LogCategory.Milestone => "★ ",
                _ => string.Empty
            };
        }

        public static string GetSaliencePrefix(SalienceGrade salience) =>
            salience switch
            {
                SalienceGrade.Milestone => "◆ ",
                SalienceGrade.Significant => "● ",
                _ => string.Empty
            };

        public static string FormatRichText(LogEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.Text))
                return string.Empty;

            var color = ColorUtility.ToHtmlStringRGB(GetDisplayColor(entry.Category, entry.Salience));
            var icon = GetCategoryIcon(entry.Category);
            var prefix = GetSaliencePrefix(entry.Salience);
            var text = entry.Text;

            if (entry.IsPending)
                text = $"<i>{text}<color=#888888>▌</color></i>";

            if (entry.Salience >= SalienceGrade.Milestone || entry.Category == LogCategory.Milestone)
            {
                var accent = ColorUtility.ToHtmlStringRGB(MilestoneAccent);
                return $"<color=#{accent}>▌</color><color=#{color}><b>{prefix}{icon}{text}</b></color><color=#{accent}> ▌</color>";
            }

            if (entry.Salience >= SalienceGrade.Significant)
                return $"<color=#{color}><b>{prefix}{icon}{text}</b></color>";

            if (entry.Salience >= SalienceGrade.Notable)
                return $"<color=#{color}>{prefix}{icon}{text}</color>";

            return $"<color=#{color}>{icon}{text}</color>";
        }

        public static string FormatTaggedLine(string tag, string text, Color color)
        {
            var hex = ColorUtility.ToHtmlStringRGB(color);
            return $"<color=#{hex}>[{tag}] {text}</color>";
        }

        private static Color Dim(Color color, float factor)
        {
            return new Color(color.r * factor, color.g * factor, color.b * factor, color.a);
        }

        private static Color Brighten(Color color, float factor)
        {
            return new Color(
                Mathf.Min(1f, color.r * factor),
                Mathf.Min(1f, color.g * factor),
                Mathf.Min(1f, color.b * factor),
                color.a);
        }

        private static Color Blend(Color a, Color b, float t)
        {
            t = Mathf.Clamp01(t);
            return Color.Lerp(a, b, t);
        }
    }
}
