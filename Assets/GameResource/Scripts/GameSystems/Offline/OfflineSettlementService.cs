using System;
using Backend.Chronicle;
using Backend.Meta.Currency;
using Backend.Simulation;

namespace Backend.GameSystems.Offline
{
    /// <summary>
    /// 오프라인 정산·지급·상태 갱신을 수행하는 순수 서비스.
    /// </summary>
    public sealed class OfflineSettlementService
    {
        private readonly Wallet _wallet;
        private readonly BalanceTable _balanceTable;
        private readonly IServerTimeProvider _serverTimeProvider;
        private readonly Func<DateTimeOffset> _localUtcNow;
        private readonly Func<NarrationRequest, string> _narrationBuilder;

        private DateTimeOffset _lastSettlementUtc;
        private int _currentFloor = 1;
        private int _innFacilityLevel;
        private bool _hasActiveMonthlyContract;
        private bool _hasInitializedSettlement;

        /// <summary>
        /// 오프라인 정산 서비스를 생성한다.
        /// </summary>
        public OfflineSettlementService(
            Wallet wallet,
            BalanceTable balanceTable,
            IServerTimeProvider serverTimeProvider = null,
            Func<DateTimeOffset> localUtcNow = null,
            Func<NarrationRequest, string> narrationBuilder = null)
        {
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _balanceTable = balanceTable ?? throw new ArgumentNullException(nameof(balanceTable));
            _serverTimeProvider = serverTimeProvider;
            _localUtcNow = localUtcNow ?? (() => DateTimeOffset.UtcNow);
            _narrationBuilder = narrationBuilder;
        }

        /// <summary>
        /// 현재 탐험 층(1-based).
        /// </summary>
        public int CurrentFloor => _currentFloor;

        /// <summary>
        /// 여관 시설 레벨.
        /// </summary>
        public int InnFacilityLevel
        {
            get => _innFacilityLevel;
            set => _innFacilityLevel = Math.Max(0, value);
        }

        /// <summary>
        /// 월간 심연 계약 활성 여부.
        /// </summary>
        public bool HasActiveMonthlyContract
        {
            get => _hasActiveMonthlyContract;
            set => _hasActiveMonthlyContract = value;
        }

        /// <summary>
        /// 마지막 정산 UTC 시각.
        /// </summary>
        public DateTimeOffset LastSettlementUtc => _lastSettlementUtc;

        /// <summary>
        /// 최초 정산 시각을 현재 시각으로 초기화한다.
        /// </summary>
        public void InitializeIfNeeded()
        {
            if (_hasInitializedSettlement)
                return;

            _lastSettlementUtc = OfflineTimeResolver.ResolveCurrentTimeUtc(
                _serverTimeProvider,
                _localUtcNow);
            _hasInitializedSettlement = true;
        }

        /// <summary>
        /// 복귀 시 오프라인 보상을 정산하고 Wallet 에 반영한다.
        /// </summary>
        public OfflineSettlementResult SettleOnReturn()
        {
            InitializeIfNeeded();

            var elapsedSnapshot = OfflineTimeResolver.ResolveElapsed(
                _lastSettlementUtc,
                _serverTimeProvider,
                _localUtcNow);

            var cap = OfflineCapCalculator.GetCap(_innFacilityLevel, _hasActiveMonthlyContract);
            var timeManipulationBlocked = elapsedSnapshot.UsedLocalFallback
                && elapsedSnapshot.Elapsed <= TimeSpan.Zero
                && elapsedSnapshot.CurrentTimeUtc < _lastSettlementUtc;

            if (elapsedSnapshot.Elapsed <= TimeSpan.Zero)
            {
                _lastSettlementUtc = elapsedSnapshot.CurrentTimeUtc;
                return new OfflineSettlementResult
                {
                    LastSettlementUtc = _lastSettlementUtc,
                    CurrentTimeUtc = elapsedSnapshot.CurrentTimeUtc,
                    RawElapsed = TimeSpan.Zero,
                    SettledDuration = TimeSpan.Zero,
                    Cap = cap,
                    UsedLocalFallback = elapsedSnapshot.UsedLocalFallback,
                    TimeManipulationBlocked = timeManipulationBlocked,
                };
            }

            var rewards = OfflineRewardCalculator.BuildRewards(
                _balanceTable,
                _currentFloor,
                elapsedSnapshot.Elapsed,
                cap);

            var shouldShowSummary = rewards.SettledDuration.TotalSeconds
                >= OfflinePolicy.MinSummaryElapsedSeconds
                && rewards.HasRewards;

            if (shouldShowSummary)
                rewards.ApplyTo(_wallet);

            var highlights = shouldShowSummary
                ? OfflineHighlightGenerator.Generate(
                    _currentFloor,
                    rewards.SettledDuration,
                    elapsedSnapshot.CurrentTimeUtc.UtcTicks.GetHashCode(),
                    _narrationBuilder)
                : Array.Empty<string>();

            _lastSettlementUtc = elapsedSnapshot.CurrentTimeUtc;

            return new OfflineSettlementResult
            {
                LastSettlementUtc = _lastSettlementUtc,
                CurrentTimeUtc = elapsedSnapshot.CurrentTimeUtc,
                RawElapsed = elapsedSnapshot.Elapsed,
                SettledDuration = rewards.SettledDuration,
                Cap = cap,
                UsedLocalFallback = elapsedSnapshot.UsedLocalFallback,
                TimeManipulationBlocked = timeManipulationBlocked,
                GoldReward = rewards.Gold,
                Highlights = highlights,
                ShouldShowSummary = shouldShowSummary,
                AppliedToWallet = shouldShowSummary,
            };
        }

        /// <summary>
        /// 세이브 스냅샷을 생성한다.
        /// </summary>
        public OfflineProgressSaveData ToSaveData()
        {
            return new OfflineProgressSaveData
            {
                LastSettlementUtcTicks = _lastSettlementUtc.UtcTicks,
                CurrentFloor = _currentFloor,
                InnFacilityLevel = _innFacilityLevel,
                HasActiveMonthlyContract = _hasActiveMonthlyContract,
            };
        }

        /// <summary>
        /// 세이브 스냅샷에서 상태를 복원한다.
        /// </summary>
        public void LoadFromSaveData(OfflineProgressSaveData saveData)
        {
            if (saveData == null)
                return;

            if (saveData.LastSettlementUtcTicks > 0L)
            {
                _lastSettlementUtc = new DateTimeOffset(saveData.LastSettlementUtcTicks, TimeSpan.Zero);
                _hasInitializedSettlement = true;
            }

            if (saveData.CurrentFloor >= 1)
                _currentFloor = saveData.CurrentFloor;

            _innFacilityLevel = Math.Max(0, saveData.InnFacilityLevel);
            _hasActiveMonthlyContract = saveData.HasActiveMonthlyContract;
        }

        /// <summary>
        /// 테스트·시뮬레이션용 탐험 층을 설정한다.
        /// </summary>
        public void SetCurrentFloorForTests(int floor)
        {
            _currentFloor = Math.Max(1, floor);
        }

        /// <summary>
        /// 테스트용 마지막 정산 시각을 설정한다.
        /// </summary>
        public void SetLastSettlementUtcForTests(DateTimeOffset lastSettlementUtc)
        {
            _lastSettlementUtc = lastSettlementUtc;
            _hasInitializedSettlement = true;
        }
    }
}
