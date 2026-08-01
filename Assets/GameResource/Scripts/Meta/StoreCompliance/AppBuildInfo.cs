namespace Backend.Meta.StoreCompliance
{
    /// <summary>
    /// 빌드 시 주입된 환경·버전 메타데이터.
    /// </summary>
    public static class AppBuildInfo
    {
        /// <summary>
        /// 현재 빌드 환경.
        /// </summary>
        public static BuildEnvironmentKind Environment { get; private set; } = ResolveEnvironment();

        /// <summary>
        /// 시맨틱 버전 문자열 (PlayerSettings.bundleVersion).
        /// </summary>
        public static string Version => UnityEngine.Application.version;

        /// <summary>
        /// 아동 대상 앱이 아님 — 스토어 등록·COPPA/연령 등급 메타.
        /// </summary>
        public const bool TargetsChildren = false;

        /// <summary>
        /// 테스트용 환경을 덮어쓴다.
        /// </summary>
        public static void SetEnvironmentForTests(BuildEnvironmentKind environment)
        {
            Environment = environment;
        }

        /// <summary>
        /// 테스트용 환경 캐시를 초기화한다.
        /// </summary>
        public static void ResetForTests()
        {
            Environment = ResolveEnvironment();
        }

        private static BuildEnvironmentKind ResolveEnvironment()
        {
#if ABYSS_ENV_PRODUCTION
            return BuildEnvironmentKind.Production;
#elif ABYSS_ENV_STAGING
            return BuildEnvironmentKind.Staging;
#else
            return BuildEnvironmentKind.Development;
#endif
        }
    }
}
