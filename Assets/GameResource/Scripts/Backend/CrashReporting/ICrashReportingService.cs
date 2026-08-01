namespace Backend.Services.CrashReporting
{
    /// <summary>
    /// Crashlytics 추상화.
    /// </summary>
    public interface ICrashReportingService
    {
        /// <summary>
        /// Crashlytics 를 초기화한다.
        /// </summary>
        void Initialize();

        /// <summary>
        /// 사용자 ID를 설정한다.
        /// </summary>
        void SetUserId(string userId);

        /// <summary>
        /// 커스텀 키를 설정한다.
        /// </summary>
        void SetCustomKey(string key, string value);

        /// <summary>
        /// 비치명적 예외를 기록한다.
        /// </summary>
        void LogException(System.Exception exception);
    }
}
