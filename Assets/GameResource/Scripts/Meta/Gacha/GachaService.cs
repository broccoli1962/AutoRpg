using System;
using Backend.Chronicle;
using Backend.Meta.Characters;
using Backend.Meta.Currency;
using Backend.Simulation;

namespace Backend.Meta.Gacha
{
    /// <summary>
    /// 소환(가챠) 실행·천장·원장·재화 차감을 담당한다.
    /// </summary>
    public sealed class GachaService
    {
        private const int TEN_PULL_COUNT = 10;

        private readonly Wallet _wallet;
        private readonly ExplorerCatalog _catalog;
        private readonly GachaSummonLedger _ledger;
        private readonly GachaPityState _pity;
        private readonly BalanceTable _balanceTable;
        private readonly Func<long> _seedProvider;
        private readonly Func<long, IRandomSource> _randomFactory;
        private long _nextSeed;

        public GachaService(
            Wallet wallet,
            ExplorerCatalog catalog,
            GachaSummonLedger ledger,
            GachaPityState pity,
            BalanceTable balanceTable,
            long nextSeed = 1L,
            Func<long> seedProvider = null,
            Func<long, IRandomSource> randomFactory = null)
        {
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _ledger = ledger ?? throw new ArgumentNullException(nameof(ledger));
            _pity = pity ?? throw new ArgumentNullException(nameof(pity));
            _balanceTable = balanceTable ?? throw new ArgumentNullException(nameof(balanceTable));
            _nextSeed = nextSeed > 0L ? nextSeed : 1L;
            _seedProvider = seedProvider;
            _randomFactory = randomFactory ?? (seed => new SeededRandomSource((int)(seed & int.MaxValue)));
        }

        /// <summary>
        /// 연결된 소환 원장을 반환한다.
        /// </summary>
        public GachaSummonLedger Ledger => _ledger;

        /// <summary>
        /// 천장 상태를 반환한다.
        /// </summary>
        public GachaPityState Pity => _pity;

        /// <summary>
        /// 단차(1회) 소환을 실행한다.
        /// </summary>
        public GachaSummonResult TrySingleSummon(
            GachaRateTable rateTable,
            IGachaCharacterPool pool)
        {
            return TrySummonInternal(rateTable, pool, rateTable.SinglePullCost, 1, applyTenPullGuarantee: false);
        }

        /// <summary>
        /// 10연차 소환을 실행한다. SR 이상 1개를 보장한다.
        /// </summary>
        public GachaSummonResult TryTenSummon(
            GachaRateTable rateTable,
            IGachaCharacterPool pool)
        {
            return TrySummonInternal(rateTable, pool, rateTable.TenPullCost, TEN_PULL_COUNT, applyTenPullGuarantee: true);
        }

        /// <summary>
        /// 세이브용 스냅샷을 생성한다.
        /// </summary>
        public GachaSaveData ToSaveData()
        {
            return new GachaSaveData
            {
                Pity = _pity.ToSaveData(),
                Ledger = _ledger.ToSaveData(),
                NextSeed = _nextSeed,
            };
        }

        /// <summary>
        /// 세이브 스냅샷에서 GachaService 를 복원한다.
        /// </summary>
        public static GachaService FromSaveData(
            GachaSaveData saveData,
            Wallet wallet,
            ExplorerCatalog catalog,
            BalanceTable balanceTable,
            Func<DateTimeOffset> utcNow = null,
            Func<long> seedProvider = null,
            Func<long, IRandomSource> randomFactory = null)
        {
            var ledger = GachaSummonLedger.FromSaveData(saveData?.Ledger, utcNow);
            var pity = GachaPityState.FromSaveData(saveData?.Pity);
            var nextSeed = saveData?.NextSeed ?? 1L;

            return new GachaService(
                wallet,
                catalog,
                ledger,
                pity,
                balanceTable,
                nextSeed,
                seedProvider,
                randomFactory);
        }

        private GachaSummonResult TrySummonInternal(
            GachaRateTable rateTable,
            IGachaCharacterPool pool,
            int cost,
            int pullCount,
            bool applyTenPullGuarantee)
        {
            if (rateTable == null)
                throw new ArgumentNullException(nameof(rateTable));
            if (pool == null)
                throw new ArgumentNullException(nameof(pool));

            try
            {
                rateTable.ValidateRates();
            }
            catch (InvalidOperationException)
            {
                return GachaSummonResult.InvalidTable();
            }

            if (!_wallet.CanAfford(CurrencyType.AbyssStone, cost))
                return GachaSummonResult.InsufficientBalance();

            var debit = _wallet.TryDebit(
                CurrencyType.AbyssStone,
                cost,
                CurrencyReasonCodes.SummonCost);

            if (!debit.Success)
                return GachaSummonResult.InsufficientBalance();

            var seed = AllocateSeed();
            var random = _randomFactory(seed);
            var pulls = new GachaPullResult[pullCount];

            for (var i = 0; i < pullCount; i++)
                pulls[i] = RollPull(rateTable, pool, random);

            if (applyTenPullGuarantee)
                GachaRoller.ApplyTenPullGuarantee(pulls, pool, random);

            for (var i = 0; i < pullCount; i++)
                CommitPull(pulls[i]);

            var records = ToPullRecords(pulls);
            _ledger.Record(seed, pool.BannerId, records);

            return GachaSummonResult.Succeeded(seed, pool.BannerId, pulls, _pity);
        }

        private GachaPullResult RollPull(
            GachaRateTable rateTable,
            IGachaCharacterPool pool,
            IRandomSource random)
        {
            _pity.IncrementCounters();

            var grade = GachaRoller.RollGrade(rateTable, _pity, random, out var ssrPity, out var urPity);
            var characterId = pool.PickCharacter(grade, random);

            return new GachaPullResult(grade, characterId, ssrPity, urPity);
        }

        private void CommitPull(GachaPullResult pull)
        {
            GachaRoller.ApplyPityReset(_pity, pull.Grade);
            _catalog.TryAcquire(pull.CharacterId, pull.Grade, _balanceTable);
        }

        private long AllocateSeed()
        {
            if (_seedProvider != null)
                return _seedProvider();

            var seed = _nextSeed;
            _nextSeed++;
            return seed;
        }

        private static GachaPullRecord[] ToPullRecords(GachaPullResult[] pulls)
        {
            var records = new GachaPullRecord[pulls.Length];

            for (var i = 0; i < pulls.Length; i++)
            {
                var pull = pulls[i];
                records[i] = new GachaPullRecord
                {
                    Grade = pull.Grade,
                    CharacterId = pull.CharacterId,
                    TriggeredSsrPity = pull.TriggeredSsrPity,
                    TriggeredUrPity = pull.TriggeredUrPity,
                    TenPullGuaranteeApplied = pull.TenPullGuaranteeApplied,
                };
            }

            return records;
        }
    }
}
