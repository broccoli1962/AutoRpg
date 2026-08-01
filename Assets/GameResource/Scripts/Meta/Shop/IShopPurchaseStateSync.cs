namespace Backend.Meta.Shop
{
    /// <summary>
    /// 1회 한정·첫 구매 보너스 소진 상태를 서버에 영속화한다.
    /// </summary>
    public interface IShopPurchaseStateSync
    {
        /// <summary>
        /// 소진/보너스 사용 상태를 서버에 저장한다.
        /// </summary>
        void PersistPurchaseState(ShopSaveData saveData);

        /// <summary>
        /// 서버에서 구매 상태를 복원한다.
        /// </summary>
        bool TryRestorePurchaseState(out ShopSaveData saveData);
    }

    /// <summary>
    /// 개발용 로컬 영속화 구현. 메모리에 상태를 유지한다.
    /// </summary>
    public sealed class LocalStubShopPurchaseStateSync : IShopPurchaseStateSync
    {
        private ShopSaveData _cached;

        /// <summary>
        /// 상태를 로컬 캐시에 저장한다.
        /// </summary>
        public void PersistPurchaseState(ShopSaveData saveData)
        {
            _cached = Clone(saveData);
        }

        /// <summary>
        /// 로컬 캐시에서 상태를 복원한다.
        /// </summary>
        public bool TryRestorePurchaseState(out ShopSaveData saveData)
        {
            if (_cached == null)
            {
                saveData = null;
                return false;
            }

            saveData = Clone(_cached);
            return true;
        }

        private static ShopSaveData Clone(ShopSaveData source)
        {
            if (source == null)
                return null;

            return new ShopSaveData
            {
                ConsumedOneTimeProductIds = (string[])source.ConsumedOneTimeProductIds?.Clone(),
                FirstPurchaseBonusUsedProductIds = (string[])source.FirstPurchaseBonusUsedProductIds?.Clone(),
                ProcessedTransactionIds = (string[])source.ProcessedTransactionIds?.Clone(),
                MonthlyContractExpiryUtc = source.MonthlyContractExpiryUtc,
                SubscriptionDailyPeriodKey = source.SubscriptionDailyPeriodKey,
                HasPermanentAdRemoval = source.HasPermanentAdRemoval,
            };
        }
    }
}
