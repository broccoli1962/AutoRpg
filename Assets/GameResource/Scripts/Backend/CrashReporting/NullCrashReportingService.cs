namespace Backend.Services.CrashReporting
{
    /// <summary>
    /// Crashlytics no-op 구현.
    /// </summary>
    public sealed class NullCrashReportingService : ICrashReportingService
    {
        /// <summary>
        /// Crashlytics 를 초기화한다.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// 사용자 ID를 설정한다.
        /// </summary>
        public void SetUserId(string userId)
        {
        }

        /// <summary>
        /// 커스텀 키를 설정한다.
        /// </summary>
        public void SetCustomKey(string key, string value)
        {
        }

        /// <summary>
        /// 비치명적 예외를 기록한다.
        /// </summary>
        public void LogException(System.Exception exception)
        {
        }
    }
}
