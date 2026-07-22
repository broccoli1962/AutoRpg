namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 12_UIUX 3-panel HUD 공통 레이아웃 수치.
    /// </summary>
    public static class ExplorationHudLayoutMetrics
    {
        public const float TopBarHeight = 168f;
        public const float LeftPanelWidth = 280f;
        public const float RightPanelWidth = 400f;
        public const float HorizontalPadding = 12f;
        public const float ColumnGap = 8f;

        public static float BottomInsetPx => GuildHudTabController.BottomInsetPx;

        public static float CenterPanelLeft =>
            HorizontalPadding + LeftPanelWidth + ColumnGap;

        public static float CenterPanelWidth =>
            UnityEngine.Mathf.Max(
                240f,
                UnityEngine.Screen.width - CenterPanelLeft - RightPanelWidth - HorizontalPadding * 2f - ColumnGap);

        public static float RightPanelLeft =>
            UnityEngine.Screen.width - RightPanelWidth - HorizontalPadding;
    }
}
