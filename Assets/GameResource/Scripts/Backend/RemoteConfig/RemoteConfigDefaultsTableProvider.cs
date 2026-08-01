using Backend.AddressableKey;
using Backend.Object.Management;
using UnityEngine;

namespace Backend.Services.RemoteConfig
{
    /// <summary>
    /// RemoteConfigDefaultsTable 런타임 접근 제공.
    /// </summary>
    public static class RemoteConfigDefaultsTableProvider
    {
        private static RemoteConfigDefaultsTable _cached;

        /// <summary>
        /// 로드된 기본값 테이블을 반환한다.
        /// </summary>
        public static RemoteConfigDefaultsTable Get()
        {
            if (_cached != null)
                return _cached;

            if (!GameStateUtil.IsQuitting)
            {
                var address = AddressableKeys.Backend.Get("RemoteConfigDefaultsTable");
                if (!string.IsNullOrEmpty(address))
                    _cached = ResourceManager.LoadResource<RemoteConfigDefaultsTable>(address);
            }

            if (_cached != null)
                return _cached;

            _cached = ScriptableObject.CreateInstance<RemoteConfigDefaultsTable>();
            _cached.ApplySpecDefaults();
            _cached.name = "RemoteConfigDefaultsTable_RuntimeFallback";
            return _cached;
        }

        /// <summary>
        /// 테스트용 캐시를 교체한다.
        /// </summary>
        public static void SetForTests(RemoteConfigDefaultsTable table)
        {
            _cached = table;
        }

        /// <summary>
        /// 캐시를 비운다.
        /// </summary>
        public static void ResetCache()
        {
            _cached = null;
        }
    }
}
