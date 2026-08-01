using Backend.Meta.Characters;
using Backend.Meta.Currency;
using Backend.Meta.Gacha;
using Backend.Simulation;

namespace Backend.Meta
{
    /// <summary>
    /// UI·게임플레이에서 공유하는 메타 시스템 런타임 접근점.
    /// </summary>
    public static class MetaRuntimeProvider
    {
        private static Wallet _wallet;
        private static ExplorerCatalog _catalog;
        private static GachaService _gacha;

        /// <summary>
        /// 재화 지갑을 반환한다.
        /// </summary>
        public static Wallet Wallet => EnsureInitialized().Wallet;

        /// <summary>
        /// 탐험가 도감을 반환한다.
        /// </summary>
        public static ExplorerCatalog Catalog => EnsureInitialized().Catalog;

        /// <summary>
        /// 소환 서비스를 반환한다.
        /// </summary>
        public static GachaService Gacha => EnsureInitialized().Gacha;

        /// <summary>
        /// 테스트용 런타임을 교체한다.
        /// </summary>
        public static void SetForTests(Wallet wallet, ExplorerCatalog catalog, GachaService gacha)
        {
            _wallet = wallet;
            _catalog = catalog;
            _gacha = gacha;
        }

        /// <summary>
        /// 런타임 캐시를 비운다.
        /// </summary>
        public static void Reset()
        {
            _wallet = null;
            _catalog = null;
            _gacha = null;
        }

        private static (Wallet Wallet, ExplorerCatalog Catalog, GachaService Gacha) EnsureInitialized()
        {
            if (_gacha != null)
                return (_wallet, _catalog, _gacha);

            var ledger = new TransactionLedger();
            _wallet = new Wallet(ledger);
            _catalog = new ExplorerCatalog();
            var gachaLedger = new GachaSummonLedger();
            var pity = new GachaPityState();
            var balance = BalanceTableProvider.Get();

            _gacha = new GachaService(
                _wallet,
                _catalog,
                gachaLedger,
                pity,
                balance);

            return (_wallet, _catalog, _gacha);
        }
    }
}
