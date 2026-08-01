using System.Collections.Generic;

namespace Backend.Services.Analytics
{
    /// <summary>
    /// EditMode·오프라인용 분석 스텁. 이벤트를 메모리에 기록한다.
    /// </summary>
    public sealed class SimulatedAnalyticsService : IGameAnalyticsService
    {
        private readonly List<AnalyticsEventRecord> _records = new();
        private readonly Dictionary<string, string> _userProperties = new();

        /// <summary>
        /// 분석 SDK 초기화 여부.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// 기록된 이벤트 목록.
        /// </summary>
        public IReadOnlyList<AnalyticsEventRecord> Records => _records;

        /// <summary>
        /// 분석 SDK를 초기화한다.
        /// </summary>
        public void Initialize()
        {
            IsInitialized = true;
        }

        /// <summary>
        /// 파라미터 없는 이벤트를 송신한다.
        /// </summary>
        public void LogEvent(string eventName)
        {
            if (!IsInitialized || string.IsNullOrEmpty(eventName))
                return;

            _records.Add(new AnalyticsEventRecord(eventName, null));
        }

        /// <summary>
        /// 파라미터가 있는 이벤트를 송신한다.
        /// </summary>
        public void LogEvent(string eventName, IReadOnlyDictionary<string, object> parameters)
        {
            if (!IsInitialized || string.IsNullOrEmpty(eventName))
                return;

            _records.Add(new AnalyticsEventRecord(eventName, parameters));
        }

        /// <summary>
        /// 사용자 속성을 설정한다.
        /// </summary>
        public void SetUserProperty(string name, string value)
        {
            if (string.IsNullOrEmpty(name))
                return;

            _userProperties[name] = value ?? string.Empty;
        }

        /// <summary>
        /// 테스트용 기록을 비운다.
        /// </summary>
        public void ClearForTests()
        {
            _records.Clear();
            _userProperties.Clear();
        }
    }

    /// <summary>
    /// 기록된 분석 이벤트 1건.
    /// </summary>
    public sealed class AnalyticsEventRecord
    {
        public string EventName { get; }
        public IReadOnlyDictionary<string, object> Parameters { get; }

        public AnalyticsEventRecord(string eventName, IReadOnlyDictionary<string, object> parameters)
        {
            EventName = eventName;
            Parameters = parameters;
        }
    }
}
