using System;
using System.Collections.Generic;

namespace Backend.Meta.Achievements
{
    /// <summary>
    /// Remote Config 에서 내려온 업적 보상 오버라이드를 적용한다.
    /// </summary>
    public sealed class AchievementRemoteConfigOverlay
    {
        private readonly Dictionary<string, long> _tierRewardOverrides = new();
        private double _globalRewardMultiplier = 1.0;

        /// <summary>
        /// 전역 보상 배율을 설정한다.
        /// </summary>
        public void SetGlobalRewardMultiplier(double multiplier)
        {
            _globalRewardMultiplier = multiplier > 0.0 ? multiplier : 1.0;
        }

        /// <summary>
        /// 단계별 보상을 Remote Config 값으로 덮어쓴다.
        /// </summary>
        public void SetTierRewardOverride(string remoteConfigKey, long amount)
        {
            if (string.IsNullOrEmpty(remoteConfigKey))
                return;

            if (amount < 0L)
                return;

            _tierRewardOverrides[remoteConfigKey] = amount;
        }

        /// <summary>
        /// Remote Config key-value 맵을 파싱해 오버레이에 반영한다.
        /// </summary>
        public void ApplyRemoteValues(IReadOnlyDictionary<string, string> remoteValues)
        {
            if (remoteValues == null)
                return;

            foreach (var pair in remoteValues)
            {
                if (pair.Key == AchievementRemoteConfigKeys.GlobalRewardMultiplier)
                {
                    if (double.TryParse(pair.Value, out var multiplier))
                        SetGlobalRewardMultiplier(multiplier);
                    continue;
                }

                if (!pair.Key.StartsWith("achievement_reward_", StringComparison.Ordinal))
                    continue;

                if (!long.TryParse(pair.Value, out var amount))
                    continue;

                SetTierRewardOverride(pair.Key, amount);
            }
        }

        /// <summary>
        /// 테이블 기본 보상과 오버레이를 합쳐 최종 심연석 보상량을 계산한다.
        /// </summary>
        public long ResolveReward(AchievementTierDefinition tier)
        {
            if (tier == null)
                return 0L;

            if (!string.IsNullOrEmpty(tier.RemoteConfigKey)
                && _tierRewardOverrides.TryGetValue(tier.RemoteConfigKey, out var overrideAmount))
            {
                return overrideAmount;
            }

            return (long)Math.Round(tier.BaseAbyssStoneReward * _globalRewardMultiplier);
        }
    }
}
