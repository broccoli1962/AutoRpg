using System;
using System.Collections.Generic;

namespace Backend.Meta.Currency
{
    /// <summary>
    /// Wallet 세이브 데이터.
    /// </summary>
    [Serializable]
    public sealed class WalletSaveData
    {
        public CurrencyBalanceEntry[] Balances = Array.Empty<CurrencyBalanceEntry>();
        public TransactionLedgerSaveData Ledger = new();
    }

    /// <summary>
    /// 재화 타입별 잔액 1건.
    /// </summary>
    [Serializable]
    public struct CurrencyBalanceEntry
    {
        public CurrencyType Type;
        public long Amount;
    }
}
