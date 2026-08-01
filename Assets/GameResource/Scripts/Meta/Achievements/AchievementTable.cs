using System;
using System.Collections.Generic;
using UnityEngine;

namespace Backend.Meta.Achievements
{
    /// <summary>
    /// 7종 카테고리 다단계 업적 정의 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "AchievementTable", menuName = "Abyss Chronicle/Achievement Table")]
    public sealed class AchievementTable : ScriptableObject
    {
        [SerializeField] private AchievementCategoryDefinition[] _categories = Array.Empty<AchievementCategoryDefinition>();

        public IReadOnlyList<AchievementCategoryDefinition> Categories => _categories;

        /// <summary>
        /// 카테고리로 정의를 조회한다.
        /// </summary>
        public AchievementCategoryDefinition FindCategory(AchievementCategory category)
        {
            foreach (var definition in _categories)
            {
                if (definition != null && definition.Category == category)
                    return definition;
            }

            return null;
        }

        /// <summary>
        /// 단계 ID로 정의를 조회한다.
        /// </summary>
        public AchievementTierDefinition FindTier(string tierId)
        {
            if (string.IsNullOrEmpty(tierId))
                return null;

            foreach (var category in _categories)
            {
                if (category?.Tiers == null)
                    continue;

                foreach (var tier in category.Tiers)
                {
                    if (tier != null && tier.TierId == tierId)
                        return tier;
                }
            }

            return null;
        }

        /// <summary>
        /// 단계 ID가 속한 카테고리 정의를 조회한다.
        /// </summary>
        public AchievementCategoryDefinition FindCategoryForTier(string tierId)
        {
            if (string.IsNullOrEmpty(tierId))
                return null;

            foreach (var category in _categories)
            {
                if (category?.Tiers == null)
                    continue;

                foreach (var tier in category.Tiers)
                {
                    if (tier != null && tier.TierId == tierId)
                        return category;
                }
            }

            return null;
        }

        /// <summary>
        /// spec 기본값으로 직렬화 필드를 채운다.
        /// </summary>
        public void ApplySpecDefaults()
        {
            _categories = new[]
            {
                CreateCategory(
                    AchievementCategory.TotalKills,
                    "total_kills",
                    AchievementProgressMode.Additive,
                    Tier("total_kills", 0, 100L, 10L),
                    Tier("total_kills", 1, 1_000L, 30L),
                    Tier("total_kills", 2, 10_000L, 100L)),
                CreateCategory(
                    AchievementCategory.HighestFloor,
                    "highest_floor",
                    AchievementProgressMode.Maximum,
                    Tier("highest_floor", 0, 25L, 10L),
                    Tier("highest_floor", 1, 50L, 30L),
                    Tier("highest_floor", 2, 100L, 100L)),
                CreateCategory(
                    AchievementCategory.EquipmentUpgrades,
                    "equipment_upgrades",
                    AchievementProgressMode.Additive,
                    Tier("equipment_upgrades", 0, 10L, 10L),
                    Tier("equipment_upgrades", 1, 100L, 30L),
                    Tier("equipment_upgrades", 2, 500L, 100L)),
                CreateCategory(
                    AchievementCategory.SummonCount,
                    "summon_count",
                    AchievementProgressMode.Additive,
                    Tier("summon_count", 0, 10L, 10L),
                    Tier("summon_count", 1, 50L, 30L),
                    Tier("summon_count", 2, 200L, 100L)),
                CreateCategory(
                    AchievementCategory.CollectionCompletion,
                    "collection_completion",
                    AchievementProgressMode.Percentage,
                    Tier("collection_completion", 0, 25L, 10L),
                    Tier("collection_completion", 1, 50L, 30L),
                    Tier("collection_completion", 2, 100L, 100L)),
                CreateCategory(
                    AchievementCategory.PrestigeCount,
                    "prestige_count",
                    AchievementProgressMode.Additive,
                    Tier("prestige_count", 0, 1L, 10L),
                    Tier("prestige_count", 1, 5L, 30L),
                    Tier("prestige_count", 2, 20L, 100L)),
                CreateCategory(
                    AchievementCategory.CompendiumEntries,
                    "compendium_entries",
                    AchievementProgressMode.Additive,
                    Tier("compendium_entries", 0, 5L, 10L),
                    Tier("compendium_entries", 1, 15L, 30L),
                    Tier("compendium_entries", 2, 30L, 100L)),
            };
        }

        private static AchievementCategoryDefinition CreateCategory(
            AchievementCategory category,
            string categoryId,
            AchievementProgressMode progressMode,
            params AchievementTierDefinition[] tiers)
        {
            return new AchievementCategoryDefinition
            {
                Category = category,
                CategoryId = categoryId,
                ProgressMode = progressMode,
                Tiers = tiers,
            };
        }

        private static AchievementTierDefinition Tier(
            string categoryId,
            int tierIndex,
            long targetValue,
            long baseReward)
        {
            return new AchievementTierDefinition
            {
                TierId = $"{categoryId}_tier_{tierIndex}",
                TierIndex = tierIndex,
                TargetValue = targetValue,
                BaseAbyssStoneReward = baseReward,
                RemoteConfigKey = AchievementRemoteConfigKeys.TierReward(categoryId, tierIndex),
            };
        }
    }
}
