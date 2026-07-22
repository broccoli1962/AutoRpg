namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 12_UIUX 3-panel HUD 공통 레이아웃 수치. 모바일 safe area·컴팩트 폭을 반영한다.
    /// </summary>
    public static class ExplorationHudLayoutMetrics
    {
        public const float TopBarHeight = 168f;
        public const float HorizontalPadding = 12f;
        public const float ColumnGap = 8f;

        public static float SafeAreaTopInset =>
            UnityEngine.Screen.height - UnityEngine.Screen.safeArea.yMax;

        public static float SafeAreaBottomInset =>
            UnityEngine.Screen.safeArea.yMin;

        public static bool IsCompactLayout =>
            UnityEngine.Screen.width < 900f;

        public static float LeftPanelWidth =>
            IsCompactLayout ? 220f : 280f;

        public static float RightPanelWidth =>
            IsCompactLayout ? 280f : 400f;

        public static float ContentWidth => UnityEngine.Screen.width;

        public static float EffectiveTopBarHeight =>
            TopBarHeight + SafeAreaTopInset;

        public static float BottomInsetPx =>
            GuildHudTabController.BottomInsetPx + SafeAreaBottomInset;

        public static float CenterPanelLeft =>
            HorizontalPadding + LeftPanelWidth + ColumnGap;

        public static float CenterPanelWidth =>
            UnityEngine.Mathf.Max(
                200f,
                UnityEngine.Screen.width - CenterPanelLeft - RightPanelWidth - HorizontalPadding * 2f - ColumnGap);

        public static float RightPanelLeft =>
            UnityEngine.Screen.width - RightPanelWidth - HorizontalPadding;
    }
}
