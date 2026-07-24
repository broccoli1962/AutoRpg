namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 클래시 로얄식 세로 밴드 HUD 메트릭 (1080×1920). Prefab bake 기준값.
    /// 런타임 sizeDelta/fontSize 강제에 쓰지 말고 Prefab을 진실 공급원으로 둔다.
    /// </summary>
    public static class ExplorationHudLayoutMetrics
    {
        public const float ReferenceWidth = 1080f;
        public const float ReferenceHeight = 1920f;

        public const float HorizontalPadding = 16f;
        public const float PanelInnerPadding = 16f;
        public const float SectionGap = 12f;
        public const float CardGap = 10f;

        /// <summary>Band A — 상단 재화/액션.</summary>
        public const float TopBarHeight = 120f;

        /// <summary>Band E — 하단 탭 (터치 여유 포함).</summary>
        public const float TabBarHeight = 140f;

        public const float PartyRowHeight = 120f;
        /// <summary>탐험 중 파티 미니 행.</summary>
        public const float PartyRowCompactHeight = 88f;

        /// <summary>Band D — 로그 스트립.</summary>
        public const float LogStripHeight = 140f;
        public const float LogPanelHeight = LogStripHeight;
        public const int LogStripMaxVisibleLines = 4;
        public const int LogStripBodyFontSize = 28;

        /// <summary>Band B — 스테이지 아레나 최소 높이.</summary>
        public const float CenterPanelMinHeight = 1200f;

        public const float StageActorPartyWidth = 120f;
        public const float StageActorPartyHeight = 160f;
        public const float StageActorMonsterWidth = 136f;
        public const float StageActorMonsterHeight = 176f;
        public const float StageGroundInset = 64f;

        public const int PartyMemberCount = 4;

        public const float StartCardBannerAspect = 2.35f;
        public const float ZoneBannerAspect = 2.75f;
        public const float TabIconDisplaySize = 48f;
        public const float LogIconDisplaySize = 28f;
        public const float PortraitDisplaySize = 52f;
        public const float HpBarHeight = 10f;

        public const float ActionButtonWidth = 168f;
        public const float ActionButtonHeight = 48f;
        public const float ActionButtonWidthRatio = 2f;
        public const float ActionButtonHeightRatio = 1f;
        public const float StartCardButtonWidth = 360f;
        public const float StartCardButtonHeight = 88f;
        public const float TabItemPreferredHeight = 96f;

        public const int TopBarTitleFontSize = 28;
        public const int TopBarBodyFontSize = 24;
        public const int TopBarMutedFontSize = 20;
        public const int PartyNameFontSize = 24;
        public const int PartyRoleFontSize = 20;
        public const int PartyDetailFontSize = 18;
        public const int TabLabelFontSize = 22;
        public const int ActionButtonFontSize = 26;
        public const int LogHeaderFontSize = 26;
        public const int LogBodyFontSize = 28;
        public const int LogEmptyFontSize = 24;
        public const float LogItemMinHeight = 52f;

        public const float ProgressBarHeight = 16f;
        public const int ExploreFloorFontSize = 24;
        public const int ExploreProgressLabelFontSize = 22;
        public const int ExploreStatusFontSize = 22;

        public static float ActionRowHeight => ActionButtonHeight;

        public static float TopBarStatusRowBottomInset => ActionRowHeight;

        public static float BottomInsetPx => TabBarHeight;

        public static float BodyHeight => ReferenceHeight - TopBarHeight - BottomInsetPx;

        public static float ContentWidth => ReferenceWidth - HorizontalPadding * 2f;

        public static float PartyMemberCardWidth =>
            (ContentWidth - PanelInnerPadding * 2f - CardGap * (PartyMemberCount - 1)) / PartyMemberCount;

        public static float PartyMemberCardHeight => PartyRowCompactHeight - PanelInnerPadding;

        public static float StartCardWidth => ContentWidth - PanelInnerPadding * 2f;

        /// <summary>오버레이 카드 폭 (~92%).</summary>
        public static float OverlayPanelWidth => ContentWidth * 0.92f;

        /// <summary>TopBar~TabBar 사이 카드 높이 (여백 포함).</summary>
        public static float OverlayPanelHeightLarge => BodyHeight - 48f;

        public static float OverlayPanelHeightMedium => BodyHeight - 120f;

        public static float OverlayContentWidth => OverlayPanelWidth - PanelInnerPadding * 2f;

        public const int OverlayBodyFontSize = 28;
        public const int OverlayTitleFontSize = 36;
        public const int OverlayActionButtonFontSize = 28;
        public const float OverlayActionButtonHeight = 88f;
    }
}
