using System;
using Backend.Meta.Currency;

namespace Backend.Meta.Attendance
{
    /// <summary>
    /// 출석 보상 항목.
    /// </summary>
    [Serializable]
    public struct AttendanceRewardEntry
    {
        public int DayIndex;
        public CurrencyType CurrencyType;
        public long Amount;
    }
}
