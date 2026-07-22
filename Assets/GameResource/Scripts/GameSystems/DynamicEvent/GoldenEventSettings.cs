using Backend.GameSystems.Save;
using UnityEngine;

namespace Backend.GameSystems.DynamicEvent
{
    /// <summary>
    /// 황금(희귀) 동적 이벤트 자동정지 옵션 (12_UIUX.md).
    /// </summary>
    public static class GoldenEventSettings
    {
        private const string PrefKey = "abyss_golden_event_auto_pause";

        public static bool AutoPauseOnGolden
        {
            get => PlayerPrefs.GetInt(PrefKey, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(PrefKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static void ToggleAutoPause()
        {
            AutoPauseOnGolden = !AutoPauseOnGolden;
            Debug.Log($"[GoldenEventSettings] AutoPauseOnGolden={AutoPauseOnGolden}");
            GameSaveManager.Save();
        }

        public static string GetDisplayLabel() =>
            AutoPauseOnGolden ? "황금:정지" : "황금:자동";

        public static int ExportSetting() => AutoPauseOnGolden ? 1 : 0;

        public static void ImportSetting(int value)
        {
            AutoPauseOnGolden = value != 0;
        }
    }
}
