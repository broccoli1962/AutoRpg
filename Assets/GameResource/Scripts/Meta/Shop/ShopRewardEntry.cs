using System;
using Backend.Meta.Characters;
using Backend.Meta.Currency;

namespace Backend.Meta.Shop
{
    /// <summary>
    /// 상점 상품 지급 항목.
    /// </summary>
    [Serializable]
    public sealed class ShopRewardEntry
    {
        public ShopRewardType RewardType = ShopRewardType.Currency;
        public CurrencyType CurrencyType = CurrencyType.AbyssStone;
        public long Amount;
        public string CharacterId;
        public ExplorerGrade CharacterGrade = ExplorerGrade.SR;
        public ShopEntitlementType Entitlement = ShopEntitlementType.None;
    }

    /// <summary>
    /// 상점 보상 유형.
    /// </summary>
    public enum ShopRewardType
    {
        Currency = 0,
        Character = 1,
        Entitlement = 2,
    }

    /// <summary>
    /// 상점 구매로 부여되는 영구/기간 권한.
    /// </summary>
    public enum ShopEntitlementType
    {
        None = 0,
        PermanentAdRemoval = 1,
        MonthlyContractActive = 2,
        SeasonPassPremium = 3,
    }
}
