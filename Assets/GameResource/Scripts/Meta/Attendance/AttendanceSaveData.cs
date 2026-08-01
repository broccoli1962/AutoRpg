using System;

namespace Backend.Meta.Attendance
{
    /// <summary>
    /// 출석 상태 세이브 스냅샷.
    /// </summary>
    [Serializable]
    public sealed class AttendanceSaveData
    {
        public bool NewPlayerTrackCompleted;
        public int NewPlayerNextDay = 1;
        public int LastNewPlayerClaimDayKey;
        public int MonthlyMonthKey;
        public int MonthlyClaimedDaysMask;
        public int LastMonthlyClaimDayKey;
    }
}
