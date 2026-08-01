using System;
using System.Collections.Generic;

namespace Backend.Meta.Currency
{
    /// <summary>
    /// 재화 잔액 조회·증감·차감 가능 여부를 제공한다. 모든 변동은 TransactionLedger 에 기록된다.
    /// </summary>
    public sealed class Wallet
    {
        private const string INVALID_AMOUNT_REASON = "Amount must be greater than zero.";
        private const string INSUFFICIENT_BALANCE_REASON = "Insufficient balance.";
        private const string OVERFLOW_REASON = "Balance overflow.";

        private readonly Dictionary<CurrencyType, long> _balances = new();
        private readonly TransactionLedger _ledger;

        public Wallet(TransactionLedger ledger)
        {
            _ledger = ledger ?? throw new ArgumentNullException(nameof(ledger));
        }

        /// <summary>
        /// 연결된 거래 원장을 반환한다.
        /// </summary>
        public TransactionLedger Ledger => _ledger;

        /// <summary>
        /// 특정 재화 잔액을 반환한다.
        /// </summary>
        public long GetBalance(CurrencyType type)
        {
            return _balances.TryGetValue(type, out var balance) ? balance : 0L;
        }

        /// <summary>
        /// 차감 가능 여부를 판정한다.
        /// </summary>
        public bool CanAfford(CurrencyType type, long amount)
        {
            if (amount <= 0L)
                return false;

            return GetBalance(type) >= amount;
        }

        /// <summary>
        /// 재화를 증가시킨다. amount 는 양수여야 한다.
        /// </summary>
        public CurrencyTransactionResult TryCredit(
            CurrencyType type,
            long amount,
            string reasonCode)
        {
            if (amount <= 0L)
            {
                return CurrencyTransactionResult.Failed(
                    type,
                    amount,
                    GetBalance(type),
                    reasonCode,
                    INVALID_AMOUNT_REASON);
            }

            var current = GetBalance(type);

            if (current > long.MaxValue - amount)
            {
                return CurrencyTransactionResult.Failed(
                    type,
                    amount,
                    current,
                    reasonCode,
                    OVERFLOW_REASON);
            }

            var next = current + amount;
            _balances[type] = next;
            _ledger.Record(reasonCode, type, amount, next);

            return CurrencyTransactionResult.Succeeded(type, amount, next, reasonCode);
        }

        /// <summary>
        /// 재화를 차감한다. amount 는 양수여야 하며, 부족 시 실패 결과를 반환한다.
        /// </summary>
        public CurrencyTransactionResult TryDebit(
            CurrencyType type,
            long amount,
            string reasonCode)
        {
            if (amount <= 0L)
            {
                return CurrencyTransactionResult.Failed(
                    type,
                    -amount,
                    GetBalance(type),
                    reasonCode,
                    INVALID_AMOUNT_REASON);
            }

            var current = GetBalance(type);

            if (current < amount)
            {
                return CurrencyTransactionResult.Failed(
                    type,
                    -amount,
                    current,
                    reasonCode,
                    INSUFFICIENT_BALANCE_REASON);
            }

            var next = current - amount;
            _balances[type] = next;
            _ledger.Record(reasonCode, type, -amount, next);

            return CurrencyTransactionResult.Succeeded(type, -amount, next, reasonCode);
        }

        /// <summary>
        /// 세이브용 스냅샷을 생성한다.
        /// </summary>
        public WalletSaveData ToSaveData()
        {
            var balances = new CurrencyBalanceEntry[_balances.Count];
            var index = 0;

            foreach (var pair in _balances)
            {
                balances[index++] = new CurrencyBalanceEntry
                {
                    Type = pair.Key,
                    Amount = pair.Value,
                };
            }

            return new WalletSaveData
            {
                Balances = balances,
                Ledger = _ledger.ToSaveData(),
            };
        }

        /// <summary>
        /// 세이브 스냅샷에서 Wallet 을 복원한다.
        /// </summary>
        public static Wallet FromSaveData(
            WalletSaveData saveData,
            Func<DateTimeOffset> utcNow = null)
        {
            var ledger = TransactionLedger.FromSaveData(saveData?.Ledger, utcNow);
            var wallet = new Wallet(ledger);

            if (saveData?.Balances == null)
                return wallet;

            foreach (var entry in saveData.Balances)
            {
                if (entry.Amount < 0L)
                    continue;

                wallet._balances[entry.Type] = entry.Amount;
            }

            return wallet;
        }
    }
}
