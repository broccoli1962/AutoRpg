using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Backend.Meta.IAP
{
    /// <summary>
    /// EditMode·서버 없는 개발 빌드용 IAP 스토어 스텁.
    /// </summary>
    public sealed class SimulatedIapStoreBridge : IIapStoreBridge
    {
        private readonly HashSet<string> _registeredProductIds = new();
        private readonly Dictionary<string, string> _ownedNonConsumables = new();
        private int _transactionCounter;

        public bool IsInitialized { get; private set; }

        /// <summary>
        /// 스토어를 초기화한다.
        /// </summary>
        public UniTask<bool> InitializeAsync(string[] storeProductIds)
        {
            _registeredProductIds.Clear();

            if (storeProductIds != null)
            {
                foreach (var productId in storeProductIds)
                {
                    if (!string.IsNullOrEmpty(productId))
                        _registeredProductIds.Add(productId);
                }
            }

            IsInitialized = true;
            return UniTask.FromResult(true);
        }

        /// <summary>
        /// 상품 구매를 시뮬레이션한다.
        /// </summary>
        public UniTask<IapStorePurchaseResult> PurchaseAsync(string storeProductId)
        {
            if (!IsInitialized)
                return UniTask.FromResult(IapStorePurchaseResult.Failed(storeProductId, "Store is not initialized."));

            if (!_registeredProductIds.Contains(storeProductId))
                return UniTask.FromResult(IapStorePurchaseResult.Failed(storeProductId, "Unknown store product id."));

            if (_ownedNonConsumables.ContainsKey(storeProductId))
            {
                return UniTask.FromResult(
                    IapStorePurchaseResult.Failed(storeProductId, "Non-consumable already owned."));
            }

            var transactionId = $"sim_tx_{++_transactionCounter}_{Guid.NewGuid():N}";
            var receipt = $"sim_receipt:{storeProductId}:{transactionId}";

            if (IsNonConsumable(storeProductId))
                _ownedNonConsumables[storeProductId] = transactionId;

            return UniTask.FromResult(IapStorePurchaseResult.Succeeded(
                storeProductId,
                transactionId,
                receipt,
                "simulated"));
        }

        /// <summary>
        /// 소유 중인 비소모성 상품을 복원한다.
        /// </summary>
        public UniTask<IapStorePurchaseResult[]> RestorePurchasesAsync()
        {
            if (!IsInitialized)
                return UniTask.FromResult(Array.Empty<IapStorePurchaseResult>());

            var restored = new List<IapStorePurchaseResult>();

            foreach (var pair in _ownedNonConsumables)
            {
                restored.Add(IapStorePurchaseResult.Succeeded(
                    pair.Key,
                    pair.Value,
                    $"sim_receipt_restore:{pair.Key}",
                    "simulated",
                    isRestored: true));
            }

            return UniTask.FromResult(restored.ToArray());
        }

        /// <summary>
        /// 시뮬레이션에서는 no-op.
        /// </summary>
        public void ConfirmPendingPurchase(string storeProductId, string transactionId)
        {
        }

        private static bool IsNonConsumable(string storeProductId)
        {
            return storeProductId.Contains("starter_pack", StringComparison.Ordinal)
                || storeProductId.Contains("season_pass", StringComparison.Ordinal)
                || storeProductId.Contains("ad_removal", StringComparison.Ordinal)
                || storeProductId.Contains("growth_pack", StringComparison.Ordinal);
        }
    }
}
