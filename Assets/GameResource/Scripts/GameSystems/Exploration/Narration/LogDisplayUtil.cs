using Backend.GameSystems.Exploration.Data;
using UnityEngine;

namespace Backend.GameSystems.Exploration.Narration
{
    /// <summary>
    /// 로그 피드의 색상·아이콘·리치 텍스트 포맷을 일원화한다.
    /// </summary>
    public static class LogDisplayUtil
    {
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

        public static string FormatRichText(LogEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.Text))
                return string.Empty;

            var color = ColorUtility.ToHtmlStringRGB(GetCategoryColor(entry.Category));
            var icon = GetCategoryIcon(entry.Category);
            var text = entry.Text;

            if (entry.IsPending)
                text = $"<i>{text}<color=#888888>▌</color></i>";

            if (entry.Salience >= SalienceGrade.Milestone || entry.Category == LogCategory.Milestone)
                return $"<color=#{color}><b>{icon}{text}</b></color>";

            return $"<color=#{color}>{icon}{text}</color>";
        }

        public static string FormatTaggedLine(string tag, string text, Color color)
        {
            var hex = ColorUtility.ToHtmlStringRGB(color);
            return $"<color=#{hex}>[{tag}] {text}</color>";
        }
    }
}
