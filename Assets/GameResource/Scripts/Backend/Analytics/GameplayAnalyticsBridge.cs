using System;
using System.Collections.Generic;
using Backend.Meta.Achievements;

namespace Backend.Services.Analytics
{
    /// <summary>
    /// 게임플레이·메타 이벤트를 분석 SDK로 전달하는 정적 채널.
    /// </summary>
    public static class BackendAnalyticsEvents
    {
        public static event Action<int> TutorialStepReported;
        public static event Action<int> FloorReachedReported;
        public static event Action PrestigeReported;
        public static event Action<int> SummonReported;
        public static event Action ShopViewReported;
        public static event Action<string> ShopPurchaseReported;
        public static event Action<string> AdWatchReported;
        public static event Action<int> GrowthWallReported;

        /// <summary>
        /// 튜토리얼 단계를 발행한다.
        /// </summary>
        public static void ReportTutorialStep(int step)
        {
            if (step >= 0)
                TutorialStepReported?.Invoke(step);
        }

        /// <summary>
        /// 층 도달을 발행한다.
        /// </summary>
        public static void ReportFloorReached(int floor)
        {
            if (floor > 0)
                FloorReachedReported?.Invoke(floor);
        }

        /// <summary>
        /// 프레스티지를 발행한다.
        /// </summary>
        public static void ReportPrestige()
        {
            PrestigeReported?.Invoke();
        }

        /// <summary>
        /// 소환을 발행한다.
        /// </summary>
        public static void ReportSummon(int count = 1)
        {
            if (count > 0)
                SummonReported?.Invoke(count);
        }

        /// <summary>
        /// 상점 노출을 발행한다.
        /// </summary>
        public static void ReportShopView()
        {
            ShopViewReported?.Invoke();
        }

        /// <summary>
        /// 상점 구매를 발행한다.
        /// </summary>
        public static void ReportShopPurchase(string productId)
        {
            if (!string.IsNullOrEmpty(productId))
                ShopPurchaseReported?.Invoke(productId);
        }

        /// <summary>
        /// 광고 시청을 발행한다.
        /// </summary>
        public static void ReportAdWatch(string placementId)
        {
            if (!string.IsNullOrEmpty(placementId))
                AdWatchReported?.Invoke(placementId);
        }

        /// <summary>
        /// 성장 벽 도달을 발행한다.
        /// </summary>
        public static void ReportGrowthWall(int floor)
        {
            if (floor > 0)
                GrowthWallReported?.Invoke(floor);
        }

        /// <summary>
        /// 테스트용 구독자를 모두 해제한다.
        /// </summary>
        public static void ClearSubscribers()
        {
            TutorialStepReported = null;
            FloorReachedReported = null;
            PrestigeReported = null;
            SummonReported = null;
            ShopViewReported = null;
            ShopPurchaseReported = null;
            AdWatchReported = null;
            GrowthWallReported = null;
        }
    }

    /// <summary>
    /// MetaGameplayEvents·BackendAnalyticsEvents 를 IGameAnalyticsService 로 브릿지한다.
    /// </summary>
    public sealed class GameplayAnalyticsBridge : IDisposable
    {
        private readonly IGameAnalyticsService _analytics;

        public GameplayAnalyticsBridge(IGameAnalyticsService analytics)
        {
            _analytics = analytics ?? throw new ArgumentNullException(nameof(analytics));
        }

        /// <summary>
        /// 이벤트 구독을 시작한다.
        /// </summary>
        public void Subscribe()
        {
            MetaGameplayEvents.FloorReached += OnFloorReached;
            MetaGameplayEvents.PrestigePerformed += OnPrestige;
            MetaGameplayEvents.SummonPerformed += OnSummon;

            BackendAnalyticsEvents.TutorialStepReported += OnTutorialStep;
            BackendAnalyticsEvents.FloorReachedReported += OnFloorReached;
            BackendAnalyticsEvents.PrestigeReported += OnPrestige;
            BackendAnalyticsEvents.SummonReported += OnSummon;
            BackendAnalyticsEvents.ShopViewReported += OnShopView;
            BackendAnalyticsEvents.ShopPurchaseReported += OnShopPurchase;
            BackendAnalyticsEvents.AdWatchReported += OnAdWatch;
            BackendAnalyticsEvents.GrowthWallReported += OnGrowthWall;
        }

        /// <summary>
        /// 이벤트 구독을 해제한다.
        /// </summary>
        public void Dispose()
        {
            MetaGameplayEvents.FloorReached -= OnFloorReached;
            MetaGameplayEvents.PrestigePerformed -= OnPrestige;
            MetaGameplayEvents.SummonPerformed -= OnSummon;

            BackendAnalyticsEvents.TutorialStepReported -= OnTutorialStep;
            BackendAnalyticsEvents.FloorReachedReported -= OnFloorReached;
            BackendAnalyticsEvents.PrestigeReported -= OnPrestige;
            BackendAnalyticsEvents.SummonReported -= OnSummon;
            BackendAnalyticsEvents.ShopViewReported -= OnShopView;
            BackendAnalyticsEvents.ShopPurchaseReported -= OnShopPurchase;
            BackendAnalyticsEvents.AdWatchReported -= OnAdWatch;
            BackendAnalyticsEvents.GrowthWallReported -= OnGrowthWall;
        }

        private void OnTutorialStep(int step)
        {
            _analytics.LogEvent(GameAnalyticsEvents.TutorialStep, Param("step", step));
        }

        private void OnFloorReached(int floor)
        {
            _analytics.LogEvent(GameAnalyticsEvents.FloorReached, Param("floor", floor));
        }

        private void OnPrestige()
        {
            _analytics.LogEvent(GameAnalyticsEvents.Prestige);
        }

        private void OnSummon(int count)
        {
            _analytics.LogEvent(GameAnalyticsEvents.Summon, Param("count", count));
        }

        private void OnShopView()
        {
            _analytics.LogEvent(GameAnalyticsEvents.ShopView);
        }

        private void OnShopPurchase(string productId)
        {
            _analytics.LogEvent(GameAnalyticsEvents.ShopPurchase, Param("product_id", productId));
        }

        private void OnAdWatch(string placementId)
        {
            _analytics.LogEvent(GameAnalyticsEvents.AdWatch, Param("placement_id", placementId));
        }

        private void OnGrowthWall(int floor)
        {
            _analytics.LogEvent(GameAnalyticsEvents.GrowthWall, Param("floor", floor));
        }

        private static Dictionary<string, object> Param(string key, object value)
        {
            return new Dictionary<string, object> { { key, value } };
        }
    }
}
