using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Backend.Services.RemoteConfig
{
    /// <summary>
    /// EditMode·오프라인용 Remote Config 스텁. 번들 기본값만 사용한다.
    /// </summary>
    public sealed class SimulatedRemoteConfigService : IRemoteConfigService
    {
        private readonly RemoteConfigDefaultsTable _defaults;
        private Dictionary<string, string> _activeValues = new();

        public SimulatedRemoteConfigService(RemoteConfigDefaultsTable defaults = null)
        {
            _defaults = defaults ?? RemoteConfigDefaultsTableProvider.Get();
        }

        /// <summary>
        /// 초기화·페치 완료 여부.
        /// </summary>
        public bool IsReady { get; private set; }

        /// <summary>
        /// 마지막 페치 성공 여부.
        /// </summary>
        public bool LastFetchSucceeded { get; private set; }

        /// <summary>
        /// Remote Config 를 초기화한다.
        /// </summary>
        public UniTask<bool> InitializeAndFetchAsync()
        {
            _activeValues = new Dictionary<string, string>(_defaults.ToDictionary());
            IsReady = true;
            LastFetchSucceeded = true;
            return UniTask.FromResult(true);
        }

        /// <summary>
        /// 페치 실패를 시뮬레이션한다.
        /// </summary>
        public UniTask<bool> SimulateFetchFailureAsync()
        {
            _activeValues = new Dictionary<string, string>(_defaults.ToDictionary());
            IsReady = true;
            LastFetchSucceeded = false;
            return UniTask.FromResult(false);
        }

        /// <summary>
        /// 테스트용 원격 값을 주입한다.
        /// </summary>
        public void SetRemoteValueForTests(string key, string value)
        {
            _activeValues[key] = value;
            IsReady = true;
            LastFetchSucceeded = true;
        }

        /// <summary>
        /// 문자열 값을 조회한다.
        /// </summary>
        public string GetString(string key, string defaultValue = null)
        {
            if (_activeValues.TryGetValue(key, out var value))
                return value;

            var defaults = _defaults.ToDictionary();
            return defaults.TryGetValue(key, out var fallback) ? fallback : defaultValue ?? string.Empty;
        }

        /// <summary>
        /// 실수 값을 조회한다.
        /// </summary>
        public double GetDouble(string key, double defaultValue)
        {
            var raw = GetString(key, null);
            return double.TryParse(raw, out var parsed) ? parsed : defaultValue;
        }

        /// <summary>
        /// 불리언 값을 조회한다.
        /// </summary>
        public bool GetBool(string key, bool defaultValue)
        {
            var raw = GetString(key, null);
            if (bool.TryParse(raw, out var parsed))
                return parsed;

            return defaultValue;
        }

        /// <summary>
        /// 현재 활성 값 맵을 반환한다.
        /// </summary>
        public IReadOnlyDictionary<string, string> GetAllValues()
        {
            return _activeValues;
        }
    }
}
