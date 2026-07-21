using System.Collections.Generic;
using Backend.GameSystems.Equipment.Data;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Simulation;

namespace Backend.GameSystems.Equipment
{
    public static class EquipmentDefinitions
    {
        public const string RustyBladeId = "rusty_blade";
        public const string MossLeatherId = "moss_leather";
        public const string HollowDaggerId = "hollow_dagger";
        public const string SilverGuardId = "silver_guard";

        private static readonly EquipmentDefinition RustyBlade = new()
        {
            Id = RustyBladeId,
            DisplayName = "녹슨 검",
            Grade = EquipmentGrade.Common,
            Slot = EquipmentSlot.Weapon,
            StrBonus = 2
        };

        private static readonly EquipmentDefinition MossLeather = new()
        {
            Id = MossLeatherId,
            DisplayName = "이끼 가죽 갑옷",
            Grade = EquipmentGrade.Common,
            Slot = EquipmentSlot.Armor,
            VitBonus = 2
        };

        private static readonly EquipmentDefinition HollowDagger = new()
        {
            Id = HollowDaggerId,
            DisplayName = "공허의 단검",
            Grade = EquipmentGrade.Rare,
            Slot = EquipmentSlot.Weapon,
            AgiBonus = 4,
            StrBonus = 1
        };

        private static readonly EquipmentDefinition SilverGuard = new()
        {
            Id = SilverGuardId,
            DisplayName = "은빛 수호구",
            Grade = EquipmentGrade.Rare,
            Slot = EquipmentSlot.Armor,
            VitBonus = 4,
            StrBonus = 1
        };

        public static IReadOnlyList<EquipmentDefinition> All { get; } = new List<EquipmentDefinition>
        {
            RustyBlade,
            MossLeather,
            HollowDagger,
            SilverGuard
        };

        public static EquipmentDefinition Get(string id)
        {
            foreach (var item in All)
            {
                if (item.Id == id)
                    return item;
            }

            return null;
        }

        public static EquipmentDefinition RollDrop(MonsterRarity rarity, DeterministicRandom random)
        {
            var roll = random.NextFloat();
            if (rarity >= MonsterRarity.Rare)
            {
                return roll < 0.5f ? HollowDagger : SilverGuard;
            }

            if (rarity >= MonsterRarity.Notable && roll < 0.25f)
            {
                return roll < 0.5f ? HollowDagger : SilverGuard;
            }

            return roll < 0.5f ? RustyBlade : MossLeather;
        }
    }
}
