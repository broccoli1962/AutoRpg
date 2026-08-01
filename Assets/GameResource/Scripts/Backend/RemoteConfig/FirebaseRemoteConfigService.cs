using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Backend.Services.RemoteConfig
{
    /// <summary>
    /// Firebase Remote Config 구현체. ABYSS_HAS_FIREBASE 정의 시 실 SDK를 사용한다.
    /// </summary>
    public sealed class FirebaseRemoteConfigService : IRemoteConfigService
    {
        private readonly RemoteConfigDefaultsTable _defaults;
        private Dictionary<string, string> _activeValues = new();

        public FirebaseRemoteConfigService(RemoteConfigDefaultsTable defaults = null)
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
        /// Remote Config 를 초기화하고 페치한다.
        /// </summary>
        public async UniTask<bool> InitializeAndFetchAsync()
        {
            ApplyDefaultsInternal();
#if ABYSS_HAS_FIREBASE
            try
            {
                var remoteConfig = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance;
                var defaults = _defaults.ToDictionary();
                var configDefaults = new Dictionary<string, object>();
                foreach (var pair in defaults)
                    configDefaults[pair.Key] = pair.Value;

                await remoteConfig.SetDefaultsAsync(configDefaults);
                await remoteConfig.FetchAsync(TimeSpan.Zero);
                await remoteConfig.ActivateAsync();

                _activeValues = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (var pair in remoteConfig.AllValues)
                    _activeValues[pair.Key] = pair.Value.StringValue;

                IsReady = true;
                LastFetchSucceeded = true;
                return true;
            }
            catch (Exception)
            {
                ApplyDefaultsInternal();
                IsReady = true;
                LastFetchSucceeded = false;
                return false;
            }
#else
            await UniTask.CompletedTask;
            IsReady = true;
            LastFetchSucceeded = false;
            return false;
#endif
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
            return bool.TryParse(raw, out var parsed) ? parsed : defaultValue;
        }

        /// <summary>
        /// 현재 활성 값 맵을 반환한다.
        /// </summary>
        public IReadOnlyDictionary<string, string> GetAllValues()
        {
            return _activeValues;
        }

        private void ApplyDefaultsInternal()
        {
            _activeValues = new Dictionary<string, string>(_defaults.ToDictionary());
            IsReady = true;
        }
    }
}
