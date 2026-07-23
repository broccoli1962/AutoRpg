using Backend.GameSystems.Exploration.Data;
using UnityEngine;

namespace Backend.GameSystems.Exploration.Stage
{
    /// <summary>
    /// 몬스터 희귀도·구역·테이블 id → 스테이지 스프라이트·스케일·틴트.
    /// </summary>
    public readonly struct StageMonsterVisual
    {
        public StageMonsterVisual(Color bodyColor, float scale, bool showEliteRing, string spriteKey)
        {
            BodyColor = bodyColor;
            Scale = scale;
            ShowEliteRing = showEliteRing;
            SpriteKey = spriteKey;
        }

        public Color BodyColor { get; }
        public float Scale { get; }
        public bool ShowEliteRing { get; }
        public string SpriteKey { get; }

        public static StageMonsterVisual Resolve(ExplorationEvent explorationEvent)
        {
            if (explorationEvent?.Combat == null)
                return Default();

            var rarity = ResolveRarity(explorationEvent);
            var zoneTint = StageZoneTheme.Resolve(explorationEvent.ZoneId).ParallaxBannerTint;
            var bodyColor = BlendBodyColor(zoneTint, rarity);
            var scale = rarity switch
            {
                MonsterRarity.Boss => 1.55f,
                MonsterRarity.Rare => 1.28f,
                MonsterRarity.Notable => 1.12f,
                _ => 1f
            };

            if (scale <= 1f && explorationEvent.Salience >= SalienceGrade.Significant)
                scale = 1.08f;

            var spriteKey = StageVisualCatalog.ResolveMonsterSpriteKey(explorationEvent);
            return new StageMonsterVisual(bodyColor, scale, rarity >= MonsterRarity.Rare, spriteKey);
        }

        public static StageMonsterVisual Default() =>
            new(new Color(0.86f, 0.32f, 0.28f, 1f), 1f, false, StageVisualCatalog.MonsterBeast);

        private static MonsterRarity ResolveRarity(ExplorationEvent explorationEvent)
        {
            var combat = explorationEvent.Combat;
            if (combat.EnemyGroup != null && combat.EnemyGroup.Count > 0)
            {
                var monsterId = combat.EnemyGroup[0];
                foreach (var monster in ZoneDefinitions.GetMonsters(explorationEvent.ZoneId))
                {
                    if (monster.Id == monsterId)
                        return monster.Rarity;
                }
            }

            return explorationEvent.Salience switch
            {
                SalienceGrade.Milestone => MonsterRarity.Boss,
                SalienceGrade.Significant => MonsterRarity.Rare,
                SalienceGrade.Notable => MonsterRarity.Notable,
                _ => MonsterRarity.Common
            };
        }

        private static Color BlendBodyColor(Color zoneTint, MonsterRarity rarity)
        {
            var baseColor = rarity switch
            {
                MonsterRarity.Boss => new Color(0.72f, 0.18f, 0.82f, 1f),
                MonsterRarity.Rare => new Color(0.92f, 0.42f, 0.22f, 1f),
                MonsterRarity.Notable => new Color(0.9f, 0.55f, 0.28f, 1f),
                _ => new Color(0.86f, 0.32f, 0.28f, 1f)
            };

            return Color.Lerp(baseColor, zoneTint, 0.22f);
        }
    }
}
