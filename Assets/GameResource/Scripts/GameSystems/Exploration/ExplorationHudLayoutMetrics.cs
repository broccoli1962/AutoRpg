namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 12_UIUX v2 — 1920×1080 기준 HUD 레이아웃 단일 출처.
    /// </summary>
    public static class ExplorationHudLayoutMetrics
    {
        public const float ReferenceWidth = 1920f;
        public const float ReferenceHeight = 1080f;

        public const float SafeAreaBottomInset = 34f;
        public const float TopBarHeight = 96f;
        public const float HorizontalPadding = 16f;
        public const float ColumnGap = 16f;
        public const float LeftPanelWidth = 288f;
        public const float RightPanelWidth = 400f;
        public const float TabBarHeight = 72f;
        public const float TabBarPadding = 0f;
        public const float PanelInnerPadding = 16f;

        public static float BottomInsetPx => TabBarHeight + TabBarPadding;

        public static float BodyHeight =>
            ReferenceHeight - SafeAreaBottomInset - TopBarHeight - BottomInsetPx;

        public static float CenterPanelLeft =>
            HorizontalPadding + LeftPanelWidth + ColumnGap;

        public static float CenterPanelWidth =>
            ReferenceWidth - CenterPanelLeft - RightPanelWidth - HorizontalPadding - ColumnGap;

        public static float RightPanelLeft =>
            ReferenceWidth - RightPanelWidth - HorizontalPadding;

        public static float LeftPanelContentWidth => LeftPanelWidth - PanelInnerPadding * 2f;

        public static float CenterPanelContentWidth => CenterPanelWidth - PanelInnerPadding * 2f;

        public static float RightPanelContentWidth => RightPanelWidth - PanelInnerPadding * 2f;

        public const float PartyMemberCardHeight = 128f;
        public const float StartCardWidth = 560f;
        public const float StartCardBannerAspect = 14f / 5f;
        public const float ZoneBannerAspect = 37f / 5f;
        public const float TabIconDisplaySize = 28f;
        public const float LogIconDisplaySize = 24f;
        public const float PortraitDisplaySize = 72f;
        public const float HpBarHeight = 6f;
    }
}
