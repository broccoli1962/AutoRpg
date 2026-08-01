using System;
using UnityEngine;

namespace Backend.Meta.Ads
{
    /// <summary>
    /// 보상형 광고 위치별 정의.
    /// </summary>
    [Serializable]
    public sealed class AdPlacementDefinition
    {
        [SerializeField] private RewardedAdPlacement _placement;
        [SerializeField] private string _placementId;
        [SerializeField] private int _dailyLimit;

        public RewardedAdPlacement Placement => _placement;
        public string PlacementId => _placementId;
        public int DailyLimit => _dailyLimit;

        /// <summary>
        /// 직렬화 필드를 채운다.
        /// </summary>
        public void SetValues(RewardedAdPlacement placement, string placementId, int dailyLimit)
        {
            _placement = placement;
            _placementId = placementId;
            _dailyLimit = dailyLimit;
        }
    }
}
