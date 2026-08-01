using System;
using Backend.GameSystems.Offline;
using Backend.Meta.Retention;
using Backend.Meta.Shop;
using Cysharp.Threading.Tasks;

namespace Backend.Meta.Ads
{
    /// <summary>
    /// 보상형·전면 광고 일일 상한, SDK 연동, 보상 지급을 조율한다.
    /// </summary>
    public sealed class AdRewardService
    {
        private const string PLACEMENT_NOT_FOUND = "Ad placement not configured.";
        private const string DAILY_LIMIT_REACHED = "Daily ad limit reached.";
        private const string PLACEMENT_LIMIT_REACHED = "Placement daily limit reached.";
        private const string TUTORIAL_BLOCKED = "Ads are disabled during tutorial.";
        private const string INTERSTITIAL_DISABLED = "Interstitial ads are disabled.";

        private readonly IAdService _adService;
        private readonly AdConfigTable _config;
        private readonly AdRewardGrantor _grantor;
        private readonly ITutorialGate _tutorialGate;
        private readonly Func<bool> _shouldShowInterstitialAds;
        private readonly IServerTimeProvider _serverTimeProvider;
        private readonly Func<DateTimeOffset> _localUtcNow;
        private readonly AdBuffState _buffState = new();

        private int _dailyPeriodKey;
        private int _totalRewardedToday;
        private int _interstitialDailyCount;
        private int _interstitialSessionCount;
        private int _offlineDoubleCount;
        private int _instantProgressCount;
        private int _freeSummonCount;
        private int _instantRetryCount;
        private int _enhancementBoostCount;
        private int _eventReserveCount;

        public AdRewardService(
            IAdService adService,
            AdConfigTable config,
            AdRewardGrantor grantor,
            ITutorialGate tutorialGate = null,
            Func<bool> shouldShowInterstitialAds = null,
            IServerTimeProvider serverTimeProvider = null,
            Func<DateTimeOffset> localUtcNow = null)
        {
            _adService = adService ?? throw new ArgumentNullException(nameof(adService));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _grantor = grantor ?? throw new ArgumentNullException(nameof(grantor));
            _tutorialGate = tutorialGate ?? new DefaultTutorialGate();
            _shouldShowInterstitialAds = shouldShowInterstitialAds ?? (() => true);
            _serverTimeProvider = serverTimeProvider;
            _localUtcNow = localUtcNow ?? (() => DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// 광고 버프·토큰 상태.
        /// </summary>
        public AdBuffState BuffState => _buffState;

        /// <summary>
        /// 오늘 시청한 보상형 광고 총 횟수.
        /// </summary>
        public int TotalRewardedToday => _totalRewardedToday;

        /// <summary>
        /// 오늘 시청한 전면 광고 횟수.
        /// </summary>
        public int InterstitialDailyCount => _interstitialDailyCount;

        /// <summary>
        /// 현재 세션 전면 광고 횟수.
        /// </summary>
        public int InterstitialSessionCount => _interstitialSessionCount;

        /// <summary>
        /// SDK를 초기화한다.
        /// </summary>
        public UniTask<bool> InitializeAsync()
        {
            return _adService.InitializeAsync(_config);
        }

        /// <summary>
        /// 세션 카운터를 초기화한다. 앱 시작 시 1회 호출.
        /// </summary>
        public void ResetSessionCounters()
        {
            _interstitialSessionCount = 0;
        }

        /// <summary>
        /// 서버 시간 기준 일일 갱신 경계를 확인한다.
        /// </summary>
        public void RefreshDailyPeriod()
        {
            var now = ResolveNowUtc();
            RefreshDailyPeriodInternal(now);
        }

        /// <summary>
        /// 보상형 광고를 시청할 수 있는지 판정한다.
        /// </summary>
        public bool CanShowRewarded(RewardedAdPlacement placement)
        {
            RefreshDailyPeriod();

            if (_tutorialGate.IsTutorialActive)
                return false;

            if (_totalRewardedToday >= _config.TotalRewardedDailyLimit)
                return false;

            return GetPlacementCount(placement) < GetPlacementLimit(placement);
        }

        /// <summary>
        /// 위치별 남은 일일 횟수를 반환한다.
        /// </summary>
        public int GetRemainingDailyUses(RewardedAdPlacement placement)
        {
            RefreshDailyPeriod();
            var limit = GetPlacementLimit(placement);
            var used = GetPlacementCount(placement);
            return Math.Max(0, limit - used);
        }

        /// <summary>
        /// 보상형 일일 총 남은 횟수를 반환한다.
        /// </summary>
        public int GetTotalRewardedRemainingToday()
        {
            RefreshDailyPeriod();
            return Math.Max(0, _config.TotalRewardedDailyLimit - _totalRewardedToday);
        }

        /// <summary>
        /// 전면 광고를 표시할 수 있는지 판정한다.
        /// </summary>
        public bool CanShowInterstitial()
        {
            RefreshDailyPeriod();

            if (_tutorialGate.IsTutorialActive)
                return false;

            if (!_shouldShowInterstitialAds())
                return false;

            if (_interstitialDailyCount >= _config.InterstitialDailyLimit)
                return false;

            return _interstitialSessionCount < _config.InterstitialSessionLimit;
        }

        /// <summary>
        /// 보상형 광고를 시청하고 완료 시에만 보상을 지급한다.
        /// </summary>
        public async UniTask<AdRewardResult> TryShowRewardedAsync(RewardedAdPlacement placement)
        {
            RefreshDailyPeriod();

            if (_tutorialGate.IsTutorialActive)
            {
                return AdRewardResult.NotRewarded(
                    placement,
                    AdShowOutcome.Blocked,
                    TUTORIAL_BLOCKED);
            }

            var definition = _config.FindPlacement(placement);
            if (definition == null)
            {
                return AdRewardResult.NotRewarded(
                    placement,
                    AdShowOutcome.Blocked,
                    PLACEMENT_NOT_FOUND);
            }

            if (_totalRewardedToday >= _config.TotalRewardedDailyLimit)
            {
                return AdRewardResult.NotRewarded(
                    placement,
                    AdShowOutcome.Blocked,
                    DAILY_LIMIT_REACHED);
            }

            if (GetPlacementCount(placement) >= definition.DailyLimit)
            {
                return AdRewardResult.NotRewarded(
                    placement,
                    AdShowOutcome.Blocked,
                    PLACEMENT_LIMIT_REACHED);
            }

            var outcome = await _adService.ShowRewardedAsync(definition.PlacementId);
            if (outcome != AdShowOutcome.Completed)
            {
                return AdRewardResult.NotRewarded(placement, outcome);
            }

            IncrementPlacementCount(placement);
            _totalRewardedToday++;

            var grant = _grantor.Grant(placement, _config, _buffState);
            if (!grant.Success)
            {
                return AdRewardResult.NotRewarded(
                    placement,
                    AdShowOutcome.Failed,
                    grant.FailureReason);
            }

            return AdRewardResult.Succeeded(
                placement,
                grant.GrantedGold,
                grant.GrantedSummonTickets);
        }

        /// <summary>
        /// 자연 휴지점에서 전면 광고를 시도한다. 실패해도 게임 진행을 막지 않는다.
        /// </summary>
        public async UniTask<AdShowOutcome> TryShowInterstitialAsync(InterstitialTrigger trigger)
        {
            RefreshDailyPeriod();

            if (_tutorialGate.IsTutorialActive)
                return AdShowOutcome.Blocked;

            if (!_shouldShowInterstitialAds())
                return AdShowOutcome.Skipped;

            if (_interstitialDailyCount >= _config.InterstitialDailyLimit)
                return AdShowOutcome.Blocked;

            if (_interstitialSessionCount >= _config.InterstitialSessionLimit)
                return AdShowOutcome.Blocked;

            var outcome = await _adService.ShowInterstitialAsync();
            if (outcome == AdShowOutcome.Completed)
            {
                _interstitialDailyCount++;
                _interstitialSessionCount++;
            }

            return outcome;
        }

        /// <summary>
        /// 광고 제거 상품 보유 시 전면 광고가 비활성화되는지 확인한다.
        /// </summary>
        public bool IsInterstitialEnabledByPurchase()
        {
            return _shouldShowInterstitialAds();
        }

        /// <summary>
        /// 세이브 스냅샷을 생성한다.
        /// </summary>
        public AdSaveData ToSaveData()
        {
            var saveData = new AdSaveData
            {
                DailyPeriodKey = _dailyPeriodKey,
                TotalRewardedToday = _totalRewardedToday,
                InterstitialDailyCount = _interstitialDailyCount,
                InterstitialSessionCount = _interstitialSessionCount,
                OfflineDoubleCount = _offlineDoubleCount,
                InstantProgressCount = _instantProgressCount,
                FreeSummonCount = _freeSummonCount,
                InstantRetryCount = _instantRetryCount,
                EnhancementBoostCount = _enhancementBoostCount,
                EventReserveCount = _eventReserveCount,
            };

            _buffState.WriteToSaveData(saveData);
            return saveData;
        }

        /// <summary>
        /// 세이브 스냅샷을 복원한다.
        /// </summary>
        public void LoadSaveData(AdSaveData saveData)
        {
            if (saveData == null)
                return;

            _dailyPeriodKey = saveData.DailyPeriodKey;
            _totalRewardedToday = saveData.TotalRewardedToday;
            _interstitialDailyCount = saveData.InterstitialDailyCount;
            _interstitialSessionCount = saveData.InterstitialSessionCount;
            _offlineDoubleCount = saveData.OfflineDoubleCount;
            _instantProgressCount = saveData.InstantProgressCount;
            _freeSummonCount = saveData.FreeSummonCount;
            _instantRetryCount = saveData.InstantRetryCount;
            _enhancementBoostCount = saveData.EnhancementBoostCount;
            _eventReserveCount = saveData.EventReserveCount;
            _buffState.LoadFromSaveData(saveData);
        }

        private void RefreshDailyPeriodInternal(DateTimeOffset nowUtc)
        {
            var dayKey = DailyResetClock.GetDayKey(nowUtc);

            if (_dailyPeriodKey != 0 && dayKey != _dailyPeriodKey)
                ResetDailyCounters();

            _dailyPeriodKey = dayKey;
        }

        private void ResetDailyCounters()
        {
            _totalRewardedToday = 0;
            _interstitialDailyCount = 0;
            _offlineDoubleCount = 0;
            _instantProgressCount = 0;
            _freeSummonCount = 0;
            _instantRetryCount = 0;
            _enhancementBoostCount = 0;
            _eventReserveCount = 0;
        }

        private int GetPlacementCount(RewardedAdPlacement placement)
        {
            return placement switch
            {
                RewardedAdPlacement.OfflineDouble => _offlineDoubleCount,
                RewardedAdPlacement.InstantProgress => _instantProgressCount,
                RewardedAdPlacement.FreeSummonTicket => _freeSummonCount,
                RewardedAdPlacement.InstantRetry => _instantRetryCount,
                RewardedAdPlacement.EnhancementBoost => _enhancementBoostCount,
                RewardedAdPlacement.EventReserve => _eventReserveCount,
                _ => 0,
            };
        }

        private int GetPlacementLimit(RewardedAdPlacement placement)
        {
            var definition = _config.FindPlacement(placement);
            return definition?.DailyLimit ?? 0;
        }

        private void IncrementPlacementCount(RewardedAdPlacement placement)
        {
            switch (placement)
            {
                case RewardedAdPlacement.OfflineDouble:
                    _offlineDoubleCount++;
                    break;
                case RewardedAdPlacement.InstantProgress:
                    _instantProgressCount++;
                    break;
                case RewardedAdPlacement.FreeSummonTicket:
                    _freeSummonCount++;
                    break;
                case RewardedAdPlacement.InstantRetry:
                    _instantRetryCount++;
                    break;
                case RewardedAdPlacement.EnhancementBoost:
                    _enhancementBoostCount++;
                    break;
                case RewardedAdPlacement.EventReserve:
                    _eventReserveCount++;
                    break;
            }
        }

        private DateTimeOffset ResolveNowUtc()
        {
            if (_serverTimeProvider != null
                && _serverTimeProvider.TryGetServerTimeUtc(out var serverTime))
            {
                return serverTime;
            }

            return _localUtcNow();
        }
    }
}
