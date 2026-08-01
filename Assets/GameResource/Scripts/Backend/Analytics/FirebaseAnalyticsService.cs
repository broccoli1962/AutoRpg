using System.Collections.Generic;
using UnityEngine;

namespace Backend.Services.Analytics
{
    /// <summary>
    /// Firebase Analytics 구현체. ABYSS_HAS_FIREBASE 정의 시 실 SDK를 사용한다.
    /// </summary>
    public sealed class FirebaseAnalyticsService : IGameAnalyticsService
    {
        /// <summary>
        /// 분석 SDK 초기화 여부.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// 분석 SDK를 초기화한다.
        /// </summary>
        public void Initialize()
        {
#if ABYSS_HAS_FIREBASE
            IsInitialized = true;
#else
            IsInitialized = false;
#endif
        }

        /// <summary>
        /// 파라미터 없는 이벤트를 송신한다.
        /// </summary>
        public void LogEvent(string eventName)
        {
#if ABYSS_HAS_FIREBASE
            if (!IsInitialized || string.IsNullOrEmpty(eventName))
                return;

            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName);
#endif
        }

        /// <summary>
        /// 파라미터가 있는 이벤트를 송신한다.
        /// </summary>
        public void LogEvent(string eventName, IReadOnlyDictionary<string, object> parameters)
        {
#if ABYSS_HAS_FIREBASE
            if (!IsInitialized || string.IsNullOrEmpty(eventName))
                return;

            if (parameters == null || parameters.Count == 0)
            {
                LogEvent(eventName);
                return;
            }

            var firebaseParams = new Firebase.Analytics.Parameter[parameters.Count];
            var index = 0;
            foreach (var pair in parameters)
            {
                firebaseParams[index++] = ConvertParameter(pair.Key, pair.Value);
            }

            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, firebaseParams);
#endif
        }

        /// <summary>
        /// 사용자 속성을 설정한다.
        /// </summary>
        public void SetUserProperty(string name, string value)
        {
#if ABYSS_HAS_FIREBASE
            if (!IsInitialized || string.IsNullOrEmpty(name))
                return;

            Firebase.Analytics.FirebaseAnalytics.SetUserProperty(name, value ?? string.Empty);
#endif
        }

#if ABYSS_HAS_FIREBASE
        private static Firebase.Analytics.Parameter ConvertParameter(string key, object value)
        {
            switch (value)
            {
                case int intValue:
                    return new Firebase.Analytics.Parameter(key, intValue);
                case long longValue:
                    return new Firebase.Analytics.Parameter(key, longValue);
                case double doubleValue:
                    return new Firebase.Analytics.Parameter(key, doubleValue);
                case float floatValue:
                    return new Firebase.Analytics.Parameter(key, floatValue);
                default:
                    return new Firebase.Analytics.Parameter(key, value?.ToString() ?? string.Empty);
            }
        }
#endif
    }
}
