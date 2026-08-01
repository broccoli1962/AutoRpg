using Backend.AddressableKey;
using Backend.Object.Management;
using UnityEngine;

namespace Backend.Meta.Achievements
{
    /// <summary>
    /// 런타임 AchievementTable 접근 제공.
    /// </summary>
    public static class AchievementTableProvider
    {
        private static AchievementTable _cached;

        /// <summary>
        /// 로드된 AchievementTable 을 반환한다. 없으면 spec 기본값 인스턴스를 생성한다.
        /// </summary>
        public static AchievementTable Get()
        {
            if (_cached != null)
                return _cached;

            if (!GameStateUtil.IsQuitting)
            {
                var address = AddressableKeys.Achievements.Get("AchievementTable");
                if (!string.IsNullOrEmpty(address))
                    _cached = ResourceManager.LoadResource<AchievementTable>(address);
            }

            if (_cached != null)
                return _cached;

            _cached = ScriptableObject.CreateInstance<AchievementTable>();
            _cached.ApplySpecDefaults();
            _cached.name = "AchievementTable_RuntimeFallback";
            return _cached;
        }

        /// <summary>
        /// 테스트·부트스트랩용 캐시를 교체한다.
        /// </summary>
        public static void SetForTests(AchievementTable table)
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
