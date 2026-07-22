namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 모바일 세로(1080×1920) HUD 레이아웃 단일 출처.
    /// </summary>
    public static class ExplorationHudLayoutMetrics
    {
        public const float ReferenceWidth = 1080f;
        public const float ReferenceHeight = 1920f;

        public const float TopBarHeight = 120f;
        public const float HorizontalPadding = 12f;
        public const float SectionGap = 8f;
        public const float PartyRowHeight = 108f;
        public const float LogPanelHeight = 240f;
        public const float TabBarHeight = 96f;
        public const float PanelInnerPadding = 12f;

        public static float BottomInsetPx => TabBarHeight;

        public static float BodyHeight => ReferenceHeight - TopBarHeight - BottomInsetPx;

        public static float ContentWidth => ReferenceWidth - HorizontalPadding * 2f;

        public const float PartyMemberCardWidth = 280f;
        public const float PartyMemberCardHeight = 96f;
        public const float StartCardWidth = 960f;
        public const float StartCardBannerAspect = 16f / 9f;
        public const float ZoneBannerAspect = 16f / 9f;
        public const float TabIconDisplaySize = 44f;
        public const float LogIconDisplaySize = 24f;
        public const float PortraitDisplaySize = 64f;
        public const float HpBarHeight = 6f;
    }
}
