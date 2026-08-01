using System;
using System.Collections.Generic;
using Backend.Meta.Currency;
using UnityEngine;

namespace Backend.Meta.SeasonPass
{
    /// <summary>
    /// 4주 시즌·50단계 무료/프리미엄 트랙 정의 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "SeasonPassTable", menuName = "Abyss Chronicle/Season Pass Table")]
    public sealed class SeasonPassTable : ScriptableObject
    {
        public const int TIER_COUNT = 50;
        public const int SEASON_DURATION_DAYS = 28;

        [SerializeField] private SeasonDefinition[] _seasons = Array.Empty<SeasonDefinition>();
        [SerializeField] private SeasonPassTierDefinition[] _tiers = Array.Empty<SeasonPassTierDefinition>();
        [SerializeField] private SeasonPassPointConfig _pointConfig = new();

        public IReadOnlyList<SeasonDefinition> Seasons => _seasons;
        public IReadOnlyList<SeasonPassTierDefinition> Tiers => _tiers;
        public SeasonPassPointConfig PointConfig => _pointConfig;

        /// <summary>
        /// 단계 인덱스(1~50)로 정의를 조회한다.
        /// </summary>
        public SeasonPassTierDefinition FindTier(int tierIndex)
        {
            if (tierIndex <= 0)
                return null;

            foreach (var tier in _tiers)
            {
                if (tier != null && tier.TierIndex == tierIndex)
                    return tier;
            }

            return null;
        }

        /// <summary>
        /// UTC 시각 기준 활성 시즌을 조회한다.
        /// </summary>
        public SeasonDefinition ResolveActiveSeason(DateTimeOffset utcNow)
        {
            var nowTicks = utcNow.UtcTicks;

            foreach (var season in _seasons)
            {
                if (season == null)
                    continue;

                if (nowTicks >= season.StartUtcTicks && nowTicks < season.EndUtcTicks)
                    return season;
            }

            return null;
        }

        /// <summary>
        /// 시즌 번호로 정의를 조회한다.
        /// </summary>
        public SeasonDefinition FindSeason(int seasonNumber)
        {
            foreach (var season in _seasons)
            {
                if (season != null && season.SeasonNumber == seasonNumber)
                    return season;
            }

            return null;
        }

        /// <summary>
        /// 포인트 획득 경로별 지급량을 반환한다.
        /// </summary>
        public int GetPointsForSource(SeasonPointSource source)
        {
            if (_pointConfig == null)
                return 0;

            return source switch
            {
                SeasonPointSource.DailyQuestComplete => _pointConfig.DailyQuestCompletePoints,
                SeasonPointSource.WeeklyQuestComplete => _pointConfig.WeeklyQuestCompletePoints,
                SeasonPointSource.FloorReached => _pointConfig.FloorReachedPoints,
                SeasonPointSource.BossKill => _pointConfig.BossKillPoints,
                _ => 0,
            };
        }

        /// <summary>
        /// spec 기본값으로 직렬화 필드를 채운다.
        /// </summary>
        public void ApplySpecDefaults()
        {
            var seasonOneStart = new DateTimeOffset(2026, 7, 31, 19, 0, 0, TimeSpan.Zero);
            var seasonOneEnd = seasonOneStart.AddDays(SEASON_DURATION_DAYS);
            var seasonTwoStart = seasonOneEnd;
            var seasonTwoEnd = seasonTwoStart.AddDays(SEASON_DURATION_DAYS);

            _seasons = new[]
            {
                CreateSeason(1, seasonOneStart, seasonOneEnd),
                CreateSeason(2, seasonTwoStart, seasonTwoEnd),
            };

            _pointConfig = new SeasonPassPointConfig
            {
                DailyQuestCompletePoints = 40,
                WeeklyQuestCompletePoints = 120,
                FloorReachedPoints = 15,
                BossKillPoints = 25,
                DailyEarnCap = 300,
            };

            _tiers = new SeasonPassTierDefinition[TIER_COUNT];

            for (var tierIndex = 1; tierIndex <= TIER_COUNT; tierIndex++)
            {
                _tiers[tierIndex - 1] = new SeasonPassTierDefinition
                {
                    TierIndex = tierIndex,
                    RequiredPoints = tierIndex * 100,
                    FreeRewards = new[]
                    {
                        Reward(CurrencyType.Gold, 500L + tierIndex * 50L),
                    },
                    PremiumRewards = tierIndex % 10 == 0
                        ? new[]
                        {
                            Reward(CurrencyType.AbyssStone, 10L),
                            Reward(CurrencyType.SummonTicket, 1L),
                        }
                        : new[]
                        {
                            Reward(CurrencyType.AbyssStone, 5L),
                        },
                };
            }
        }

        private static SeasonDefinition CreateSeason(
            int seasonNumber,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc)
        {
            return new SeasonDefinition
            {
                SeasonNumber = seasonNumber,
                StartUtcTicks = startUtc.UtcTicks,
                EndUtcTicks = endUtc.UtcTicks,
            };
        }

        private static SeasonPassRewardEntry Reward(CurrencyType type, long amount)
        {
            return new SeasonPassRewardEntry
            {
                CurrencyType = type,
                Amount = amount,
            };
        }
    }
}
