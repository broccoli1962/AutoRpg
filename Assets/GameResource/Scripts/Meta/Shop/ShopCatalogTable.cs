using System;
using System.Collections.Generic;

namespace Backend.Meta.Shop
{
    /// <summary>
    /// 단일 IAP 상품 정의.
    /// </summary>
    [Serializable]
    public sealed class ShopProductDefinition
    {
        public string ProductId;
        public string StoreProductId;
        public ShopProductCategory Category;
        public ShopProductKind ProductKind = ShopProductKind.Consumable;
        public string DisplayNameKey;
        public int PriceKrw;
        public int PurchaseLimit;
        public bool FirstPurchaseBonusEligible;
        public int BaseAbyssStoneAmount;
        public int RequiredFloor;
        public int SubscriptionDailyAbyssStone;
        public int SubscriptionDurationDays = 30;
        public ShopRewardEntry[] Rewards = Array.Empty<ShopRewardEntry>();
        public ShopEntitlementType Entitlement = ShopEntitlementType.None;

        /// <summary>
        /// 1회 한정 상품 여부를 반환한다.
        /// </summary>
        public bool IsOneTimeLimited => PurchaseLimit == 1;

        /// <summary>
        /// 첫 구매 2배 대상 여부를 반환한다.
        /// </summary>
        public bool HasFirstPurchaseBonus => FirstPurchaseBonusEligible && BaseAbyssStoneAmount > 0;
    }

    /// <summary>
    /// 상점 카탈로그 ScriptableObject.
    /// </summary>
    public sealed class ShopCatalogTable : UnityEngine.ScriptableObject
    {
        public List<ShopProductDefinition> Products = new();

        /// <summary>
        /// 내부 ProductId 로 상품을 조회한다.
        /// </summary>
        public ShopProductDefinition FindByProductId(string productId)
        {
            if (string.IsNullOrEmpty(productId) || Products == null)
                return null;

            foreach (var product in Products)
            {
                if (product != null && product.ProductId == productId)
                    return product;
            }

            return null;
        }

        /// <summary>
        /// 스토어 ProductId 로 상품을 조회한다.
        /// </summary>
        public ShopProductDefinition FindByStoreProductId(string storeProductId)
        {
            if (string.IsNullOrEmpty(storeProductId) || Products == null)
                return null;

            foreach (var product in Products)
            {
                if (product != null && product.StoreProductId == storeProductId)
                    return product;
            }

            return null;
        }

        /// <summary>
        /// spec.md 4.3 기본 상품 구성을 적용한다.
        /// </summary>
        public void ApplySpecDefaults()
        {
            Products ??= new List<ShopProductDefinition>();

            if (Products.Count > 0)
                return;

            Products.Add(new ShopProductDefinition
            {
                ProductId = "starter_growth_pack",
                StoreProductId = "com.abysschronicle.shop.starter_pack",
                Category = ShopProductCategory.StarterGrowthPack,
                ProductKind = ShopProductKind.NonConsumable,
                DisplayNameKey = "shop.product.starter_pack",
                PriceKrw = 1100,
                PurchaseLimit = 1,
                Entitlement = ShopEntitlementType.None,
                Rewards = new[]
                {
                    new ShopRewardEntry
                    {
                        RewardType = ShopRewardType.Currency,
                        CurrencyType = Currency.CurrencyType.AbyssStone,
                        Amount = 1200,
                    },
                    new ShopRewardEntry
                    {
                        RewardType = ShopRewardType.Character,
                        CharacterId = "explorer_sr_starter",
                        CharacterGrade = Characters.ExplorerGrade.SR,
                    },
                    new ShopRewardEntry
                    {
                        RewardType = ShopRewardType.Currency,
                        CurrencyType = Currency.CurrencyType.SummonTicket,
                        Amount = 5,
                    },
                },
            });

            AddAbyssStonePack("abyss_stone_1", "com.abysschronicle.shop.abyss_stone_1", 3300, 300);
            AddAbyssStonePack("abyss_stone_2", "com.abysschronicle.shop.abyss_stone_2", 11000, 1100);
            AddAbyssStonePack("abyss_stone_3", "com.abysschronicle.shop.abyss_stone_3", 33000, 3500);
            AddAbyssStonePack("abyss_stone_4", "com.abysschronicle.shop.abyss_stone_4", 110000, 12500);

            Products.Add(new ShopProductDefinition
            {
                ProductId = "monthly_abyss_contract",
                StoreProductId = "com.abysschronicle.shop.monthly_contract",
                Category = ShopProductCategory.MonthlyAbyssContract,
                ProductKind = ShopProductKind.Subscription,
                DisplayNameKey = "shop.product.monthly_contract",
                PriceKrw = 11000,
                SubscriptionDailyAbyssStone = 100,
                SubscriptionDurationDays = 30,
                Entitlement = ShopEntitlementType.MonthlyContractActive,
                Rewards = new[]
                {
                    new ShopRewardEntry
                    {
                        RewardType = ShopRewardType.Currency,
                        CurrencyType = Currency.CurrencyType.AbyssStone,
                        Amount = 1200,
                    },
                    new ShopRewardEntry
                    {
                        RewardType = ShopRewardType.Entitlement,
                        Entitlement = ShopEntitlementType.MonthlyContractActive,
                    },
                },
            });

            Products.Add(new ShopProductDefinition
            {
                ProductId = "season_pass",
                StoreProductId = "com.abysschronicle.shop.season_pass",
                Category = ShopProductCategory.SeasonPass,
                ProductKind = ShopProductKind.NonConsumable,
                DisplayNameKey = "shop.product.season_pass",
                PriceKrw = 9900,
                Entitlement = ShopEntitlementType.SeasonPassPremium,
                Rewards = new[]
                {
                    new ShopRewardEntry
                    {
                        RewardType = ShopRewardType.Entitlement,
                        Entitlement = ShopEntitlementType.SeasonPassPremium,
                    },
                },
            });

            Products.Add(new ShopProductDefinition
            {
                ProductId = "ad_removal",
                StoreProductId = "com.abysschronicle.shop.ad_removal",
                Category = ShopProductCategory.AdRemoval,
                ProductKind = ShopProductKind.NonConsumable,
                DisplayNameKey = "shop.product.ad_removal",
                PriceKrw = 5500,
                Entitlement = ShopEntitlementType.PermanentAdRemoval,
                Rewards = new[]
                {
                    new ShopRewardEntry
                    {
                        RewardType = ShopRewardType.Entitlement,
                        Entitlement = ShopEntitlementType.PermanentAdRemoval,
                    },
                },
            });

            AddGrowthPack("growth_pack_50", "com.abysschronicle.shop.growth_pack_50", 50, 800, 2200);
            AddGrowthPack("growth_pack_100", "com.abysschronicle.shop.growth_pack_100", 100, 1500, 5500);
        }

        private void AddAbyssStonePack(string productId, string storeProductId, int priceKrw, int amount)
        {
            Products.Add(new ShopProductDefinition
            {
                ProductId = productId,
                StoreProductId = storeProductId,
                Category = ShopProductCategory.AbyssStonePack,
                ProductKind = ShopProductKind.Consumable,
                DisplayNameKey = $"shop.product.{productId}",
                PriceKrw = priceKrw,
                FirstPurchaseBonusEligible = true,
                BaseAbyssStoneAmount = amount,
                Rewards = new[]
                {
                    new ShopRewardEntry
                    {
                        RewardType = ShopRewardType.Currency,
                        CurrencyType = Currency.CurrencyType.AbyssStone,
                        Amount = amount,
                    },
                },
            });
        }

        private void AddGrowthPack(
            string productId,
            string storeProductId,
            int requiredFloor,
            int abyssStone,
            int priceKrw)
        {
            Products.Add(new ShopProductDefinition
            {
                ProductId = productId,
                StoreProductId = storeProductId,
                Category = ShopProductCategory.TieredGrowthPack,
                ProductKind = ShopProductKind.NonConsumable,
                DisplayNameKey = $"shop.product.{productId}",
                PriceKrw = priceKrw,
                PurchaseLimit = 1,
                RequiredFloor = requiredFloor,
                Rewards = new[]
                {
                    new ShopRewardEntry
                    {
                        RewardType = ShopRewardType.Currency,
                        CurrencyType = Currency.CurrencyType.AbyssStone,
                        Amount = abyssStone,
                    },
                    new ShopRewardEntry
                    {
                        RewardType = ShopRewardType.Currency,
                        CurrencyType = Currency.CurrencyType.SummonTicket,
                        Amount = 3,
                    },
                },
            });
        }
    }
}
