using System;
using System.Collections.Generic;
using UnityEngine;

namespace Backend.Meta.Ads
{
    /// <summary>
    /// 광고 위치·일일 상한·AdMob 유닛 ID ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "AdConfigTable", menuName = "Abyss Chronicle/Ad Config Table")]
    public sealed class AdConfigTable : ScriptableObject
    {
        [SerializeField] private int _totalRewardedDailyLimit = 15;
        [SerializeField] private int _interstitialDailyLimit = 6;
        [SerializeField] private int _interstitialSessionLimit = 2;
        [SerializeField] private int _instantProgressMinutes = 30;
        [SerializeField] private int _enhancementDiscountPercent = 30;
        [SerializeField] private int _enhancementDiscountUses = 3;
        [SerializeField] private string _rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";
        [SerializeField] private string _interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";
        [SerializeField] private AdPlacementDefinition[] _placements = Array.Empty<AdPlacementDefinition>();

        /// <summary>
        /// 보상형 광고 일일 총 상한.
        /// </summary>
        public int TotalRewardedDailyLimit => _totalRewardedDailyLimit;

        /// <summary>
        /// 전면 광고 일일 상한.
        /// </summary>
        public int InterstitialDailyLimit => _interstitialDailyLimit;

        /// <summary>
        /// 전면 광고 세션 상한.
        /// </summary>
        public int InterstitialSessionLimit => _interstitialSessionLimit;

        /// <summary>
        /// 즉시 진행 보상 분 수.
        /// </summary>
        public int InstantProgressMinutes => _instantProgressMinutes;

        /// <summary>
        /// 강화 부스트 할인율(0~100).
        /// </summary>
        public int EnhancementDiscountPercent => _enhancementDiscountPercent;

        /// <summary>
        /// 강화 부스트 적용 횟수.
        /// </summary>
        public int EnhancementDiscountUses => _enhancementDiscountUses;

        /// <summary>
        /// AdMob 보상형 유닛 ID.
        /// </summary>
        public string RewardedAdUnitId => _rewardedAdUnitId;

        /// <summary>
        /// AdMob 전면 유닛 ID.
        /// </summary>
        public string InterstitialAdUnitId => _interstitialAdUnitId;

        /// <summary>
        /// 등록된 보상형 위치 목록.
        /// </summary>
        public IReadOnlyList<AdPlacementDefinition> Placements => _placements;

        /// <summary>
        /// 위치 정의를 조회한다.
        /// </summary>
        public AdPlacementDefinition FindPlacement(RewardedAdPlacement placement)
        {
            foreach (var definition in _placements)
            {
                if (definition != null && definition.Placement == placement)
                    return definition;
            }

            return null;
        }

        /// <summary>
        /// spec 기본값으로 직렬화 필드를 채운다.
        /// </summary>
        public void ApplySpecDefaults()
        {
            _totalRewardedDailyLimit = 15;
            _interstitialDailyLimit = 6;
            _interstitialSessionLimit = 2;
            _instantProgressMinutes = 30;
            _enhancementDiscountPercent = 30;
            _enhancementDiscountUses = 3;
            _rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";
            _interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";

            _placements = new[]
            {
                CreatePlacement(RewardedAdPlacement.OfflineDouble, "offline_double", 3),
                CreatePlacement(RewardedAdPlacement.InstantProgress, "instant_progress", 5),
                CreatePlacement(RewardedAdPlacement.FreeSummonTicket, "free_summon", 1),
                CreatePlacement(RewardedAdPlacement.InstantRetry, "instant_retry", 3),
                CreatePlacement(RewardedAdPlacement.EnhancementBoost, "enhancement_boost", 2),
                CreatePlacement(RewardedAdPlacement.EventReserve, "event_reserve", 1),
            };
        }

        private static AdPlacementDefinition CreatePlacement(
            RewardedAdPlacement placement,
            string placementId,
            int dailyLimit)
        {
            var definition = new AdPlacementDefinition();
            definition.SetValues(placement, placementId, dailyLimit);
            return definition;
        }
    }
}
