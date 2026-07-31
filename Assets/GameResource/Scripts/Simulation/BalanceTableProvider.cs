using Backend.AddressableKey;
using Backend.Object.Management;
using UnityEngine;

namespace Backend.Simulation
{
    /// <summary>
    /// 런타임 BalanceTable 접근 제공.
    /// </summary>
    public static class BalanceTableProvider
    {
        private static BalanceTable _cached;

        /// <summary>
        /// 로드된 BalanceTable을 반환한다. 없으면 spec 기본값 인스턴스를 생성한다.
        /// </summary>
        public static BalanceTable Get()
        {
            if (_cached != null)
                return _cached;

            if (!GameStateUtil.IsQuitting)
            {
                var address = AddressableKeys.Balance.Get("BalanceTable");
                if (!string.IsNullOrEmpty(address))
                    _cached = ResourceManager.LoadResource<BalanceTable>(address);
            }

            if (_cached != null)
                return _cached;

            _cached = ScriptableObject.CreateInstance<BalanceTable>();
            _cached.ApplySpecDefaults();
            _cached.name = "BalanceTable_RuntimeFallback";
            return _cached;
        }

        /// <summary>
        /// 테스트·부트스트랩용 캐시를 교체한다.
        /// </summary>
        public static void SetForTests(BalanceTable table)
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
