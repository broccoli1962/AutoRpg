namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 모바일 세로(1080×1920) HUD 레이아웃 단일 출처. 16px 그리드·콘텐츠 폭 기준 비율.
    /// </summary>
    public static class ExplorationHudLayoutMetrics
    {
        public const float ReferenceWidth = 1080f;
        public const float ReferenceHeight = 1920f;

        public const float HorizontalPadding = 16f;
        public const float PanelInnerPadding = 16f;
        public const float SectionGap = 12f;
        public const float CardGap = 10f;

        public const float TopBarHeight = 168f;
        public const float TabBarHeight = 106f;
        public const float PartyRowHeight = 148f;
        /// <summary>탐험 중 스테이지 확대를 위해 파티 행을 축소.</summary>
        public const float PartyRowCompactHeight = 96f;
        /// <summary>탐험 중 하단 로그 스트립 (스테이지 우선 레이아웃).</summary>
        public const float LogStripHeight = 160f;
        public const float LogPanelHeight = LogStripHeight;
        public const int LogStripMaxVisibleLines = 5;
        public const int LogStripBodyFontSize = 22;

        public const float CenterPanelMinHeight = 960f;

        public const float StageActorPartyWidth = 96f;
        public const float StageActorPartyHeight = 128f;
        public const float StageActorMonsterWidth = 112f;
        public const float StageActorMonsterHeight = 144f;
        public const float StageGroundInset = 52f;

        public const int PartyMemberCount = 4;

        public const float StartCardBannerAspect = 2.35f;
        public const float ZoneBannerAspect = 2.75f;
        public const float TabIconDisplaySize = 46f;
        public const float LogIconDisplaySize = 28f;
        public const float PortraitDisplaySize = 52f;
        public const float HpBarHeight = 10f;

        /// <summary>상단바 일시정지·귀환 등 — 가로로 길고 세로는 낮게.</summary>
        public const float ActionButtonWidth = 172f;
        public const float ActionButtonHeight = 44f;
        public const float ActionButtonWidthRatio = 2f;
        public const float ActionButtonHeightRatio = 1f;
        public const float StartCardButtonWidth = 320f;
        public const float StartCardButtonHeight = 64f;
        public const float TabItemPreferredHeight = 74f;

        public const int TopBarTitleFontSize = 24;
        public const int TopBarBodyFontSize = 20;
        public const int TopBarMutedFontSize = 18;
        public const int PartyNameFontSize = 22;
        public const int PartyRoleFontSize = 18;
        public const int PartyDetailFontSize = 16;
        public const int TabLabelFontSize = 23;
        public const int ActionButtonFontSize = 27;
        public const int LogHeaderFontSize = 26;
        public const int LogBodyFontSize = 26;
        public const int LogEmptyFontSize = 23;
        public const float LogItemMinHeight = 48f;

        public const float ProgressBarHeight = 16f;
        public const int ExploreFloorFontSize = 22;
        public const int ExploreProgressLabelFontSize = 20;
        public const int ExploreStatusFontSize = 20;

        public static float ActionRowHeight => ActionButtonHeight;

        public static float TopBarStatusRowBottomInset => ActionRowHeight;

        public static float BottomInsetPx => TabBarHeight;

        public static float BodyHeight => ReferenceHeight - TopBarHeight - BottomInsetPx;

        public static float ContentWidth => ReferenceWidth - HorizontalPadding * 2f;

        /// <summary>파티 카드 4장이 가로 스크롤 없이 콘텐츠 폭에 맞도록 계산.</summary>
        public static float PartyMemberCardWidth =>
            (ContentWidth - PanelInnerPadding * 2f - CardGap * (PartyMemberCount - 1)) / PartyMemberCount;

        public static float PartyMemberCardHeight => PartyRowHeight - PanelInnerPadding * 2f;

        public static float StartCardWidth => ContentWidth - PanelInnerPadding * 2f;

        public static float OverlayPanelWidth => ContentWidth;

        public static float OverlayPanelHeightLarge => 540f;

        public static float OverlayPanelHeightMedium => 460f;

        public static float OverlayContentWidth => ContentWidth - PanelInnerPadding * 2f;
    }
}
