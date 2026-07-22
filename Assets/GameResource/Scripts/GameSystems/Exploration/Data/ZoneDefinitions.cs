using System.Collections.Generic;

namespace Backend.GameSystems.Exploration.Data
{
    public static class ZoneDefinitions
    {
        public const string MossyHollowId = "mossy_hollow";
        public const string MossyHollowDisplayName = "이끼 동굴";
        public const int MossyHollowMaxFloor = 15;

        public const string FungalMazeId = "fungal_maze";
        public const string FungalMazeDisplayName = "균사 미궁";
        public const int FungalMazeMinFloor = 16;
        public const int FungalMazeMaxFloor = 30;

        public const float BaseProgressPerTick = 4.5f;
        public const float FloorDifficultyStep = 0.08f;
        public const float BaseEventRollChance = 0.35f;

        private static readonly ZoneDefinition[] AllZones =
        {
            new(
                MossyHollowId,
                MossyHollowDisplayName,
                1,
                MossyHollowMaxFloor,
                rewardMultiplier: 1f,
                riskMultiplier: 1f),
            new(
                FungalMazeId,
                FungalMazeDisplayName,
                FungalMazeMinFloor,
                FungalMazeMaxFloor,
                rewardMultiplier: 1.3f,
                riskMultiplier: 1.4f)
        };

        private static readonly MonsterDefinition[] MossyHollowMonsters =
        {
            new("fungus_slime", "곰팡이 슬라임", MonsterRarity.Common, 12, 4, 6, 8),
            new("cave_rat", "동굴 쥐", MonsterRarity.Common, 10, 3, 5, 6),
            new("silver_bat_swarm", "은빛 박쥐떼", MonsterRarity.Notable, 22, 8, 14, 18),
            new("moss_guardian", "이끼 수호자", MonsterRarity.Rare, 40, 14, 24, 35),
            new("hollow_boss", "심연의 이끼 군주", MonsterRarity.Boss, 80, 20, 40, 60)
        };

        private static readonly MonsterDefinition[] FungalMazeMonsters =
        {
            new("spore_wisp", "포자 정령", MonsterRarity.Common, 16, 5, 7, 10),
            new("mycelium_tendril", "균사 촉수", MonsterRarity.Notable, 28, 10, 12, 22),
            new("infected_cave_bear", "감염된 동굴 곰", MonsterRarity.Rare, 48, 16, 18, 40),
            new("spore_matriarch", "포자 여왕", MonsterRarity.Rare, 55, 18, 20, 45),
            new("mycelial_overmind", "균사체 군주", MonsterRarity.Boss, 95, 24, 26, 75)
        };

        private static readonly string[] MossyHollowMoveDescriptionIds =
        {
            "damp_passage",
            "glowing_moss",
            "narrow_tunnel",
            "water_drip",
            "fungal_scent"
        };

        private static readonly string[] FungalMazeMoveDescriptionIds =
        {
            "spore_mist",
            "twisted_mycelium",
            "losing_direction",
            "pulsing_fungus",
            "whispering_spores"
        };

        private static readonly DiscoveryDefinition[] MossyHollowDiscoveries =
        {
            new("mana_shard", "마나결정", 1, 12),
            new("old_coin_pouch", "낡은 은화 주머니", 1, 25),
            new("healing_herb", "치유 이끼", 1, 8),
            new("rusty_ring", "낡은 반지", 1, 15)
        };

        private static readonly DiscoveryDefinition[] FungalMazeDiscoveries =
        {
            new("spore_vial", "포자 시약", 1, 18),
            new("fungal_cap", "발광 버섯갓", 1, 14),
            new("mycelium_thread", "균사 실", 1, 22),
            new("spore_filter_mask", "포자 차단 마스크", 1, 30)
        };

        public static string GetZoneDisplayName(string zoneId)
        {
            return TryFindZone(zoneId, out var zone) ? zone.DisplayName : zoneId;
        }

        public static int GetMinFloor(string zoneId)
        {
            return TryFindZone(zoneId, out var zone) ? zone.MinFloor : 1;
        }

        public static int GetMaxFloor(string zoneId)
        {
            return TryFindZone(zoneId, out var zone) ? zone.MaxFloor : MossyHollowMaxFloor;
        }

        public static int GetZoneRelativeFloor(string zoneId, int absoluteFloor)
        {
            return absoluteFloor - GetMinFloor(zoneId) + 1;
        }

        public static float GetRewardMultiplier(string zoneId)
        {
            return TryFindZone(zoneId, out var zone) ? zone.RewardMultiplier : 1f;
        }

        public static float GetRiskMultiplier(string zoneId)
        {
            return TryFindZone(zoneId, out var zone) ? zone.RiskMultiplier : 1f;
        }

        public static bool TryAdvanceZone(ExplorationState state)
        {
            if (state == null)
                return false;

            for (var i = 0; i < AllZones.Length - 1; i++)
            {
                if (AllZones[i].Id != state.ZoneId)
                    continue;

                var next = AllZones[i + 1];
                state.ZoneId = next.Id;
                state.MaxFloor = next.MaxFloor;
                return true;
            }

            return false;
        }

        public static float GetFloorDifficulty(string zoneId, int floor)
        {
            var relativeFloor = GetZoneRelativeFloor(zoneId, floor);
            var risk = GetRiskMultiplier(zoneId);
            return (1f + (relativeFloor - 1) * FloorDifficultyStep) * risk;
        }

        public static IReadOnlyList<MonsterDefinition> GetMonsters(string zoneId)
        {
            return zoneId switch
            {
                FungalMazeId => FungalMazeMonsters,
                _ => MossyHollowMonsters
            };
        }

        public static IReadOnlyList<string> GetMoveDescriptionIds(string zoneId)
        {
            return zoneId switch
            {
                FungalMazeId => FungalMazeMoveDescriptionIds,
                _ => MossyHollowMoveDescriptionIds
            };
        }

        public static IReadOnlyList<string> GetMoveDescriptionIds() => GetMoveDescriptionIds(MossyHollowId);

        public static IReadOnlyList<DiscoveryDefinition> GetDiscoveries(string zoneId)
        {
            return zoneId switch
            {
                FungalMazeId => FungalMazeDiscoveries,
                _ => MossyHollowDiscoveries
            };
        }

        public static IReadOnlyList<DiscoveryDefinition> GetDiscoveries() => GetDiscoveries(MossyHollowId);

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

        private static bool TryFindZone(string zoneId, out ZoneDefinition zone)
        {
            foreach (var entry in AllZones)
            {
                if (entry.Id == zoneId)
                {
                    zone = entry;
                    return true;
                }
            }

            zone = default;
            return false;
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

        private readonly struct ZoneDefinition
        {
            public readonly string Id;
            public readonly string DisplayName;
            public readonly int MinFloor;
            public readonly int MaxFloor;
            public readonly float RewardMultiplier;
            public readonly float RiskMultiplier;

            public ZoneDefinition(
                string id,
                string displayName,
                int minFloor,
                int maxFloor,
                float rewardMultiplier,
                float riskMultiplier)
            {
                Id = id;
                DisplayName = displayName;
                MinFloor = minFloor;
                MaxFloor = maxFloor;
                RewardMultiplier = rewardMultiplier;
                RiskMultiplier = riskMultiplier;
            }
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
