using System;
using Backend.GameSystems.Offline;
using Backend.Meta.Currency;
using Backend.Meta.Retention;

namespace Backend.Meta.Attendance
{
    /// <summary>
    /// 신규 7일·월간 28일 출석 보상을 관리한다.
    /// </summary>
    public sealed class AttendanceService
    {
        private const string ALREADY_CLAIMED_TODAY = "Attendance reward already claimed today.";
        private const string NEW_PLAYER_COMPLETE = "New player attendance track is complete.";
        private const string REWARD_NOT_FOUND = "Attendance reward not found.";
        private const string MONTHLY_ALREADY_CLAIMED = "Monthly calendar day already claimed.";

        private readonly Wallet _wallet;
        private readonly IServerTimeProvider _serverTimeProvider;
        private readonly Func<DateTimeOffset> _localUtcNow;

        private bool _newPlayerTrackCompleted;
        private int _newPlayerNextDay = 1;
        private int _lastNewPlayerClaimDayKey;
        private int _monthlyMonthKey;
        private int _monthlyClaimedDaysMask;
        private int _lastMonthlyClaimDayKey;

        public AttendanceService(
            Wallet wallet,
            IServerTimeProvider serverTimeProvider = null,
            Func<DateTimeOffset> localUtcNow = null)
        {
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _serverTimeProvider = serverTimeProvider;
            _localUtcNow = localUtcNow;
        }

        /// <summary>
        /// 신규 7일 트랙 완료 여부.
        /// </summary>
        public bool IsNewPlayerTrackCompleted => _newPlayerTrackCompleted;

        /// <summary>
        /// 신규 7일 다음 수령 일차(1~7).
        /// </summary>
        public int NewPlayerNextDay => _newPlayerNextDay;

        /// <summary>
        /// 월간 캘린더에서 특정 일차 수령 여부.
        /// </summary>
        public bool IsMonthlyDayClaimed(int dayIndex)
        {
            if (dayIndex < 1 || dayIndex > 28)
                return false;

            var bit = 1 << (dayIndex - 1);
            return (_monthlyClaimedDaysMask & bit) != 0;
        }

        /// <summary>
        /// 오늘 출석 보상을 수령한다. 신규 7일과 월간 캘린더를 동시에 처리한다.
        /// </summary>
        public AttendanceClaimResult TryCheckIn(AttendanceTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var now = ResolveNowUtc();
            RefreshMonthlyPeriod(now);
            var dayKey = DailyResetClock.GetDayKey(now);

            AttendanceClaimResult lastResult = null;

            if (!_newPlayerTrackCompleted)
            {
                var newPlayerResult = TryCheckInNewPlayer(table, now, dayKey);
                if (newPlayerResult.Success)
                    lastResult = newPlayerResult;
            }

            var monthlyResult = TryCheckInMonthly(table, now, dayKey);
            if (monthlyResult.Success)
                lastResult = monthlyResult;

            if (lastResult != null)
                return lastResult;

            if (!_newPlayerTrackCompleted && _lastNewPlayerClaimDayKey == dayKey)
                return AttendanceClaimResult.Failed(ALREADY_CLAIMED_TODAY);

            if (_lastMonthlyClaimDayKey == dayKey)
                return AttendanceClaimResult.Failed(ALREADY_CLAIMED_TODAY);

            return AttendanceClaimResult.Failed(ALREADY_CLAIMED_TODAY);
        }

        /// <summary>
        /// 세이브용 스냅샷을 생성한다.
        /// </summary>
        public AttendanceSaveData ToSaveData()
        {
            return new AttendanceSaveData
            {
                NewPlayerTrackCompleted = _newPlayerTrackCompleted,
                NewPlayerNextDay = _newPlayerNextDay,
                LastNewPlayerClaimDayKey = _lastNewPlayerClaimDayKey,
                MonthlyMonthKey = _monthlyMonthKey,
                MonthlyClaimedDaysMask = _monthlyClaimedDaysMask,
                LastMonthlyClaimDayKey = _lastMonthlyClaimDayKey,
            };
        }

        /// <summary>
        /// 세이브 스냅샷에서 AttendanceService 를 복원한다.
        /// </summary>
        public static AttendanceService FromSaveData(
            AttendanceSaveData saveData,
            Wallet wallet,
            IServerTimeProvider serverTimeProvider = null,
            Func<DateTimeOffset> localUtcNow = null)
        {
            var service = new AttendanceService(wallet, serverTimeProvider, localUtcNow);

            if (saveData == null)
                return service;

            service._newPlayerTrackCompleted = saveData.NewPlayerTrackCompleted;
            service._newPlayerNextDay = Math.Max(1, saveData.NewPlayerNextDay);
            service._lastNewPlayerClaimDayKey = saveData.LastNewPlayerClaimDayKey;
            service._monthlyMonthKey = saveData.MonthlyMonthKey;
            service._monthlyClaimedDaysMask = saveData.MonthlyClaimedDaysMask;
            service._lastMonthlyClaimDayKey = saveData.LastMonthlyClaimDayKey;

            service.RefreshMonthlyPeriod(service.ResolveNowUtc());
            return service;
        }

        private AttendanceClaimResult TryCheckInNewPlayer(
            AttendanceTable table,
            DateTimeOffset nowUtc,
            int dayKey)
        {
            if (_lastNewPlayerClaimDayKey == dayKey)
                return AttendanceClaimResult.Failed(ALREADY_CLAIMED_TODAY);

            if (_newPlayerNextDay > 7)
            {
                _newPlayerTrackCompleted = true;
                return AttendanceClaimResult.Failed(NEW_PLAYER_COMPLETE);
            }

            var previousDayKey = _lastNewPlayerClaimDayKey;
            if (previousDayKey != 0 && previousDayKey != DailyResetClock.GetPreviousDayKey(nowUtc))
                _newPlayerNextDay = 1;

            var reward = table.FindNewPlayerReward(_newPlayerNextDay);
            if (reward == null)
                return AttendanceClaimResult.Failed(REWARD_NOT_FOUND);

            if (!CreditReward(reward.Value))
                return AttendanceClaimResult.Failed("Failed to credit reward.");

            var claimedDay = _newPlayerNextDay;
            _lastNewPlayerClaimDayKey = dayKey;
            _newPlayerNextDay++;

            if (_newPlayerNextDay > 7)
                _newPlayerTrackCompleted = true;

            return AttendanceClaimResult.Succeeded(isNewPlayerTrack: true, dayIndex: claimedDay);
        }

        private AttendanceClaimResult TryCheckInMonthly(
            AttendanceTable table,
            DateTimeOffset nowUtc,
            int dayKey)
        {
            if (_lastMonthlyClaimDayKey == dayKey)
                return AttendanceClaimResult.Failed(ALREADY_CLAIMED_TODAY);

            var calendarDay = DailyResetClock.GetMonthlyCalendarDay(nowUtc);
            if (IsMonthlyDayClaimed(calendarDay))
                return AttendanceClaimResult.Failed(MONTHLY_ALREADY_CLAIMED);

            var reward = table.FindMonthlyReward(calendarDay);
            if (reward == null)
                return AttendanceClaimResult.Failed(REWARD_NOT_FOUND);

            if (!CreditReward(reward.Value))
                return AttendanceClaimResult.Failed("Failed to credit reward.");

            _monthlyClaimedDaysMask |= 1 << (calendarDay - 1);
            _lastMonthlyClaimDayKey = dayKey;

            return AttendanceClaimResult.Succeeded(isNewPlayerTrack: false, dayIndex: calendarDay);
        }

        private void RefreshMonthlyPeriod(DateTimeOffset nowUtc)
        {
            var monthKey = DailyResetClock.GetMonthKey(nowUtc);
            if (_monthlyMonthKey != 0 && monthKey != _monthlyMonthKey)
            {
                _monthlyClaimedDaysMask = 0;
                _lastMonthlyClaimDayKey = 0;
            }

            _monthlyMonthKey = monthKey;
        }

        private bool CreditReward(AttendanceRewardEntry reward)
        {
            if (reward.Amount <= 0L)
                return true;

            return _wallet.TryCredit(
                reward.CurrencyType,
                reward.Amount,
                CurrencyReasonCodes.AttendanceReward).Success;
        }

        private DateTimeOffset ResolveNowUtc()
        {
            if (_serverTimeProvider != null
                && _serverTimeProvider.TryGetServerTimeUtc(out var serverTimeUtc))
            {
                return serverTimeUtc;
            }

            return _localUtcNow != null ? _localUtcNow() : DateTimeOffset.UtcNow;
        }
    }
}
