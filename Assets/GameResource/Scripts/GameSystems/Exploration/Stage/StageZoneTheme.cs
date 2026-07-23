using Backend.GameSystems.Exploration.Data;
using UnityEngine;

namespace Backend.GameSystems.Exploration.Stage
{
    /// <summary>
    /// 구역별 스테이지 배경·바닥·패럴랙스 틴트 (Phase 6 — Zone 1~6).
    /// </summary>
    public readonly struct StageZoneTheme
    {
        public StageZoneTheme(
            Color background,
            Color parallax,
            Color ground,
            Color parallaxBannerTint,
            string zoneLabel)
        {
            Background = background;
            Parallax = parallax;
            Ground = ground;
            ParallaxBannerTint = parallaxBannerTint;
            ZoneLabel = zoneLabel;
        }

        public Color Background { get; }
        public Color Parallax { get; }
        public Color Ground { get; }
        public Color ParallaxBannerTint { get; }
        public string ZoneLabel { get; }

        public static StageZoneTheme Resolve(string zoneId)
        {
            if (zoneId == ZoneDefinitions.FungalMazeId)
            {
                return new StageZoneTheme(
                    new Color(0.1f, 0.09f, 0.14f, 0.94f),
                    new Color(0.34f, 0.2f, 0.42f, 0.55f),
                    new Color(0.28f, 0.22f, 0.32f, 1f),
                    new Color(0.78f, 0.55f, 0.95f, 0.72f),
                    ZoneDefinitions.FungalMazeDisplayName);
            }

            if (zoneId == ZoneDefinitions.CrystalCavernId)
            {
                return new StageZoneTheme(
                    new Color(0.06f, 0.1f, 0.18f, 0.94f),
                    new Color(0.22f, 0.42f, 0.62f, 0.55f),
                    new Color(0.18f, 0.34f, 0.48f, 1f),
                    new Color(0.55f, 0.85f, 1f, 0.68f),
                    ZoneDefinitions.CrystalCavernDisplayName);
            }

            if (zoneId == ZoneDefinitions.MoltenDepthsId)
            {
                return new StageZoneTheme(
                    new Color(0.16f, 0.06f, 0.05f, 0.94f),
                    new Color(0.48f, 0.18f, 0.08f, 0.55f),
                    new Color(0.42f, 0.2f, 0.1f, 1f),
                    new Color(1f, 0.55f, 0.28f, 0.66f),
                    ZoneDefinitions.MoltenDepthsDisplayName);
            }

            if (zoneId == ZoneDefinitions.SilentRuinsId)
            {
                return new StageZoneTheme(
                    new Color(0.08f, 0.08f, 0.1f, 0.94f),
                    new Color(0.24f, 0.24f, 0.3f, 0.55f),
                    new Color(0.22f, 0.22f, 0.26f, 1f),
                    new Color(0.72f, 0.72f, 0.82f, 0.62f),
                    ZoneDefinitions.SilentRuinsDisplayName);
            }

            if (zoneId == ZoneDefinitions.AbyssalThresholdId)
            {
                return new StageZoneTheme(
                    new Color(0.04f, 0.03f, 0.08f, 0.96f),
                    new Color(0.16f, 0.08f, 0.24f, 0.6f),
                    new Color(0.12f, 0.08f, 0.18f, 1f),
                    new Color(0.62f, 0.35f, 0.95f, 0.58f),
                    ZoneDefinitions.AbyssalThresholdDisplayName);
            }

            return new StageZoneTheme(
                new Color(0.08f, 0.12f, 0.1f, 0.92f),
                new Color(0.12f, 0.18f, 0.16f, 0.55f),
                new Color(0.22f, 0.32f, 0.24f, 1f),
                new Color(0.55f, 0.82f, 0.62f, 0.7f),
                ZoneDefinitions.MossyHollowDisplayName);
        }
    }
}
