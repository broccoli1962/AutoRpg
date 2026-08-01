using System;
using System.Collections.Generic;
using Backend.Meta.Currency;

namespace Backend.Meta.Achievements
{
    /// <summary>
    /// 누적 업적 진행도 추적, 단계 달성 판정, 보상 수령을 담당한다.
    /// 프레스티지로 진행도가 초기화되지 않는다.
    /// </summary>
    public sealed class AchievementService
    {
        private const string TIER_NOT_FOUND = "Achievement tier not found.";
        private const string TIER_NOT_COMPLETE = "Achievement tier is not complete.";
        private const string TIER_ALREADY_CLAIMED = "Achievement tier reward already claimed.";

        private readonly Wallet _wallet;
        private readonly Dictionary<AchievementCategory, long> _progress = new();
        private readonly HashSet<string> _claimedTierIds = new();

        public AchievementService(Wallet wallet)
        {
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
        }

        /// <summary>
        /// 카테고리 진행도를 반환한다.
        /// </summary>
        public long GetProgress(AchievementCategory category)
        {
            return _progress.TryGetValue(category, out var value) ? value : 0L;
        }

        /// <summary>
        /// 단계 보상 수령 여부를 반환한다.
        /// </summary>
        public bool IsTierClaimed(string tierId)
        {
            return !string.IsNullOrEmpty(tierId) && _claimedTierIds.Contains(tierId);
        }

        /// <summary>
        /// 단계 달성 여부를 반환한다.
        /// </summary>
        public bool IsTierComplete(string tierId, AchievementTable table)
        {
            var tier = table?.FindTier(tierId);
            var category = table?.FindCategoryForTier(tierId);
            if (tier == null || category == null)
                return false;

            return GetProgress(category.Category) >= tier.TargetValue;
        }

        /// <summary>
        /// 누적형 카테고리 진행도를 보고한다.
        /// </summary>
        public void ReportProgress(AchievementCategory category, long amount, AchievementTable table)
        {
            if (amount <= 0L || table == null)
                return;

            var definition = table.FindCategory(category);
            if (definition == null || definition.ProgressMode != AchievementProgressMode.Additive)
                return;

            var current = GetProgress(category);
            _progress[category] = current + amount;
        }

        /// <summary>
        /// 최고 도달 층을 보고한다. 기존 값보다 클 때만 갱신한다.
        /// </summary>
        public void ReportHighestFloor(int floor, AchievementTable table)
        {
            if (floor <= 0 || table == null)
                return;

            var definition = table.FindCategory(AchievementCategory.HighestFloor);
            if (definition == null || definition.ProgressMode != AchievementProgressMode.Maximum)
                return;

            var current = GetProgress(AchievementCategory.HighestFloor);
            if (floor > current)
                _progress[AchievementCategory.HighestFloor] = floor;
        }

        /// <summary>
        /// 수집 완성도(0~100%)를 보고한다.
        /// </summary>
        public void ReportCollectionCompletion(int ownedUniqueCount, int totalAvailableCount, AchievementTable table)
        {
            if (table == null || totalAvailableCount <= 0 || ownedUniqueCount < 0)
                return;

            var definition = table.FindCategory(AchievementCategory.CollectionCompletion);
            if (definition == null || definition.ProgressMode != AchievementProgressMode.Percentage)
                return;

            var percent = ownedUniqueCount * 100L / totalAvailableCount;
            if (percent > 100L)
                percent = 100L;

            _progress[AchievementCategory.CollectionCompletion] = percent;
        }

        /// <summary>
        /// 도감 등재 수를 현재 값으로 동기화한다.
        /// </summary>
        public void SyncCompendiumCount(int compendiumCount, AchievementTable table)
        {
            if (compendiumCount < 0 || table == null)
                return;

            var definition = table.FindCategory(AchievementCategory.CompendiumEntries);
            if (definition == null)
                return;

            var current = GetProgress(AchievementCategory.CompendiumEntries);
            if (compendiumCount > current)
                _progress[AchievementCategory.CompendiumEntries] = compendiumCount;
        }

        /// <summary>
        /// 단계 보상을 수령한다.
        /// </summary>
        public AchievementClaimResult TryClaimTier(
            string tierId,
            AchievementTable table,
            IAchievementRewardResolver rewardResolver)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            if (rewardResolver == null)
                throw new ArgumentNullException(nameof(rewardResolver));

            var tier = table.FindTier(tierId);
            var categoryDefinition = table.FindCategoryForTier(tierId);
            if (tier == null || categoryDefinition == null)
                return AchievementClaimResult.Failed(tierId, AchievementCategory.TotalKills, TIER_NOT_FOUND);

            if (_claimedTierIds.Contains(tierId))
            {
                return AchievementClaimResult.Failed(
                    tierId,
                    categoryDefinition.Category,
                    TIER_ALREADY_CLAIMED);
            }

            if (GetProgress(categoryDefinition.Category) < tier.TargetValue)
            {
                return AchievementClaimResult.Failed(
                    tierId,
                    categoryDefinition.Category,
                    TIER_NOT_COMPLETE);
            }

            var rewardAmount = rewardResolver.ResolveReward(tier);
            if (rewardAmount > 0L)
            {
                var creditResult = _wallet.TryCredit(
                    CurrencyType.AbyssStone,
                    rewardAmount,
                    CurrencyReasonCodes.AchievementReward);

                if (!creditResult.Success)
                {
                    return AchievementClaimResult.Failed(
                        tierId,
                        categoryDefinition.Category,
                        creditResult.FailureReason ?? "Failed to credit reward.");
                }
            }

            _claimedTierIds.Add(tierId);
            return AchievementClaimResult.Succeeded(tierId, categoryDefinition.Category, rewardAmount);
        }

        /// <summary>
        /// 세이브용 스냅샷을 생성한다.
        /// </summary>
        public AchievementSaveData ToSaveData()
        {
            var entries = new AchievementProgressEntry[_progress.Count];
            var index = 0;

            foreach (var pair in _progress)
            {
                entries[index++] = new AchievementProgressEntry
                {
                    Category = pair.Key,
                    CurrentValue = pair.Value,
                };
            }

            var claimed = new string[_claimedTierIds.Count];
            _claimedTierIds.CopyTo(claimed);

            return new AchievementSaveData
            {
                ProgressEntries = entries,
                ClaimedTierIds = claimed,
            };
        }

        /// <summary>
        /// 세이브 스냅샷에서 AchievementService 를 복원한다.
        /// </summary>
        public static AchievementService FromSaveData(AchievementSaveData saveData, Wallet wallet)
        {
            var service = new AchievementService(wallet);

            if (saveData == null)
                return service;

            if (saveData.ProgressEntries != null)
            {
                foreach (var entry in saveData.ProgressEntries)
                {
                    if (entry == null)
                        continue;

                    service._progress[entry.Category] = Math.Max(0L, entry.CurrentValue);
                }
            }

            if (saveData.ClaimedTierIds != null)
            {
                foreach (var tierId in saveData.ClaimedTierIds)
                {
                    if (!string.IsNullOrEmpty(tierId))
                        service._claimedTierIds.Add(tierId);
                }
            }

            return service;
        }
    }
}
