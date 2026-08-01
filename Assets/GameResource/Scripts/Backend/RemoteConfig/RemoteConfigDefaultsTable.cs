using System;
using System.Collections.Generic;
using UnityEngine;

namespace Backend.Services.RemoteConfig
{
    /// <summary>
    /// Remote Config 번들 기본값 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "RemoteConfigDefaultsTable", menuName = "Abyss Chronicle/Remote Config Defaults")]
    public sealed class RemoteConfigDefaultsTable : ScriptableObject
    {
        [SerializeField] private RemoteConfigDefaultEntry[] _entries = Array.Empty<RemoteConfigDefaultEntry>();

        /// <summary>
        /// 기본값 맵을 반환한다.
        /// </summary>
        public IReadOnlyDictionary<string, string> ToDictionary()
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            if (_entries == null)
                return map;

            foreach (var entry in _entries)
            {
                if (string.IsNullOrEmpty(entry.Key))
                    continue;

                map[entry.Key] = entry.Value ?? string.Empty;
            }

            return map;
        }

        /// <summary>
        /// spec 기본값으로 직렬화 필드를 채운다.
        /// </summary>
        public void ApplySpecDefaults()
        {
            _entries = new[]
            {
                Entry(RemoteConfigKeys.MonsterHpGrowth, "1.135"),
                Entry(RemoteConfigKeys.MonsterAtkGrowth, "1.130"),
                Entry(RemoteConfigKeys.MonsterDefGrowth, "1.130"),
                Entry(RemoteConfigKeys.GoldDropGrowth, "1.120"),
                Entry(RemoteConfigKeys.UpgradeCostGrowth, "1.080"),
                Entry(RemoteConfigKeys.TotalRewardedDailyLimit, "15"),
                Entry(RemoteConfigKeys.InterstitialDailyLimit, "6"),
                Entry(RemoteConfigKeys.AchievementGlobalRewardMultiplier, "1.0"),
                Entry(RemoteConfigKeys.EventSeasonPassEnabled, "true"),
                Entry(RemoteConfigKeys.EventGachaBannerEnabled, "true"),
            };
        }

        private static RemoteConfigDefaultEntry Entry(string key, string value)
        {
            return new RemoteConfigDefaultEntry { Key = key, Value = value };
        }
    }

    /// <summary>
    /// Remote Config 기본값 1건.
    /// </summary>
    [Serializable]
    public struct RemoteConfigDefaultEntry
    {
        public string Key;
        public string Value;
    }
}
