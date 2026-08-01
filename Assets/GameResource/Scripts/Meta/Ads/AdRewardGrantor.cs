using System;
using Backend.GameSystems.Offline;
using Backend.Meta.Currency;
using Backend.Simulation;

namespace Backend.Meta.Ads
{
    /// <summary>
    /// 보상형 광고 완료 후 placement 별 보상을 Wallet 에 반영한다.
    /// </summary>
    public sealed class AdRewardGrantor
    {
        private readonly Wallet _wallet;
        private readonly BalanceTable _balanceTable;
        private readonly Func<int> _currentFloorProvider;

        public AdRewardGrantor(
            Wallet wallet,
            BalanceTable balanceTable,
            Func<int> currentFloorProvider = null)
        {
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _balanceTable = balanceTable ?? throw new ArgumentNullException(nameof(balanceTable));
            _currentFloorProvider = currentFloorProvider ?? (() => 1);
        }

        /// <summary>
        /// placement 에 맞는 보상을 지급한다.
        /// </summary>
        public AdRewardGrantResult Grant(RewardedAdPlacement placement, AdConfigTable config, AdBuffState buffState)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            buffState ??= new AdBuffState();

            switch (placement)
            {
                case RewardedAdPlacement.OfflineDouble:
                    return GrantOfflineDouble(buffState);
                case RewardedAdPlacement.InstantProgress:
                    return GrantInstantProgress(config, buffState);
                case RewardedAdPlacement.FreeSummonTicket:
                    return GrantFreeSummonTicket(buffState);
                case RewardedAdPlacement.InstantRetry:
                    return GrantInstantRetry(buffState);
                case RewardedAdPlacement.EnhancementBoost:
                    return GrantEnhancementBoost(config, buffState);
                case RewardedAdPlacement.EventReserve:
                    return AdRewardGrantResult.Empty(placement);
                default:
                    return AdRewardGrantResult.Empty(placement);
            }
        }

        private AdRewardGrantResult GrantOfflineDouble(AdBuffState buffState)
        {
            var pending = OfflineReturnSummaryContext.Pending;
            if (pending == null || pending.GoldReward <= 0L)
                return AdRewardGrantResult.Empty(RewardedAdPlacement.OfflineDouble);

            var originalGold = pending.GoldReward;
            var targetGold = (long)Math.Floor(originalGold / OfflinePolicy.DefaultEfficiency * 2d);
            var bonusGold = targetGold - originalGold;

            if (bonusGold <= 0L)
                return AdRewardGrantResult.Empty(RewardedAdPlacement.OfflineDouble);

            var credit = _wallet.TryCredit(
                CurrencyType.Gold,
                bonusGold,
                CurrencyReasonCodes.AdRewardOfflineDouble);

            if (!credit.Success)
                return AdRewardGrantResult.Failed(RewardedAdPlacement.OfflineDouble, credit.FailureReason);

            buffState.MarkOfflineDoubleApplied();
            return AdRewardGrantResult.WithGold(RewardedAdPlacement.OfflineDouble, bonusGold);
        }

        private AdRewardGrantResult GrantInstantProgress(AdConfigTable config, AdBuffState buffState)
        {
            var floor = Math.Max(1, _currentFloorProvider());
            var duration = TimeSpan.FromMinutes(config.InstantProgressMinutes);
            var gold = OfflineRewardCalculator.CalculateGold(
                _balanceTable,
                floor,
                duration,
                efficiency: 1d);

            if (gold <= 0L)
                return AdRewardGrantResult.Empty(RewardedAdPlacement.InstantProgress);

            var credit = _wallet.TryCredit(
                CurrencyType.Gold,
                gold,
                CurrencyReasonCodes.AdRewardInstantProgress);

            if (!credit.Success)
                return AdRewardGrantResult.Failed(RewardedAdPlacement.InstantProgress, credit.FailureReason);

            buffState.MarkInstantProgressApplied();
            return AdRewardGrantResult.WithGold(RewardedAdPlacement.InstantProgress, gold);
        }

        private AdRewardGrantResult GrantFreeSummonTicket(AdBuffState buffState)
        {
            var credit = _wallet.TryCredit(
                CurrencyType.SummonTicket,
                1L,
                CurrencyReasonCodes.AdRewardFreeSummon);

            if (!credit.Success)
                return AdRewardGrantResult.Failed(RewardedAdPlacement.FreeSummonTicket, credit.FailureReason);

            buffState.MarkFreeSummonApplied();
            return AdRewardGrantResult.WithSummonTickets(RewardedAdPlacement.FreeSummonTicket, 1L);
        }

        private AdRewardGrantResult GrantInstantRetry(AdBuffState buffState)
        {
            buffState.GrantInstantRetryToken();
            return AdRewardGrantResult.Empty(RewardedAdPlacement.InstantRetry);
        }

        private AdRewardGrantResult GrantEnhancementBoost(AdConfigTable config, AdBuffState buffState)
        {
            buffState.GrantEnhancementDiscount(
                config.EnhancementDiscountPercent,
                config.EnhancementDiscountUses);
            return AdRewardGrantResult.Empty(RewardedAdPlacement.EnhancementBoost);
        }
    }

    /// <summary>
    /// 광고 버프·토큰 런타임 상태.
    /// </summary>
    public sealed class AdBuffState
    {
        public bool HasInstantRetryToken { get; private set; }
        public int EnhancementDiscountPercent { get; private set; }
        public int EnhancementDiscountRemainingUses { get; private set; }

        /// <summary>
        /// 즉시 재도전 토큰을 소비한다.
        /// </summary>
        public bool TryConsumeInstantRetryToken()
        {
            if (!HasInstantRetryToken)
                return false;

            HasInstantRetryToken = false;
            return true;
        }

        /// <summary>
        /// 강화 할인을 소비한다. 할인율(0~100)을 반환하고 없으면 0.
        /// </summary>
        public int TryConsumeEnhancementDiscount()
        {
            if (EnhancementDiscountRemainingUses <= 0)
                return 0;

            EnhancementDiscountRemainingUses--;
            return EnhancementDiscountPercent;
        }

        internal void GrantInstantRetryToken()
        {
            HasInstantRetryToken = true;
        }

        internal void GrantEnhancementDiscount(int percent, int uses)
        {
            EnhancementDiscountPercent = Math.Clamp(percent, 0, 100);
            EnhancementDiscountRemainingUses = Math.Max(0, uses);
        }

        internal void MarkOfflineDoubleApplied()
        {
        }

        internal void MarkInstantProgressApplied()
        {
        }

        internal void MarkFreeSummonApplied()
        {
        }

        /// <summary>
        /// 세이브 스냅샷을 적용한다.
        /// </summary>
        public void LoadFromSaveData(AdSaveData saveData)
        {
            if (saveData == null)
                return;

            HasInstantRetryToken = saveData.HasInstantRetryToken;
            EnhancementDiscountPercent = saveData.EnhancementDiscountPercent;
            EnhancementDiscountRemainingUses = saveData.EnhancementDiscountRemainingUses;
        }

        /// <summary>
        /// 세이브 스냅샷을 생성한다.
        /// </summary>
        public void WriteToSaveData(AdSaveData saveData)
        {
            if (saveData == null)
                return;

            saveData.HasInstantRetryToken = HasInstantRetryToken;
            saveData.EnhancementDiscountPercent = EnhancementDiscountPercent;
            saveData.EnhancementDiscountRemainingUses = EnhancementDiscountRemainingUses;
        }
    }

    /// <summary>
    /// AdRewardGrantor 내부 지급 결과.
    /// </summary>
    public readonly struct AdRewardGrantResult
    {
        public bool Success { get; }
        public RewardedAdPlacement Placement { get; }
        public string FailureReason { get; }
        public long GrantedGold { get; }
        public long GrantedSummonTickets { get; }

        private AdRewardGrantResult(
            bool success,
            RewardedAdPlacement placement,
            string failureReason,
            long grantedGold,
            long grantedSummonTickets)
        {
            Success = success;
            Placement = placement;
            FailureReason = failureReason;
            GrantedGold = grantedGold;
            GrantedSummonTickets = grantedSummonTickets;
        }

        /// <summary>
        /// 지급 없음(버프·토큰만) 결과.
        /// </summary>
        public static AdRewardGrantResult Empty(RewardedAdPlacement placement)
        {
            return new AdRewardGrantResult(true, placement, null, 0L, 0L);
        }

        /// <summary>
        /// 골드 지급 결과.
        /// </summary>
        public static AdRewardGrantResult WithGold(RewardedAdPlacement placement, long gold)
        {
            return new AdRewardGrantResult(true, placement, null, gold, 0L);
        }

        /// <summary>
        /// 소환권 지급 결과.
        /// </summary>
        public static AdRewardGrantResult WithSummonTickets(RewardedAdPlacement placement, long tickets)
        {
            return new AdRewardGrantResult(true, placement, null, 0L, tickets);
        }

        /// <summary>
        /// 지급 실패 결과.
        /// </summary>
        public static AdRewardGrantResult Failed(RewardedAdPlacement placement, string reason)
        {
            return new AdRewardGrantResult(false, placement, reason, 0L, 0L);
        }
    }
}
