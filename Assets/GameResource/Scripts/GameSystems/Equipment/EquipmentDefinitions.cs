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
        public const string SporeKnifeId = "spore_knife";
        public const string PaddedTunicId = "padded_tunic";
        public const string HollowDaggerId = "hollow_dagger";
        public const string SilverGuardId = "silver_guard";
        public const string PrismBladeId = "prism_blade";
        public const string ObsidianPlateId = "obsidian_plate";
        public const string WardenGavelId = "warden_gavel";
        public const string SilentAegisId = "silent_aegis";
        public const string AbyssFangId = "abyss_fang";
        public const string ThresholdCleaverId = "threshold_cleaver";
        public const string VoidMantleId = "void_mantle";

        public const string GuardianSetId = "guardian";
        public const string AbyssSetId = "abyss";
        public const string PrismSetId = "prism";

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
            VitBonus = 2,
            SetId = GuardianSetId
        };

        private static readonly EquipmentDefinition SporeKnife = new()
        {
            Id = SporeKnifeId,
            DisplayName = "포자 단검",
            Grade = EquipmentGrade.Uncommon,
            Slot = EquipmentSlot.Weapon,
            AgiBonus = 3,
            StrBonus = 1
        };

        private static readonly EquipmentDefinition PaddedTunic = new()
        {
            Id = PaddedTunicId,
            DisplayName = "완충 튜닉",
            Grade = EquipmentGrade.Uncommon,
            Slot = EquipmentSlot.Armor,
            VitBonus = 3
        };

        private static readonly EquipmentDefinition HollowDagger = new()
        {
            Id = HollowDaggerId,
            DisplayName = "공허의 단검",
            Grade = EquipmentGrade.Rare,
            Slot = EquipmentSlot.Weapon,
            AgiBonus = 4,
            StrBonus = 1,
            SetId = AbyssSetId
        };

        private static readonly EquipmentDefinition SilverGuard = new()
        {
            Id = SilverGuardId,
            DisplayName = "은빛 수호구",
            Grade = EquipmentGrade.Rare,
            Slot = EquipmentSlot.Armor,
            VitBonus = 4,
            StrBonus = 1,
            SetId = GuardianSetId
        };

        private static readonly EquipmentDefinition PrismBlade = new()
        {
            Id = PrismBladeId,
            DisplayName = "프리즘 검",
            Grade = EquipmentGrade.Epic,
            Slot = EquipmentSlot.Weapon,
            IntBonus = 3,
            StrBonus = 2,
            SetId = PrismSetId
        };

        private static readonly EquipmentDefinition ObsidianPlate = new()
        {
            Id = ObsidianPlateId,
            DisplayName = "흑요석 판금",
            Grade = EquipmentGrade.Epic,
            Slot = EquipmentSlot.Armor,
            VitBonus = 6,
            StrBonus = 2,
            SetId = PrismSetId
        };

        private static readonly EquipmentDefinition WardenGavel = new()
        {
            Id = WardenGavelId,
            DisplayName = "수호자의 성픔",
            Grade = EquipmentGrade.Legendary,
            Slot = EquipmentSlot.Weapon,
            IntBonus = 4,
            VitBonus = 2,
            SetId = GuardianSetId
        };

        private static readonly EquipmentDefinition SilentAegis = new()
        {
            Id = SilentAegisId,
            DisplayName = "침묵의 방패",
            Grade = EquipmentGrade.Legendary,
            Slot = EquipmentSlot.Armor,
            VitBonus = 8,
            StrBonus = 1,
            SetId = GuardianSetId
        };

        private static readonly EquipmentDefinition AbyssFang = new()
        {
            Id = AbyssFangId,
            DisplayName = "심연의 송곳니",
            Grade = EquipmentGrade.Legendary,
            Slot = EquipmentSlot.Weapon,
            AgiBonus = 5,
            StrBonus = 3,
            SetId = AbyssSetId
        };

        private static readonly EquipmentDefinition ThresholdCleaver = new()
        {
            Id = ThresholdCleaverId,
            DisplayName = "문턱의 참격검",
            Grade = EquipmentGrade.Mythic,
            Slot = EquipmentSlot.Weapon,
            StrBonus = 6,
            VitBonus = 3,
            SetId = AbyssSetId
        };

        private static readonly EquipmentDefinition VoidMantle = new()
        {
            Id = VoidMantleId,
            DisplayName = "공허의 망토",
            Grade = EquipmentGrade.Mythic,
            Slot = EquipmentSlot.Armor,
            VitBonus = 10,
            IntBonus = 3,
            SetId = AbyssSetId
        };

        public static IReadOnlyList<EquipmentDefinition> All { get; } = new List<EquipmentDefinition>
        {
            RustyBlade,
            MossLeather,
            SporeKnife,
            PaddedTunic,
            HollowDagger,
            SilverGuard,
            PrismBlade,
            ObsidianPlate,
            WardenGavel,
            SilentAegis,
            AbyssFang,
            ThresholdCleaver,
            VoidMantle
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

        public static EquipmentDefinition RollDrop(
            MonsterRarity rarity,
            DeterministicRandom random,
            string zoneId,
            int floor = 0)
        {
            var maxGrade = GetMaxDropGrade(zoneId, rarity, floor);
            var candidates = new List<EquipmentDefinition>();
            foreach (var item in All)
            {
                if ((int)item.Grade <= (int)maxGrade)
                    candidates.Add(item);
            }

            if (candidates.Count == 0)
                return random.NextFloat() < 0.5f ? RustyBlade : MossLeather;

            var targetGrade = RollTargetGrade(random, maxGrade, rarity);
            var gradePool = new List<EquipmentDefinition>();
            foreach (var item in candidates)
            {
                if (item.Grade == targetGrade)
                    gradePool.Add(item);
            }

            if (gradePool.Count == 0)
                gradePool = candidates;

            return gradePool[random.NextInt(gradePool.Count)];
        }

        private static EquipmentGrade GetMaxDropGrade(string zoneId, MonsterRarity rarity, int floor)
        {
            var zoneIndex = ZoneDefinitions.GetZoneIndex(zoneId);
            var maxGrade = zoneIndex switch
            {
                0 => EquipmentGrade.Uncommon,
                1 => EquipmentGrade.Rare,
                2 => EquipmentGrade.Rare,
                3 => EquipmentGrade.Epic,
                4 => EquipmentGrade.Legendary,
                _ => EquipmentGrade.Legendary
            };

            if (zoneIndex >= 5)
            {
                maxGrade = floor > ZoneDefinitions.AbyssalThresholdMaxFloor || rarity >= MonsterRarity.Boss
                    ? EquipmentGrade.Mythic
                    : EquipmentGrade.Legendary;
            }
            else if (rarity >= MonsterRarity.Boss && (int)maxGrade < (int)EquipmentGrade.Legendary)
            {
                maxGrade = EquipmentGrade.Legendary;
            }

            return maxGrade;
        }

        private static EquipmentGrade RollTargetGrade(
            DeterministicRandom random,
            EquipmentGrade maxGrade,
            MonsterRarity rarity)
        {
            var roll = random.NextFloat();
            var maxIndex = (int)maxGrade;

            if (rarity >= MonsterRarity.Boss)
                roll += 0.25f;
            else if (rarity >= MonsterRarity.Rare)
                roll += 0.12f;

            var targetIndex = roll switch
            {
                >= 0.92f when maxIndex >= (int)EquipmentGrade.Mythic => (int)EquipmentGrade.Mythic,
                >= 0.82f when maxIndex >= (int)EquipmentGrade.Legendary => (int)EquipmentGrade.Legendary,
                >= 0.68f when maxIndex >= (int)EquipmentGrade.Epic => (int)EquipmentGrade.Epic,
                >= 0.48f when maxIndex >= (int)EquipmentGrade.Rare => (int)EquipmentGrade.Rare,
                >= 0.28f when maxIndex >= (int)EquipmentGrade.Uncommon => (int)EquipmentGrade.Uncommon,
                _ => (int)EquipmentGrade.Common
            };

            if (targetIndex > maxIndex)
                targetIndex = maxIndex;

            return (EquipmentGrade)targetIndex;
        }
    }
}
