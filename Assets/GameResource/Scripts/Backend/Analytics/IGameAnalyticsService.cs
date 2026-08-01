using System.Collections.Generic;

namespace Backend.Services.Analytics
{
    /// <summary>
    /// Firebase Analytics 추상화.
    /// </summary>
    public interface IGameAnalyticsService
    {
        /// <summary>
        /// 분석 SDK 초기화 여부.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 분석 SDK를 초기화한다.
        /// </summary>
        void Initialize();

        /// <summary>
        /// 파라미터 없는 이벤트를 송신한다.
        /// </summary>
        void LogEvent(string eventName);

        /// <summary>
        /// 파라미터가 있는 이벤트를 송신한다.
        /// </summary>
        void LogEvent(string eventName, IReadOnlyDictionary<string, object> parameters);

        /// <summary>
        /// 사용자 속성을 설정한다.
        /// </summary>
        void SetUserProperty(string name, string value);
    }
}
