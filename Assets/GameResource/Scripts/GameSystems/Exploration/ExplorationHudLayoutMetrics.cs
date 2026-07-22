namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 12_UIUX 3-panel HUD 레이아웃. 1920x1080 기준 좌표계.
    /// </summary>
    public static class ExplorationHudLayoutMetrics
    {
        public const float ReferenceWidth = 1920f;
        public const float ReferenceHeight = 1080f;

        public const float TopBarHeight = 120f;
        public const float HorizontalPadding = 16f;
        public const float ColumnGap = 12f;
        public const float LeftPanelWidth = 300f;
        public const float RightPanelWidth = 420f;
        public const float TabBarHeight = 56f;
        public const float TabBarPadding = 8f;

        public static float BottomInsetPx => TabBarHeight + TabBarPadding;

        public static float BodyHeight => ReferenceHeight - TopBarHeight - BottomInsetPx;

        public static float CenterPanelLeft =>
            HorizontalPadding + LeftPanelWidth + ColumnGap;

        public static float CenterPanelWidth =>
            ReferenceWidth - CenterPanelLeft - RightPanelWidth - HorizontalPadding - ColumnGap;

        public static float RightPanelLeft =>
            ReferenceWidth - RightPanelWidth - HorizontalPadding;

        public static float LeftPanelContentWidth => LeftPanelWidth - 24f;

        public static float CenterPanelContentWidth => CenterPanelWidth - 32f;

        public static float RightPanelContentWidth => RightPanelWidth - 24f;

        /// <summary>중앙 패널 하단 초상/상태 영역 예약 높이.</summary>
        public const float CenterFooterHeight = 148f;

        /// <summary>중앙 패널 상단 헤더(구역/진행바) 높이.</summary>
        public const float CenterHeaderHeight = 112f;
    }
}
