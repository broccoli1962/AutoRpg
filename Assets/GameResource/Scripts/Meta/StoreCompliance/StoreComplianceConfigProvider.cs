using Backend.AddressableKey;
using Backend.Object.Management;
using UnityEngine;

namespace Backend.Meta.StoreCompliance
{
    /// <summary>
    /// 런타임 StoreComplianceConfig 접근 제공.
    /// </summary>
    public static class StoreComplianceConfigProvider
    {
        private static StoreComplianceConfig _cached;

        /// <summary>
        /// 로드된 StoreComplianceConfig 를 반환한다.
        /// </summary>
        public static StoreComplianceConfig Get()
        {
            if (_cached != null)
                return _cached;

            if (!GameStateUtil.IsQuitting)
            {
                var address = AddressableKeys.StoreCompliance.Get("StoreComplianceConfig");
                if (!string.IsNullOrEmpty(address))
                    _cached = ResourceManager.LoadResource<StoreComplianceConfig>(address);
            }

            if (_cached != null)
                return _cached;

            _cached = ScriptableObject.CreateInstance<StoreComplianceConfig>();
            _cached.ApplySpecDefaults();
            _cached.name = "StoreComplianceConfig_RuntimeFallback";
            return _cached;
        }

        /// <summary>
        /// 테스트용 캐시를 교체한다.
        /// </summary>
        public static void SetForTests(StoreComplianceConfig config)
        {
            _cached = config;
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
