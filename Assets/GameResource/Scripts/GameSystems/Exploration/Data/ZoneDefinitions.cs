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

        public const string CrystalCavernId = "crystal_cavern";
        public const string CrystalCavernDisplayName = "수정 공동";
        public const int CrystalCavernMinFloor = 31;
        public const int CrystalCavernMaxFloor = 45;

        public const string MoltenDepthsId = "molten_depths";
        public const string MoltenDepthsDisplayName = "용암 지대";
        public const int MoltenDepthsMinFloor = 46;
        public const int MoltenDepthsMaxFloor = 60;

        public const string SilentRuinsId = "silent_ruins";
        public const string SilentRuinsDisplayName = "침묵의 폐허";
        public const int SilentRuinsMinFloor = 61;
        public const int SilentRuinsMaxFloor = 75;

        public const string AbyssalThresholdId = "abyssal_threshold";
        public const string AbyssalThresholdDisplayName = "심연의 문턱";
        public const int AbyssalThresholdMinFloor = 76;
        public const int AbyssalThresholdMaxFloor = 90;

        public const int EndlessSegmentFloorCount = 15;
        public const float EndlessSegmentRewardBoost = 0.15f;
        public const float EndlessSegmentRiskBoost = 0.18f;
        public const float EndlessMonsterStatBoostPerSegment = 0.12f;

        public const float BaseProgressPerTick = 4.5f;
        public const float FloorDifficultyStep = 0.07f;
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
                riskMultiplier: 1.4f),
            new(
                CrystalCavernId,
                CrystalCavernDisplayName,
                CrystalCavernMinFloor,
                CrystalCavernMaxFloor,
                rewardMultiplier: 1.8f,
                riskMultiplier: 1.9f),
            new(
                MoltenDepthsId,
                MoltenDepthsDisplayName,
                MoltenDepthsMinFloor,
                MoltenDepthsMaxFloor,
                rewardMultiplier: 2.5f,
                riskMultiplier: 2.8f),
            new(
                SilentRuinsId,
                SilentRuinsDisplayName,
                SilentRuinsMinFloor,
                SilentRuinsMaxFloor,
                rewardMultiplier: 3.5f,
                riskMultiplier: 3.8f),
            new(
                AbyssalThresholdId,
                AbyssalThresholdDisplayName,
                AbyssalThresholdMinFloor,
                AbyssalThresholdMaxFloor,
                rewardMultiplier: 4.5f,
                riskMultiplier: 5.0f)
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

        private static readonly MonsterDefinition[] CrystalCavernMonsters =
        {
            new("crystal_wisp", "수정 정령", MonsterRarity.Common, 20, 6, 9, 14),
            new("radiant_moth_swarm", "광휘 나비떼", MonsterRarity.Notable, 32, 11, 14, 26),
            new("mana_serpent", "마나 뱀", MonsterRarity.Rare, 52, 18, 16, 42),
            new("crystal_golem", "결정 골렘", MonsterRarity.Rare, 62, 20, 22, 50),
            new("prism_sovereign", "프리즘 군주", MonsterRarity.Boss, 110, 28, 30, 90)
        };

        private static readonly MonsterDefinition[] MoltenDepthsMonsters =
        {
            new("magma_slime", "용암 슬라임", MonsterRarity.Common, 24, 8, 12, 18),
            new("flame_serpent", "화염 뱀", MonsterRarity.Notable, 38, 14, 16, 32),
            new("ash_wraith", "잿더미 악령", MonsterRarity.Rare, 58, 20, 18, 48),
            new("magma_golem", "마그마 골렘", MonsterRarity.Rare, 72, 24, 24, 58),
            new("inferno_tyrant", "지옥불 폭군", MonsterRarity.Boss, 125, 32, 28, 110)
        };

        private static readonly MonsterDefinition[] SilentRuinsMonsters =
        {
            new("ruin_shade", "폐허 망령", MonsterRarity.Common, 28, 9, 14, 22),
            new("ghost_priest", "망령 사제", MonsterRarity.Notable, 44, 16, 20, 38),
            new("cursed_statue", "저주받은 석상", MonsterRarity.Rare, 68, 22, 26, 55),
            new("ancient_warden", "고대 골렘 수호자", MonsterRarity.Rare, 78, 26, 28, 65),
            new("silent_archon", "침묵의 집행자", MonsterRarity.Boss, 140, 34, 32, 130)
        };

        private static readonly MonsterDefinition[] AbyssalThresholdMonsters =
        {
            new("abyss_leech", "심연 흡혈충", MonsterRarity.Common, 32, 12, 16, 28),
            new("shadow_twin", "그림자 분신", MonsterRarity.Notable, 50, 18, 22, 45),
            new("void_stalker", "공허 추적자", MonsterRarity.Rare, 82, 28, 26, 72),
            new("threshold_keeper", "문지기", MonsterRarity.Rare, 95, 30, 30, 85),
            new("abyss_predator", "심연 포식자", MonsterRarity.Boss, 160, 38, 34, 150)
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

        private static readonly string[] CrystalCavernMoveDescriptionIds =
        {
            "prismatic_glow",
            "mana_surge",
            "crystal_echo",
            "refracted_path",
            "humming_resonance"
        };

        private static readonly string[] MoltenDepthsMoveDescriptionIds =
        {
            "heat_shimmer",
            "lava_crackle",
            "sulfur_breath",
            "molten_fissure",
            "ember_rain"
        };

        private static readonly string[] SilentRuinsMoveDescriptionIds =
        {
            "fallen_pillar",
            "whispering_ruins",
            "dust_covered_path",
            "ancient_inscription",
            "echoing_hall"
        };

        private static readonly string[] AbyssalThresholdMoveDescriptionIds =
        {
            "lightless_depth",
            "distorted_echo",
            "gravity_shift",
            "void_whisper",
            "threshold_glimmer"
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

        private static readonly DiscoveryDefinition[] CrystalCavernDiscoveries =
        {
            new("pure_mana_shard", "순수 마나결정", 2, 20),
            new("crystal_lens", "수정 렌즈", 1, 28),
            new("resonance_stone", "공명석", 1, 35),
            new("prismatic_dust", "프리즘 가루", 1, 18)
        };

        private static readonly DiscoveryDefinition[] MoltenDepthsDiscoveries =
        {
            new("obsidian_shard", "흑요석 파편", 1, 32),
            new("magma_core", "용암 핵", 1, 40),
            new("relic_fragment", "유물조각", 1, 25),
            new("heat_warped_coin", "열에 휜 주화", 1, 28)
        };

        private static readonly DiscoveryDefinition[] SilentRuinsDiscoveries =
        {
            new("relic_fragment", "유물조각", 2, 30),
            new("ancient_tablet", "고대 서판", 1, 45),
            new("warden_seal", "수호자 인장", 1, 38),
            new("silent_chalice", "침묵의 성배", 1, 42)
        };

        private static readonly DiscoveryDefinition[] AbyssalThresholdDiscoveries =
        {
            new("relic_fragment", "심연 유물조각", 2, 50),
            new("void_crystal", "공허 결정", 1, 55),
            new("threshold_key", "문턱의 열쇠", 1, 60),
            new("abyss_lore_fragment", "심연 로어 조각", 1, 35)
        };

        public static List<string> CreateAllZoneIdList()
        {
            var list = new List<string>(AllZones.Length);
            foreach (var zone in AllZones)
                list.Add(zone.Id);

            return list;
        }

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

        public static bool IsEndlessZone(string zoneId) => zoneId == AbyssalThresholdId;

        public static int GetEndlessSegmentIndex(int absoluteFloor)
        {
            if (absoluteFloor <= AbyssalThresholdMaxFloor)
                return 0;

            return (absoluteFloor - AbyssalThresholdMaxFloor - 1) / EndlessSegmentFloorCount + 1;
        }

        public static bool TryExtendEndlessZone(ExplorationState state)
        {
            if (state == null || !IsEndlessZone(state.ZoneId))
                return false;

            if (state.CurrentFloor <= state.MaxFloor)
                return false;

            state.MaxFloor += EndlessSegmentFloorCount;
            return true;
        }

        public static float GetEndlessScaleMultiplier(int segmentIndex, float perSegmentBoost)
        {
            if (segmentIndex <= 0)
                return 1f;

            return 1f + segmentIndex * perSegmentBoost;
        }

        public static float GetRewardMultiplier(string zoneId, int floor = 0)
        {
            var multiplier = TryFindZone(zoneId, out var zone) ? zone.RewardMultiplier : 1f;
            if (floor > 0 && IsEndlessZone(zoneId))
            {
                var segment = GetEndlessSegmentIndex(floor);
                multiplier *= GetEndlessScaleMultiplier(segment, EndlessSegmentRewardBoost);
            }

            return multiplier;
        }

        public static float GetRiskMultiplier(string zoneId, int floor = 0)
        {
            var multiplier = TryFindZone(zoneId, out var zone) ? zone.RiskMultiplier : 1f;
            if (floor > 0 && IsEndlessZone(zoneId))
            {
                var segment = GetEndlessSegmentIndex(floor);
                multiplier *= GetEndlessScaleMultiplier(segment, EndlessSegmentRiskBoost);
            }

            return multiplier;
        }

        public static MonsterDefinition ScaleMonsterForFloor(MonsterDefinition monster, string zoneId, int floor)
        {
            var segment = GetEndlessSegmentIndex(floor);
            if (segment <= 0 || !IsEndlessZone(zoneId))
                return monster;

            var scale = GetEndlessScaleMultiplier(segment, EndlessMonsterStatBoostPerSegment);
            return new MonsterDefinition(
                monster.Id,
                monster.DisplayName,
                monster.Rarity,
                System.Math.Max(1, (int)(monster.Hp * scale)),
                System.Math.Max(1, (int)(monster.Attack * scale)),
                System.Math.Max(1, (int)(monster.Defense * scale)),
                System.Math.Max(1, (int)(monster.GoldReward * scale)));
        }

        public static bool TryAdvanceZone(ExplorationState state, out string completedZoneId)
        {
            completedZoneId = null;
            if (state == null)
                return false;

            for (var i = 0; i < AllZones.Length - 1; i++)
            {
                if (AllZones[i].Id != state.ZoneId)
                    continue;

                completedZoneId = AllZones[i].Id;
                var next = AllZones[i + 1];
                state.ZoneId = next.Id;
                state.MaxFloor = next.MaxFloor;
                return true;
            }

            return false;
        }

        public static bool TryAdvanceZone(ExplorationState state) =>
            TryAdvanceZone(state, out _);

        public const string RelicFragmentItemId = "relic_fragment";

        public static bool IsRelicFragmentItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
                return false;

            return itemId == RelicFragmentItemId || itemId.StartsWith("relic_");
        }

        public static int GetRelicFragmentQuantity(DiscoveryDefinition discovery)
        {
            if (string.IsNullOrEmpty(discovery.ItemId) || !IsRelicFragmentItem(discovery.ItemId))
                return 0;

            return discovery.Quantity > 0 ? discovery.Quantity : 1;
        }

        public static int GetZoneClearReputationBonus(string zoneId) =>
            2 + GetZoneIndex(zoneId) * 2;

        public static int GetZoneIndex(string zoneId)
        {
            for (var i = 0; i < AllZones.Length; i++)
            {
                if (AllZones[i].Id == zoneId)
                    return i;
            }

            return 0;
        }

        public static int GetZoneCompleteLegacyBonus(string zoneId)
        {
            return 6 + GetZoneIndex(zoneId) * 4;
        }

        public static int GetManaShardQuantity(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
                return 0;

            return itemId.Contains("mana_shard") ? 1 : 0;
        }

        public static int GetDiscoveryManaShards(DiscoveryDefinition discovery)
        {
            if (string.IsNullOrEmpty(discovery.ItemId))
                return 0;

            if (!discovery.ItemId.Contains("mana_shard"))
                return 0;

            return discovery.Quantity > 0 ? discovery.Quantity : 1;
        }

        public static float GetFloorDifficulty(string zoneId, int floor)
        {
            var relativeFloor = GetZoneRelativeFloor(zoneId, floor);
            var risk = GetRiskMultiplier(zoneId, floor);
            return (1f + (relativeFloor - 1) * FloorDifficultyStep) * risk;
        }

        public static IReadOnlyList<MonsterDefinition> GetMonsters(string zoneId)
        {
            return zoneId switch
            {
                FungalMazeId => FungalMazeMonsters,
                CrystalCavernId => CrystalCavernMonsters,
                MoltenDepthsId => MoltenDepthsMonsters,
                SilentRuinsId => SilentRuinsMonsters,
                AbyssalThresholdId => AbyssalThresholdMonsters,
                _ => MossyHollowMonsters
            };
        }

        public static IReadOnlyList<string> GetMoveDescriptionIds(string zoneId)
        {
            return zoneId switch
            {
                FungalMazeId => FungalMazeMoveDescriptionIds,
                CrystalCavernId => CrystalCavernMoveDescriptionIds,
                MoltenDepthsId => MoltenDepthsMoveDescriptionIds,
                SilentRuinsId => SilentRuinsMoveDescriptionIds,
                AbyssalThresholdId => AbyssalThresholdMoveDescriptionIds,
                _ => MossyHollowMoveDescriptionIds
            };
        }

        public static IReadOnlyList<string> GetMoveDescriptionIds() => GetMoveDescriptionIds(MossyHollowId);

        public static IReadOnlyList<DiscoveryDefinition> GetDiscoveries(string zoneId)
        {
            return zoneId switch
            {
                FungalMazeId => FungalMazeDiscoveries,
                CrystalCavernId => CrystalCavernDiscoveries,
                MoltenDepthsId => MoltenDepthsDiscoveries,
                SilentRuinsId => SilentRuinsDiscoveries,
                AbyssalThresholdId => AbyssalThresholdDiscoveries,
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
                    CreateCharacter("char_kyle", "카일", CharacterRole.Mage, PersonalityTag.Greedy, 8, 10, 14, 9, 10),
                    CreateCharacter("char_sora", "소라", CharacterRole.Bard, PersonalityTag.Cheerful, 9, 11, 12, 8, 12),
                    CreateCharacter("char_elena", "엘레나", CharacterRole.Cleric, PersonalityTag.Loyal, 10, 9, 13, 11, 7)
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
