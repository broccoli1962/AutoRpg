namespace Backend.Meta.Shop
{
    /// <summary>
    /// 상점 구매 처리 결과.
    /// </summary>
    public readonly struct ShopPurchaseResult
    {
        public bool Success { get; }
        public string ProductId { get; }
        public string Reason { get; }
        public int GrantedRewardCount { get; }

        private ShopPurchaseResult(
            bool success,
            string productId,
            string reason,
            int grantedRewardCount)
        {
            Success = success;
            ProductId = productId;
            Reason = reason;
            GrantedRewardCount = grantedRewardCount;
        }

        /// <summary>
        /// 성공 결과를 생성한다.
        /// </summary>
        public static ShopPurchaseResult Succeeded(string productId, int grantedRewardCount)
        {
            return new ShopPurchaseResult(true, productId, null, grantedRewardCount);
        }

        /// <summary>
        /// 실패 결과를 생성한다.
        /// </summary>
        public static ShopPurchaseResult Failed(string productId, string reason)
        {
            return new ShopPurchaseResult(false, productId, reason, 0);
        }
    }
}
