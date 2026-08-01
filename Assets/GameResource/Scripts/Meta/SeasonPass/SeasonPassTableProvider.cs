using Backend.AddressableKey;
using Backend.Object.Management;
using UnityEngine;

namespace Backend.Meta.SeasonPass
{
    /// <summary>
    /// 런타임 SeasonPassTable 접근 제공.
    /// </summary>
    public static class SeasonPassTableProvider
    {
        private static SeasonPassTable _cached;

        /// <summary>
        /// 로드된 SeasonPassTable 을 반환한다. 없으면 spec 기본값 인스턴스를 생성한다.
        /// </summary>
        public static SeasonPassTable Get()
        {
            if (_cached != null)
                return _cached;

            if (!GameStateUtil.IsQuitting)
            {
                var address = AddressableKeys.SeasonPass.Get("SeasonPassTable");
                if (!string.IsNullOrEmpty(address))
                    _cached = ResourceManager.LoadResource<SeasonPassTable>(address);
            }

            if (_cached != null)
                return _cached;

            _cached = ScriptableObject.CreateInstance<SeasonPassTable>();
            _cached.ApplySpecDefaults();
            _cached.name = "SeasonPassTable_RuntimeFallback";
            return _cached;
        }

        /// <summary>
        /// 테스트·부트스트랩용 캐시를 교체한다.
        /// </summary>
        public static void SetForTests(SeasonPassTable table)
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
