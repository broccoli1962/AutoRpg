using System;

namespace Backend.Meta.IAP
{
    /// <summary>
    /// 검증 대기 또는 재처리 대상 IAP 트랜잭션.
    /// </summary>
    [Serializable]
    public sealed class IapPendingTransaction
    {
        public string StoreProductId;
        public string TransactionId;
        public string Receipt;
        public string Platform;
        public DateTimeOffset QueuedAtUtc;
    }

    /// <summary>
    /// IAP 세이브 데이터.
    /// </summary>
    [Serializable]
    public sealed class IapSaveData
    {
        public IapPendingTransaction[] PendingTransactions = Array.Empty<IapPendingTransaction>();
    }
}
