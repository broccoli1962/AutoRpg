namespace Backend.GameSystems.Performance
{
    /// <summary>
    /// 품질 프리셋 사용자 설정을 PlayerPrefs 에 저장한다.
    /// </summary>
    public static class PerformanceSettingsStore
    {
        private const string PREF_QUALITY_PRESET = "abyss_quality_preset";

        /// <summary>
        /// 저장된 품질 프리셋을 읽는다. 기본값은 Auto.
        /// </summary>
        public static QualityPreset LoadPreset()
        {
            var raw = UnityEngine.PlayerPrefs.GetInt(PREF_QUALITY_PRESET, (int)QualityPreset.Auto);
            if (raw < (int)QualityPreset.Auto || raw > (int)QualityPreset.Recommended)
                return QualityPreset.Auto;

            return (QualityPreset)raw;
        }

        /// <summary>
        /// 품질 프리셋을 저장한다.
        /// </summary>
        public static void SavePreset(QualityPreset preset)
        {
            UnityEngine.PlayerPrefs.SetInt(PREF_QUALITY_PRESET, (int)preset);
            UnityEngine.PlayerPrefs.Save();
        }
    }
}
