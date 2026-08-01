using Backend.Meta;
using Backend.Meta.Currency;
using Backend.Simulation;

namespace Backend.GameSystems.Offline
{
    /// <summary>
    /// UI·씬에서 공유하는 오프라인 정산 런타임 접근점.
    /// </summary>
    public static class OfflineRuntimeProvider
    {
        private static OfflineSettlementService _service;

        /// <summary>
        /// 오프라인 정산 서비스를 반환한다.
        /// </summary>
        public static OfflineSettlementService Service => EnsureInitialized();

        /// <summary>
        /// 테스트용 서비스를 교체한다.
        /// </summary>
        public static void SetForTests(OfflineSettlementService service)
        {
            _service = service;
        }

        /// <summary>
        /// 런타임 캐시를 비운다.
        /// </summary>
        public static void Reset()
        {
            _service = null;
        }

        private static OfflineSettlementService EnsureInitialized()
        {
            if (_service != null)
                return _service;

            _service = new OfflineSettlementService(
                MetaRuntimeProvider.Wallet,
                BalanceTableProvider.Get());

            return _service;
        }
    }
}
