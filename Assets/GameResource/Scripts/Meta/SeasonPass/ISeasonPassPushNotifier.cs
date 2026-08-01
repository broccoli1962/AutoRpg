using System;

namespace Backend.Meta.SeasonPass
{
    /// <summary>
    /// 시즌 종료 임박 푸시 알림 스케줄 훅.
    /// </summary>
    public interface ISeasonPassPushNotifier
    {
        /// <summary>
        /// 시즌 종료 임박 알림을 예약한다.
        /// </summary>
        void ScheduleSeasonEndReminder(int seasonNumber, DateTimeOffset seasonEndUtc);

        /// <summary>
        /// 시즌 종료 알림 예약을 취소한다.
        /// </summary>
        void CancelSeasonEndReminder(int seasonNumber);
    }

    /// <summary>
    /// 개발용 no-op 푸시 알림 구현.
    /// </summary>
    public sealed class NullSeasonPassPushNotifier : ISeasonPassPushNotifier
    {
        /// <summary>
        /// 예약 요청을 무시한다.
        /// </summary>
        public void ScheduleSeasonEndReminder(int seasonNumber, DateTimeOffset seasonEndUtc)
        {
        }

        /// <summary>
        /// 취소 요청을 무시한다.
        /// </summary>
        public void CancelSeasonEndReminder(int seasonNumber)
        {
        }
    }
}
