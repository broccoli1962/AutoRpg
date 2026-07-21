using System.Collections.Generic;

namespace Backend.GameSystems.Exploration.Data
{
    public static class ZoneDefinitions
    {
        public const string MossyHollowId = "mossy_hollow";
        public const string MossyHollowDisplayName = "이끼 동굴";
        public const int MossyHollowMaxFloor = 15;

        public const float BaseProgressPerTick = 4.5f;
        public const float FloorDifficultyStep = 0.08f;
        public const float BaseEventRollChance = 0.35f;

        private static readonly MonsterDefinition[] MossyHollowMonsters =
        {
            new("fungus_slime", "곰팡이 슬라임", MonsterRarity.Common, 12, 4, 6, 8),
            new("cave_rat", "동굴 쥐", MonsterRarity.Common, 10, 3, 5, 6),
            new("silver_bat_swarm", "은빛 박쥐떼", MonsterRarity.Notable, 22, 8, 14, 18),
            new("moss_guardian", "이끼 수호자", MonsterRarity.Rare, 40, 14, 24, 35),
            new("hollow_boss", "심연의 이끼 군주", MonsterRarity.Boss, 80, 20, 40, 60)
        };

        private static readonly string[] MoveDescriptionIds =
        {
            "damp_passage",
            "glowing_moss",
            "narrow_tunnel",
            "water_drip",
            "fungal_scent"
        };

        private static readonly DiscoveryDefinition[] Discoveries =
        {
            new("mana_shard", "마나결정", 1, 12),
            new("old_coin_pouch", "낡은 은화 주머니", 1, 25),
            new("healing_herb", "치유 이끼", 1, 8),
            new("rusty_ring", "낡은 반지", 1, 15)
        };

        public static string GetZoneDisplayName(string zoneId)
        {
            return zoneId switch
            {
                MossyHollowId => MossyHollowDisplayName,
                _ => zoneId
            };
        }

        public static int GetMaxFloor(string zoneId)
        {
            return zoneId switch
            {
                MossyHollowId => MossyHollowMaxFloor,
                _ => MossyHollowMaxFloor
            };
        }

        public static float GetFloorDifficulty(string zoneId, int floor)
        {
            _ = zoneId;
            return 1f + (floor - 1) * FloorDifficultyStep;
        }

        public static IReadOnlyList<MonsterDefinition> GetMonsters(string zoneId)
        {
            return zoneId switch
            {
                MossyHollowId => MossyHollowMonsters,
                _ => MossyHollowMonsters
            };
        }

        public static IReadOnlyList<string> GetMoveDescriptionIds() => MoveDescriptionIds;

        public static IReadOnlyList<DiscoveryDefinition> GetDiscoveries() => Discoveries;

        public static PartyState CreateDefaultParty()
        {
            return new PartyState
            {
                Members =
                {
                    CreateCharacter("char_lena", "레나", CharacterRole.Warrior, PersonalityTag.Cautious, 14, 10, 8, 12, 6),
                    CreateCharacter("char_marco", "마르코", CharacterRole.Rogue, PersonalityTag.Loyal, 10, 14, 8, 10, 8),
                    CreateCharacter("char_kyle", "카일", CharacterRole.Mage, PersonalityTag.Greedy, 8, 10, 14, 9, 10)
                }
            };
        }

        private static CharacterState CreateCharacter(
            string id,
            string displayName,
            CharacterRole role,
            PersonalityTag personality,
            int str,
            int agi,
            int intel,
            int vit,
            int luk)
        {
            var maxHp = 80 + vit * 6;
            return new CharacterState
            {
                CharacterId = id,
                DisplayName = displayName,
                Role = role,
                Level = 1,
                Str = str,
                Agi = agi,
                Int = intel,
                Vit = vit,
                Luk = luk,
                MaxHp = maxHp,
                CurrentHp = maxHp,
                PersonalityTags = { personality }
            };
        }

        public readonly struct MonsterDefinition
        {
            public readonly string Id;
            public readonly string DisplayName;
            public readonly MonsterRarity Rarity;
            public readonly int Hp;
            public readonly int Attack;
            public readonly int Defense;
            public readonly int GoldReward;

            public MonsterDefinition(
                string id,
                string displayName,
                MonsterRarity rarity,
                int hp,
                int attack,
                int defense,
                int goldReward)
            {
                Id = id;
                DisplayName = displayName;
                Rarity = rarity;
                Hp = hp;
                Attack = attack;
                Defense = defense;
                GoldReward = goldReward;
            }
        }

        public readonly struct DiscoveryDefinition
        {
            public readonly string ItemId;
            public readonly string DisplayName;
            public readonly int Quantity;
            public readonly int GoldValue;

            public DiscoveryDefinition(string itemId, string displayName, int quantity, int goldValue)
            {
                ItemId = itemId;
                DisplayName = displayName;
                Quantity = quantity;
                GoldValue = goldValue;
            }
        }
    }
}
