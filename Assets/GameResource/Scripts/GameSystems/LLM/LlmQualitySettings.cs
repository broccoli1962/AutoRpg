using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Save;
using UnityEngine;

namespace Backend.GameSystems.LLM
{
    public enum LlmQualityMode
    {
        High,
        Balanced,
        Performance
    }

    /// <summary>
    /// 로컬 LLM 품질/성능 프리셋. Phase 6 저사양 대응 옵션.
    /// </summary>
    public static class LlmQualitySettings
    {
        private const string PrefKey = "abyss_llm_quality_mode";

        public static LlmQualityMode Current
        {
            get => (LlmQualityMode)PlayerPrefs.GetInt(PrefKey, (int)LlmQualityMode.Balanced);
            set
            {
                PlayerPrefs.SetInt(PrefKey, (int)value);
                PlayerPrefs.Save();
            }
        }

        public static bool UseDynamicEventLlm => Current != LlmQualityMode.Performance;

        public static bool UseDynamicEventResultLlm => Current == LlmQualityMode.High;

        public static bool ShouldUseLogLlm(ExplorationEvent explorationEvent)
        {
            if (explorationEvent == null)
                return false;

            if (Current == LlmQualityMode.Performance &&
                explorationEvent.Salience < SalienceGrade.Milestone)
            {
                return false;
            }

            return true;
        }

        public static float InferenceTimeoutSeconds => Current switch
        {
            LlmQualityMode.High => 10f,
            LlmQualityMode.Balanced => 8f,
            _ => 5f
        };

        public static int GetLogMaxTokens(SalienceGrade salience)
        {
            if (salience >= SalienceGrade.Milestone)
            {
                return Current switch
                {
                    LlmQualityMode.High => 180,
                    LlmQualityMode.Balanced => 128,
                    _ => 72
                };
            }

            return Current switch
            {
                LlmQualityMode.High => 128,
                LlmQualityMode.Balanced => 96,
                _ => 64
            };
        }

        public static int DynamicSceneMaxTokens => Current switch
        {
            LlmQualityMode.High => 220,
            LlmQualityMode.Balanced => 160,
            _ => 0
        };

        public static int DynamicResultMaxTokens => Current switch
        {
            LlmQualityMode.High => 96,
            _ => 0
        };

        public static void CycleMode()
        {
            Current = Current switch
            {
                LlmQualityMode.High => LlmQualityMode.Balanced,
                LlmQualityMode.Balanced => LlmQualityMode.Performance,
                _ => LlmQualityMode.High
            };

            Debug.Log($"[LlmQualitySettings] Mode changed to {Current}");
            GameSaveManager.Save();
        }

        public static string GetDisplayLabel()
        {
            return Current switch
            {
                LlmQualityMode.High => "LLM:고품질",
                LlmQualityMode.Balanced => "LLM:균형",
                _ => "LLM:성능"
            };
        }

        public static int ExportMode() => (int)Current;

        public static void ImportMode(int mode)
        {
            var clamped = Mathf.Clamp(mode, (int)LlmQualityMode.High, (int)LlmQualityMode.Performance);
            Current = (LlmQualityMode)clamped;
        }
    }
}
