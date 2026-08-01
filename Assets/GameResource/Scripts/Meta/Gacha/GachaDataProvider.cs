using Backend.AddressableKey;
using Backend.Object.Management;
using Backend.Util.Management;
using UnityEngine;

namespace Backend.Meta.Gacha
{
    /// <summary>
    /// 오프라인 Addressables 에서 GachaRateTable·GachaBannerPool 을 로드한다.
    /// </summary>
    public static class GachaDataProvider
    {
        private static GachaRateTable _rateTable;
        private static GachaBannerPool _bannerPool;

        /// <summary>
        /// 확률 테이블을 반환한다. Addressables 로드 실패 시 spec 기본값을 생성한다.
        /// </summary>
        public static GachaRateTable GetRateTable()
        {
            if (_rateTable != null)
                return _rateTable;

            if (!GameStateUtil.IsQuitting)
            {
                var address = AddressableKeys.Gacha.Get("GachaRateTable");
                if (!string.IsNullOrEmpty(address))
                    _rateTable = ResourceManager.LoadResource<GachaRateTable>(address);
            }

            if (_rateTable != null)
                return _rateTable;

            _rateTable = ScriptableObject.CreateInstance<GachaRateTable>();
            _rateTable.ApplySpecDefaults();
            _rateTable.name = "GachaRateTable_RuntimeFallback";
            return _rateTable;
        }

        /// <summary>
        /// 배너 풀을 반환한다. Addressables 로드 실패 시 빈 인스턴스를 생성한다.
        /// </summary>
        public static GachaBannerPool GetBannerPool()
        {
            if (_bannerPool != null)
                return _bannerPool;

            if (!GameStateUtil.IsQuitting)
            {
                var address = AddressableKeys.Gacha.Get("GachaBannerPool");
                if (!string.IsNullOrEmpty(address))
                    _bannerPool = ResourceManager.LoadResource<GachaBannerPool>(address);
            }

            if (_bannerPool != null)
                return _bannerPool;

            _bannerPool = ScriptableObject.CreateInstance<GachaBannerPool>();
            _bannerPool.name = "GachaBannerPool_RuntimeFallback";
            return _bannerPool;
        }

        /// <summary>
        /// 테스트용 캐시를 교체한다.
        /// </summary>
        public static void SetForTests(GachaRateTable rateTable, GachaBannerPool bannerPool)
        {
            _rateTable = rateTable;
            _bannerPool = bannerPool;
        }

        /// <summary>
        /// 캐시를 비운다.
        /// </summary>
        public static void ResetCache()
        {
            _rateTable = null;
            _bannerPool = null;
        }
    }
}
