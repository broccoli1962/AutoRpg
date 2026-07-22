using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Save;
using UnityEngine;

namespace Backend.GameSystems.Exploration.Narration
{
    public enum LogFrequencyMode
    {
        Low,
        Normal,
        High
    }

    /// <summary>
    /// 로그 피드 노출 빈도 — Salience 임계값과 연결 (12_UIUX.md).
    /// </summary>
    public static class LogFrequencySettings
    {
        private const string PrefKey = "abyss_log_frequency";

        public static LogFrequencyMode Current
        {
            get => (LogFrequencyMode)PlayerPrefs.GetInt(PrefKey, (int)LogFrequencyMode.Normal);
            set
            {
                PlayerPrefs.SetInt(PrefKey, (int)value);
                PlayerPrefs.Save();
            }
        }

        public static bool ShouldPublishLog(ExplorationEvent explorationEvent)
        {
            if (explorationEvent == null)
                return false;

            if (explorationEvent.EventType == EventType.OfflineSummary)
                return true;

            return explorationEvent.Salience >= GetMinimumSalience();
        }

        public static SalienceGrade GetMinimumSalience()
        {
            return Current switch
            {
                LogFrequencyMode.Low => SalienceGrade.Significant,
                LogFrequencyMode.High => SalienceGrade.Trivial,
                _ => SalienceGrade.Notable
            };
        }

        public static void CycleMode()
        {
            Current = Current switch
            {
                LogFrequencyMode.Low => LogFrequencyMode.Normal,
                LogFrequencyMode.Normal => LogFrequencyMode.High,
                _ => LogFrequencyMode.Low
            };

            Debug.Log($"[LogFrequencySettings] Mode changed to {Current}");
            GameSaveManager.Save();
        }

        public static string GetDisplayLabel()
        {
            return Current switch
            {
                LogFrequencyMode.Low => "로그:낮음",
                LogFrequencyMode.High => "로그:높음",
                _ => "로그:보통"
            };
        }

        public static int ExportMode() => (int)Current;

        public static void ImportMode(int mode)
        {
            var clamped = Mathf.Clamp(mode, (int)LogFrequencyMode.Low, (int)LogFrequencyMode.High);
            Current = (LogFrequencyMode)clamped;
        }
    }
}
