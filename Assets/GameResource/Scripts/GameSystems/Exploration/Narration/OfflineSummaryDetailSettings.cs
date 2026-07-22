using Backend.GameSystems.Save;
using UnityEngine;

namespace Backend.GameSystems.Exploration.Narration
{
    public enum OfflineSummaryDetailMode
    {
        Brief,
        Detailed
    }

    /// <summary>
    /// 오프라인 탐험 요약 상세도 (12_UIUX.md 설정).
    /// </summary>
    public static class OfflineSummaryDetailSettings
    {
        private const string PrefKey = "abyss_offline_summary_detail";

        public static OfflineSummaryDetailMode Current
        {
            get => (OfflineSummaryDetailMode)PlayerPrefs.GetInt(PrefKey, (int)OfflineSummaryDetailMode.Brief);
            set
            {
                PlayerPrefs.SetInt(PrefKey, (int)value);
                PlayerPrefs.Save();
            }
        }

        public static bool IsDetailed => Current == OfflineSummaryDetailMode.Detailed;

        public static void ToggleMode()
        {
            Current = Current == OfflineSummaryDetailMode.Brief
                ? OfflineSummaryDetailMode.Detailed
                : OfflineSummaryDetailMode.Brief;

            Debug.Log($"[OfflineSummaryDetailSettings] Mode changed to {Current}");
            GameSaveManager.Save();
        }

        public static string GetDisplayLabel()
        {
            return Current switch
            {
                OfflineSummaryDetailMode.Detailed => "오프라인:상세",
                _ => "오프라인:간략"
            };
        }

        public static int ExportMode() => (int)Current;

        public static void ImportMode(int mode)
        {
            var clamped = Mathf.Clamp(mode, (int)OfflineSummaryDetailMode.Brief, (int)OfflineSummaryDetailMode.Detailed);
            Current = (OfflineSummaryDetailMode)clamped;
        }
    }
}
