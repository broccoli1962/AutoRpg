namespace Backend.Services.CrashReporting
{
    /// <summary>
    /// Firebase Crashlytics 구현체. ABYSS_HAS_FIREBASE 정의 시 실 SDK를 사용한다.
    /// </summary>
    public sealed class FirebaseCrashlyticsService : ICrashReportingService
    {
        /// <summary>
        /// Crashlytics 를 초기화한다.
        /// </summary>
        public void Initialize()
        {
#if ABYSS_HAS_FIREBASE
            Firebase.Crashlytics.Crashlytics.IsCrashlyticsCollectionEnabled = true;
#endif
        }

        /// <summary>
        /// 사용자 ID를 설정한다.
        /// </summary>
        public void SetUserId(string userId)
        {
#if ABYSS_HAS_FIREBASE
            Firebase.Crashlytics.Crashlytics.SetUserId(userId ?? string.Empty);
#endif
        }

        /// <summary>
        /// 커스텀 키를 설정한다.
        /// </summary>
        public void SetCustomKey(string key, string value)
        {
#if ABYSS_HAS_FIREBASE
            Firebase.Crashlytics.Crashlytics.SetCustomKey(key, value ?? string.Empty);
#endif
        }

        /// <summary>
        /// 비치명적 예외를 기록한다.
        /// </summary>
        public void LogException(System.Exception exception)
        {
#if ABYSS_HAS_FIREBASE
            if (exception != null)
                Firebase.Crashlytics.Crashlytics.LogException(exception);
#endif
        }
    }
}
