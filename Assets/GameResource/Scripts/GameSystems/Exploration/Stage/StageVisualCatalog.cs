using Backend.GameSystems.Exploration.Data;
using Backend.Object.Management;

namespace Backend.GameSystems.Exploration.Stage
{
    /// <summary>
    /// 몬스터·파티 → Addressable 스프라이트 키 매핑 (GSSL id + rarity 기반).
    /// </summary>
    public static class StageVisualCatalog
    {
        public const string PartyWarrior = "stage_party_warrior";
        public const string PartyRogue = "stage_party_rogue";
        public const string PartyMage = "stage_party_mage";
        public const string PartyCleric = "stage_party_cleric";
        public const string PartyBard = "stage_party_bard";

        public const string MonsterSlime = "stage_monster_slime";
        public const string MonsterBeast = "stage_monster_beast";
        public const string MonsterElite = "stage_monster_elite";
        public const string MonsterBoss = "stage_monster_boss";

        public const string VfxSlash = "stage_vfx_slash";

        /// <summary>파티 리더 역할 → 스프라이트 키.</summary>
        public static string ResolvePartySpriteKey(CharacterRole role) =>
            role switch
            {
                CharacterRole.Rogue => PartyRogue,
                CharacterRole.Mage => PartyMage,
                CharacterRole.Cleric => PartyCleric,
                CharacterRole.Bard => PartyBard,
                _ => PartyWarrior
            };

        /// <summary>탐험 이벤트 → 몬스터 스프라이트 키.</summary>
        public static string ResolveMonsterSpriteKey(ExplorationEvent explorationEvent)
        {
            if (explorationEvent?.Combat == null)
                return MonsterBeast;

            var monsterId = explorationEvent.Combat.EnemyGroup != null && explorationEvent.Combat.EnemyGroup.Count > 0
                ? explorationEvent.Combat.EnemyGroup[0]
                : null;

            MonsterRarity rarity = MonsterRarity.Common;
            if (!string.IsNullOrEmpty(monsterId) &&
                TableManager.TryGetMonsterData(monsterId, out var row))
            {
                rarity = ParseRarity(row.rarity);
                return ResolveMonsterSpriteKey(monsterId, rarity);
            }

            foreach (var monster in ZoneDefinitions.GetMonsters(explorationEvent.ZoneId))
            {
                if (monster.Id != monsterId)
                    continue;

                return ResolveMonsterSpriteKey(monster.Id, monster.Rarity);
            }

            return ResolveMonsterSpriteKey(monsterId, ResolveFallbackRarity(explorationEvent));
        }

        /// <summary>몬스터 id·희귀도 → 스프라이트 키.</summary>
        public static string ResolveMonsterSpriteKey(string monsterId, MonsterRarity rarity)
        {
            if (rarity == MonsterRarity.Boss)
                return MonsterBoss;

            if (rarity == MonsterRarity.Rare)
                return MonsterElite;

            if (!string.IsNullOrEmpty(monsterId))
            {
                var id = monsterId.ToLowerInvariant();
                if (id.Contains("slime") || id.Contains("wisp") || id.Contains("spore"))
                    return MonsterSlime;
                if (id.Contains("bat") || id.Contains("rat") || id.Contains("serpent") || id.Contains("bear"))
                    return MonsterBeast;
            }

            return rarity == MonsterRarity.Notable ? MonsterElite : MonsterBeast;
        }

        private static MonsterRarity ResolveFallbackRarity(ExplorationEvent explorationEvent) =>
            explorationEvent.Salience switch
            {
                SalienceGrade.Milestone => MonsterRarity.Boss,
                SalienceGrade.Significant => MonsterRarity.Rare,
                SalienceGrade.Notable => MonsterRarity.Notable,
                _ => MonsterRarity.Common
            };

        private static MonsterRarity ParseRarity(string value) =>
            System.Enum.TryParse(value, out MonsterRarity rarity) ? rarity : MonsterRarity.Common;
    }
}
