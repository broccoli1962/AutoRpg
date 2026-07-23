using Backend.GameSystems.Save;
using UnityEngine;

namespace Backend.GameSystems.Exploration.Stage
{
    public enum StageVfxDensityMode
    {
        Low,
        Normal,
        High
    }

    /// <summary>
    /// 스테이지 VFX·플로팅 밀도 (Phase 7 — 저사양 대응).
    /// </summary>
    public static class StageVfxDensitySettings
    {
        private const string PrefKey = "abyss_stage_vfx_density";

        public static StageVfxDensityMode Current
        {
            get => (StageVfxDensityMode)PlayerPrefs.GetInt(PrefKey, (int)StageVfxDensityMode.Normal);
            set
            {
                PlayerPrefs.SetInt(PrefKey, (int)value);
                PlayerPrefs.Save();
            }
        }

        public static bool ShowSlashVfx => Current != StageVfxDensityMode.Low;

        public static bool ShowPartyDamageFloaters => Current == StageVfxDensityMode.High;

        public static float HitIntervalMultiplier => Current switch
        {
            StageVfxDensityMode.Low => 1.35f,
            StageVfxDensityMode.High => 0.82f,
            _ => 1f
        };

        public static int CapCombatHitCount(int hitCount) =>
            Current switch
            {
                StageVfxDensityMode.Low => Mathf.Min(hitCount, 2),
                StageVfxDensityMode.High => Mathf.Min(hitCount + 1, 5),
                _ => hitCount
            };

        public static float ParallaxShiftMultiplier => Current switch
        {
            StageVfxDensityMode.Low => 0.6f,
            StageVfxDensityMode.High => 1.2f,
            _ => 1f
        };

        public static void CycleMode()
        {
            Current = Current switch
            {
                StageVfxDensityMode.Low => StageVfxDensityMode.Normal,
                StageVfxDensityMode.Normal => StageVfxDensityMode.High,
                _ => StageVfxDensityMode.Low
            };

            Debug.Log($"[StageVfxDensitySettings] Mode changed to {Current}");
            GameSaveManager.Save();
        }

        public static string GetDisplayLabel() =>
            Current switch
            {
                StageVfxDensityMode.Low => "연출:낮음",
                StageVfxDensityMode.High => "연출:높음",
                _ => "연출:보통"
            };

        public static int ExportMode() => (int)Current;

        public static void ImportMode(int mode)
        {
            var clamped = Mathf.Clamp(mode, (int)StageVfxDensityMode.Low, (int)StageVfxDensityMode.High);
            Current = (StageVfxDensityMode)clamped;
        }
    }
}
