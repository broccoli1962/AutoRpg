using System;

namespace Backend.Meta.Attendance
{
    /// <summary>
    /// 출석 체크인 결과.
    /// </summary>
    public sealed class AttendanceClaimResult
    {
        private AttendanceClaimResult(
            bool success,
            bool isNewPlayerTrack,
            int dayIndex,
            string failureReason)
        {
            Success = success;
            IsNewPlayerTrack = isNewPlayerTrack;
            DayIndex = dayIndex;
            FailureReason = failureReason;
        }

        public bool Success { get; }
        public bool IsNewPlayerTrack { get; }
        public int DayIndex { get; }
        public string FailureReason { get; }

        /// <summary>
        /// 성공 결과를 생성한다.
        /// </summary>
        public static AttendanceClaimResult Succeeded(bool isNewPlayerTrack, int dayIndex)
        {
            return new AttendanceClaimResult(true, isNewPlayerTrack, dayIndex, null);
        }

        /// <summary>
        /// 실패 결과를 생성한다.
        /// </summary>
        public static AttendanceClaimResult Failed(string reason)
        {
            return new AttendanceClaimResult(false, false, 0, reason);
        }
    }
}
