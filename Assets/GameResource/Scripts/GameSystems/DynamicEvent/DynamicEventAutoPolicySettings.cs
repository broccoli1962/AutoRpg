using Backend.GameSystems.Save;
using UnityEngine;

namespace Backend.GameSystems.DynamicEvent
{
    public enum DynamicEventAutoPolicy
    {
        Safe,
        Adventure,
        Greedy,
        Personality
    }

    /// <summary>
    /// 동적 이벤트 자동 선택 정책 (12_UIUX.md 설정 화면).
    /// </summary>
    public static class DynamicEventAutoPolicySettings
    {
        private const string PrefKey = "abyss_dynamic_event_auto_policy";

        public static DynamicEventAutoPolicy Current
        {
            get => (DynamicEventAutoPolicy)PlayerPrefs.GetInt(PrefKey, (int)DynamicEventAutoPolicy.Personality);
            set
            {
                PlayerPrefs.SetInt(PrefKey, (int)value);
                PlayerPrefs.Save();
            }
        }

        public static void CyclePolicy()
        {
            Current = Current switch
            {
                DynamicEventAutoPolicy.Safe => DynamicEventAutoPolicy.Adventure,
                DynamicEventAutoPolicy.Adventure => DynamicEventAutoPolicy.Greedy,
                DynamicEventAutoPolicy.Greedy => DynamicEventAutoPolicy.Personality,
                _ => DynamicEventAutoPolicy.Safe
            };

            Debug.Log($"[DynamicEventAutoPolicySettings] Policy changed to {Current}");
            GameSaveManager.Save();
        }

        public static string GetDisplayLabel()
        {
            return Current switch
            {
                DynamicEventAutoPolicy.Safe => "이벤트:안전",
                DynamicEventAutoPolicy.Adventure => "이벤트:모험",
                DynamicEventAutoPolicy.Greedy => "이벤트:탐욕",
                _ => "이벤트:성격"
            };
        }

        public static int ExportPolicy() => (int)Current;

        public static void ImportPolicy(int policy)
        {
            var clamped = Mathf.Clamp(policy, (int)DynamicEventAutoPolicy.Safe, (int)DynamicEventAutoPolicy.Personality);
            Current = (DynamicEventAutoPolicy)clamped;
        }
    }
}
