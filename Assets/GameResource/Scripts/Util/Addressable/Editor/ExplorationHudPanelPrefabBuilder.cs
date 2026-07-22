#if UNITY_EDITOR

using Backend.GameSystems.DynamicEvent;

using Backend.GameSystems.Exploration;

using Backend.Object.UI;

using Backend.Object.UI.Exploration;

using Backend.Util;

using UnityEditor;

using UnityEngine;

using UnityEngine.UI;



namespace Backend.Editor

{

    /// <summary>

    /// 12_UIUX v2 — LayoutGroup 기반 ExplorationHudPanel Addressable 프리팹 생성.

    /// </summary>

    public static class ExplorationHudPanelPrefabBuilder

    {

        private const string PrefabPath = "Assets/GameResource/Prefabs/UI/ExplorationHudPanel.prefab";

        private const string V2Folder = "Assets/GameResource/Images/GameUI/v2";



        [MenuItem("Tools/Addressables/Build v2 Exploration HUD")]

        public static void BuildV2Hud()
        {
            GameUiV2KitGenerator.GenerateAll();
            BuildPrefab();
        }



        [MenuItem("Tools/Addressables/Build Exploration HUD Panel Prefab")]

        public static void BuildPrefab()

        {

            var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/BMJUA_ttf.ttf")
                ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var root = new GameObject("ExplorationHudPanel", typeof(RectTransform));

            var rootRect = root.GetComponent<RectTransform>();

            StretchFull(rootRect);



            var hudPanel = root.AddComponent<ExplorationHudPanel>();

            AddRuntimeComponents(root);



            var backdrop = CreateRectChild(rootRect, "Backdrop");

            StretchFull(backdrop);

            var backdropImage = backdrop.gameObject.AddComponent<Image>();

            backdropImage.color = ModernUiStyle.AppBackground;

            backdropImage.raycastTarget = false;



            var topBar = CreateTopBar(rootRect, font, out var statusText, out var goldText, out var filterText, out var helpText, out var pauseButton, out var resumeButton, out var returnButton);

            var body = CreateBody(rootRect, font, out var logFeedView, out var logEmptyStateText);

            CreateBottomTabBar(rootRect, font);
            CreateOverlays(root, rootRect, font);

            WireHudPanel(hudPanel, statusText, goldText, pauseButton, resumeButton, returnButton, logFeedView, filterText, helpText);

            WireLogFeedEmptyState(logFeedView, logEmptyStateText);



            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);

            global::UnityEngine.Object.DestroyImmediate(root);

            AssetDatabase.SaveAssets();

            AssetDatabase.Refresh();



            Debug.Log($"[ExplorationHudPanelPrefabBuilder] Saved v2 HUD {PrefabPath}");

        }



        private static RectTransform CreateTopBar(

            RectTransform root,

            Font font,

            out Text statusText,

            out Text goldText,

            out Text filterText,

            out Text helpText,

            out CommonButton pauseButton,

            out CommonButton resumeButton,

            out CommonButton returnButton)

        {

            var topBar = CreateRectChild(root, "TopBar");

            StretchHorizontal(topBar, ExplorationHudLayoutMetrics.TopBarHeight, 0f);

            ApplyV2Sliced(topBar.gameObject.AddComponent<Image>(), "ui_bar_top");



            var statusRow = CreateRectChild(topBar, "StatusRow");

            StretchWithInsets(statusRow, 0f, 48f, 0f, 0f);

            var statusLayout = statusRow.gameObject.AddComponent<HorizontalLayoutGroup>();

            statusLayout.padding = new RectOffset(16, 16, 8, 0);

            statusLayout.spacing = 12f;

            statusLayout.childAlignment = TextAnchor.UpperLeft;

            statusLayout.childControlWidth = true;

            statusLayout.childForceExpandWidth = true;



            statusText = CreateLayoutText(statusRow, "ZoneFloorText", string.Empty, 18, TextAnchor.UpperLeft);

            statusText.supportRichText = true;

            ModernUiStyle.ApplyTitleMedium(statusText);



            goldText = CreateLayoutText(statusRow, "GoldText", string.Empty, 15, TextAnchor.UpperLeft);

            goldText.gameObject.SetActive(false);

            ModernUiStyle.ApplyBody(goldText, 15);



            filterText = CreateLayoutText(statusRow, "FilterText", string.Empty, 13, TextAnchor.UpperLeft);

            ModernUiStyle.ApplyMuted(filterText, 13);



            helpText = CreateLayoutText(statusRow, "HelpText", string.Empty, 11, TextAnchor.UpperLeft);

            helpText.gameObject.SetActive(false);



            var actionRow = CreateRectChild(topBar, "ActionRow");

            actionRow.anchorMin = new Vector2(0f, 0f);

            actionRow.anchorMax = new Vector2(1f, 0f);

            actionRow.pivot = new Vector2(0.5f, 0f);

            actionRow.sizeDelta = new Vector2(0f, 40f);

            var actionLayout = actionRow.gameObject.AddComponent<HorizontalLayoutGroup>();

            actionLayout.padding = new RectOffset(16, 16, 0, 0);

            actionLayout.spacing = 8f;

            actionLayout.childAlignment = TextAnchor.MiddleLeft;

            actionLayout.childControlWidth = false;

            actionLayout.childForceExpandWidth = false;



            pauseButton = CreateLayoutButton(actionRow, "PauseButton", font, "일시정지", false, 112f, 40f);

            resumeButton = CreateLayoutButton(actionRow, "ResumeButton", font, "재개", false, 112f, 40f);

            returnButton = CreateLayoutButton(actionRow, "ReturnButton", font, "귀환", true, 112f, 40f);

            return topBar;

        }



        private static RectTransform CreateBody(RectTransform root, Font font, out ExplorationLogFeedView logFeedView, out Text logEmptyStateText)

        {

            var body = CreateRectChild(root, "Body");

            StretchWithInsets(body, ExplorationHudLayoutMetrics.TopBarHeight, ExplorationHudLayoutMetrics.BottomInsetPx, 0f, 0f);



            var bodyLayout = body.gameObject.AddComponent<VerticalLayoutGroup>();

            bodyLayout.padding = new RectOffset(

                (int)ExplorationHudLayoutMetrics.HorizontalPadding,

                (int)ExplorationHudLayoutMetrics.HorizontalPadding,

                0,

                0);

            bodyLayout.spacing = ExplorationHudLayoutMetrics.SectionGap;

            bodyLayout.childAlignment = TextAnchor.UpperLeft;

            bodyLayout.childControlHeight = true;

            bodyLayout.childForceExpandHeight = false;

            bodyLayout.childControlWidth = true;

            bodyLayout.childForceExpandWidth = true;



            CreateCenterPanel(body, font);

            CreatePartyRow(body, font);

            logFeedView = CreateLogPanel(body, font, out logEmptyStateText);

            return body;

        }



        private static void CreatePartyRow(RectTransform body, Font font)

        {

            var partyRow = CreateRectChild(body, "PartyRow");

            var rowLayout = partyRow.gameObject.AddComponent<LayoutElement>();

            rowLayout.preferredHeight = ExplorationHudLayoutMetrics.PartyRowHeight;

            rowLayout.minHeight = ExplorationHudLayoutMetrics.PartyRowHeight;

            ApplyV2Sliced(partyRow.gameObject.AddComponent<Image>(), "ui_panel_s");

            CreatePartyScroll(partyRow, font);

        }



        private static void CreatePartyScroll(RectTransform partyRow, Font font)

        {

            var scrollGo = new GameObject("PartyScroll", typeof(RectTransform));

            scrollGo.transform.SetParent(partyRow, false);

            StretchFull(scrollGo.GetComponent<RectTransform>());



            var scrollRect = scrollGo.AddComponent<ScrollRect>();

            scrollRect.horizontal = true;

            scrollRect.vertical = false;

            scrollRect.movementType = ScrollRect.MovementType.Clamped;



            var viewport = CreateRectChild(scrollGo.GetComponent<RectTransform>(), "Viewport");

            StretchFull(viewport);

            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

            viewport.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.04f);

            scrollRect.viewport = viewport;



            var content = CreateRectChild(viewport, "Content");

            var contentRect = content;

            contentRect.anchorMin = new Vector2(0f, 0f);

            contentRect.anchorMax = new Vector2(0f, 1f);

            contentRect.pivot = new Vector2(0f, 0.5f);

            contentRect.anchoredPosition = Vector2.zero;

            contentRect.sizeDelta = new Vector2(0f, 0f);



            var layout = content.gameObject.AddComponent<HorizontalLayoutGroup>();

            layout.padding = new RectOffset(12, 12, 8, 8);

            layout.spacing = 12f;

            layout.childControlWidth = false;

            layout.childControlHeight = true;

            layout.childForceExpandHeight = true;

            content.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRect;



            for (var i = 0; i < 4; i++)

                CreatePartyMemberCard(content, font, i);

        }



        private static void CreatePartyMemberCard(RectTransform content, Font font, int index)

        {

            var cardGo = new GameObject($"PartyMemberCard_{index}", typeof(RectTransform), typeof(Image), typeof(PartyMemberCardView));

            cardGo.transform.SetParent(content, false);

            var cardRect = cardGo.GetComponent<RectTransform>();

            cardRect.sizeDelta = new Vector2(ExplorationHudLayoutMetrics.PartyMemberCardWidth, ExplorationHudLayoutMetrics.PartyMemberCardHeight);



            var cardLayout = cardGo.AddComponent<LayoutElement>();

            cardLayout.preferredWidth = ExplorationHudLayoutMetrics.PartyMemberCardWidth;

            cardLayout.minWidth = ExplorationHudLayoutMetrics.PartyMemberCardWidth;

            cardLayout.preferredHeight = ExplorationHudLayoutMetrics.PartyMemberCardHeight;

            cardLayout.minHeight = ExplorationHudLayoutMetrics.PartyMemberCardHeight;



            ApplyV2Sliced(cardGo.GetComponent<Image>(), "ui_panel_s");



            var row = CreateRectChild(cardRect, "Row");

            StretchWithInsets(row, 12f, 12f, 12f, 12f);

            var rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();

            rowLayout.spacing = 12f;

            rowLayout.childAlignment = TextAnchor.MiddleLeft;

            rowLayout.childControlWidth = false;

            rowLayout.childForceExpandWidth = false;



            var portraitSlot = CreateRectChild(row, "PortraitSlot");

            portraitSlot.sizeDelta = new Vector2(ExplorationHudLayoutMetrics.PortraitDisplaySize, ExplorationHudLayoutMetrics.PortraitDisplaySize);

            var portraitFrame = portraitSlot.gameObject.AddComponent<Image>();

            ApplySimpleSprite(portraitFrame, "portrait_frame", Color.white);

            var portraitTint = CreateRectChild(portraitSlot, "Tint");

            StretchWithInsets(portraitTint, 8f, 8f, 8f, 8f);

            var tintImage = portraitTint.gameObject.AddComponent<Image>();

            tintImage.color = new Color(0.43f, 0.77f, 1f, 0.85f);



            var infoColumn = CreateRectChild(row, "Info");

            var infoLayout = infoColumn.gameObject.AddComponent<VerticalLayoutGroup>();

            infoLayout.spacing = 4f;

            infoLayout.childAlignment = TextAnchor.UpperLeft;

            infoLayout.childControlWidth = true;

            infoLayout.childForceExpandWidth = true;

            infoColumn.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;



            var nameText = CreateLayoutText(infoColumn, "Name", "—", 18, TextAnchor.UpperLeft);

            ModernUiStyle.ApplyTitleMedium(nameText);

            var roleText = CreateLayoutText(infoColumn, "Role", string.Empty, 12, TextAnchor.UpperLeft);

            ModernUiStyle.ApplyMuted(roleText, 12);



            var hpRow = CreateRectChild(infoColumn, "HpBar");

            hpRow.sizeDelta = new Vector2(0f, ExplorationHudLayoutMetrics.HpBarHeight);

            var hpLayout = hpRow.gameObject.AddComponent<LayoutElement>();

            hpLayout.preferredHeight = ExplorationHudLayoutMetrics.HpBarHeight;

            hpLayout.minHeight = ExplorationHudLayoutMetrics.HpBarHeight;



            var hpTrackGo = new GameObject("Track", typeof(RectTransform), typeof(Image));

            hpTrackGo.transform.SetParent(hpRow, false);

            StretchFull(hpTrackGo.GetComponent<RectTransform>());

            var hpTrack = hpTrackGo.GetComponent<Image>();

            hpTrack.color = ModernUiStyle.HpTrack;



            var hpFillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));

            hpFillGo.transform.SetParent(hpTrackGo.transform, false);

            StretchFull(hpFillGo.GetComponent<RectTransform>());

            var hpFill = hpFillGo.GetComponent<Image>();

            hpFill.type = Image.Type.Filled;

            hpFill.fillMethod = Image.FillMethod.Horizontal;

            hpFill.color = ModernUiStyle.AccentGreen;



            var detailText = CreateLayoutText(infoColumn, "Detail", string.Empty, 12, TextAnchor.UpperLeft);

            ModernUiStyle.ApplyMuted(detailText, 12);

            detailText.horizontalOverflow = HorizontalWrapMode.Wrap;



            var cardView = cardGo.GetComponent<PartyMemberCardView>();

            var so = new SerializedObject(cardView);

            so.FindProperty("_nameText").objectReferenceValue = nameText;

            so.FindProperty("_roleText").objectReferenceValue = roleText;

            so.FindProperty("_detailText").objectReferenceValue = detailText;

            so.FindProperty("_portraitFrame").objectReferenceValue = portraitFrame;

            so.FindProperty("_portraitTint").objectReferenceValue = tintImage;

            so.FindProperty("_hpTrack").objectReferenceValue = hpTrack;

            so.FindProperty("_hpFill").objectReferenceValue = hpFill;

            so.ApplyModifiedPropertiesWithoutUndo();

        }



        private static void CreateCenterPanel(RectTransform body, Font font)

        {

            var centerPanel = CreateRectChild(body, "CenterPanel");

            var centerLayout = centerPanel.gameObject.AddComponent<LayoutElement>();

            centerLayout.flexibleHeight = 1f;

            centerLayout.minHeight = 640f;

            ApplyV2Sliced(centerPanel.gameObject.AddComponent<Image>(), "ui_panel_l");

            CreateStartCard(centerPanel, font);

            CreateExploreContent(centerPanel, font);

        }



        private static void CreateStartCard(RectTransform centerPanel, Font font)

        {

            var startCard = CreateRectChild(centerPanel, "StartCard");

            startCard.anchorMin = new Vector2(0.5f, 0.5f);

            startCard.anchorMax = new Vector2(0.5f, 0.5f);

            startCard.pivot = new Vector2(0.5f, 0.5f);

            startCard.sizeDelta = new Vector2(ExplorationHudLayoutMetrics.StartCardWidth, 0f);

            startCard.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;



            var cardLayout = startCard.gameObject.AddComponent<VerticalLayoutGroup>();

            cardLayout.padding = new RectOffset(16, 16, 16, 16);

            cardLayout.spacing = 16f;

            cardLayout.childAlignment = TextAnchor.UpperCenter;

            cardLayout.childControlWidth = true;

            cardLayout.childForceExpandWidth = true;



            var bannerSlot = CreateIllustrationSlot(startCard, "BannerSlot", ExplorationHudLayoutMetrics.StartCardBannerAspect, "illust_guild_start");

            var title = CreateLayoutText(startCard, "Title", "자동 탐험 길드", 24, TextAnchor.MiddleCenter);

            ModernUiStyle.ApplyTitleLarge(title);

            var summary = CreateLayoutText(startCard, "Summary", string.Empty, 15, TextAnchor.UpperCenter);

            ModernUiStyle.ApplyBody(summary, 15);

            summary.lineSpacing = 1.2f;



            var startButtonGo = new GameObject("StartButton", typeof(RectTransform), typeof(Image), typeof(Button));

            startButtonGo.transform.SetParent(startCard, false);

            var startButtonRect = startButtonGo.GetComponent<RectTransform>();

            startButtonRect.sizeDelta = new Vector2(280f, 56f);

            startButtonGo.AddComponent<LayoutElement>().preferredHeight = 56f;

            ApplyV2Sliced(startButtonGo.GetComponent<Image>(), "ui_btn_primary");

            var startLabel = CreateLayoutText(startButtonGo.transform, "Label", "탐험 시작", 16, TextAnchor.MiddleCenter);

            StretchFull(startLabel.rectTransform);

            ModernUiStyle.ApplyButtonLabel(startLabel, true);

        }



        private static void CreateExploreContent(RectTransform centerPanel, Font font)

        {

            var explore = CreateRectChild(centerPanel, "ExploreContent");

            StretchFull(explore);

            explore.gameObject.SetActive(false);



            var layout = explore.gameObject.AddComponent<VerticalLayoutGroup>();

            layout.padding = new RectOffset(16, 16, 16, 16);

            layout.spacing = 12f;

            layout.childAlignment = TextAnchor.UpperLeft;

            layout.childControlWidth = true;

            layout.childForceExpandWidth = true;



            CreateIllustrationSlot(explore, "BannerSlot", ExplorationHudLayoutMetrics.ZoneBannerAspect, "illust_zone_banner");



            var zoneTitle = CreateLayoutText(explore, "ZoneTitle", "탐험 진행", 24, TextAnchor.UpperLeft);

            ModernUiStyle.ApplyTitleLarge(zoneTitle);

            var floorLine = CreateLayoutText(explore, "FloorLine", string.Empty, 15, TextAnchor.UpperLeft);

            ModernUiStyle.ApplyBody(floorLine, 15);



            var progressTrack = CreateRectChild(explore, "ProgressTrack");

            progressTrack.sizeDelta = new Vector2(0f, 24f);

            progressTrack.gameObject.AddComponent<LayoutElement>().preferredHeight = 24f;

            ApplyV2Sliced(progressTrack.gameObject.AddComponent<Image>(), "ui_progress_track");



            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));

            fillGo.transform.SetParent(progressTrack, false);

            StretchWithInsets(fillGo.GetComponent<RectTransform>(), 4f, 4f, 4f, 4f);

            var fillImage = fillGo.GetComponent<Image>();

            ApplyV2Sliced(fillImage, "ui_progress_fill");
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;

            fillImage.fillAmount = 0f;



            var slider = progressTrack.gameObject.AddComponent<Slider>();

            slider.fillRect = fillGo.GetComponent<RectTransform>();

            slider.targetGraphic = fillImage;

            slider.direction = Slider.Direction.LeftToRight;

            slider.minValue = 0f;

            slider.maxValue = 1f;



            var progressLabel = CreateLayoutText(explore, "ProgressLabel", string.Empty, 13, TextAnchor.UpperLeft);

            ModernUiStyle.ApplyMuted(progressLabel, 13);



            var portraitStrip = CreateRectChild(explore, "PartyStrip");

            portraitStrip.sizeDelta = new Vector2(0f, ExplorationHudLayoutMetrics.PortraitDisplaySize);

            var stripLayout = portraitStrip.gameObject.AddComponent<HorizontalLayoutGroup>();

            stripLayout.spacing = 12f;

            stripLayout.childAlignment = TextAnchor.MiddleLeft;

            stripLayout.childControlWidth = false;

            stripLayout.childForceExpandHeight = false;



            for (var i = 0; i < 4; i++)

            {

                var slot = CreateRectChild(portraitStrip, $"Portrait_{i}");

                slot.sizeDelta = new Vector2(ExplorationHudLayoutMetrics.PortraitDisplaySize, ExplorationHudLayoutMetrics.PortraitDisplaySize);

                ApplySimpleSprite(slot.gameObject.AddComponent<Image>(), "portrait_frame", Color.white);

            }



            var statusLine = CreateLayoutText(explore, "StatusLine", string.Empty, 13, TextAnchor.UpperLeft);

            ModernUiStyle.ApplyMuted(statusLine, 13);

        }



        private static ExplorationLogFeedView CreateLogPanel(RectTransform body, Font font, out Text emptyStateText)

        {

            var logPanel = CreateRectChild(body, "LogPanel");

            var logLayout = logPanel.gameObject.AddComponent<LayoutElement>();

            logLayout.preferredHeight = ExplorationHudLayoutMetrics.LogPanelHeight;

            logLayout.minHeight = ExplorationHudLayoutMetrics.LogPanelHeight;

            ApplyV2Sliced(logPanel.gameObject.AddComponent<Image>(), "ui_panel_l");

            var header = CreateRectChild(logPanel, "LogHeader");

            header.sizeDelta = new Vector2(0f, 40f);

            header.gameObject.AddComponent<LayoutElement>().preferredHeight = 40f;

            ApplyV2Sliced(header.gameObject.AddComponent<Image>(), "ui_bar_top");



            var headerText = CreateLayoutText(header, "Label", "탐험 로그", 16, TextAnchor.MiddleLeft);

            headerText.rectTransform.offsetMin = new Vector2(16f, 0f);

            ModernUiStyle.ApplyTitleMedium(headerText);



            var logFeedGo = new GameObject("LogFeedView", typeof(RectTransform));

            logFeedGo.transform.SetParent(logPanel, false);

            var logFeedRect = logFeedGo.GetComponent<RectTransform>();

            StretchWithInsets(logFeedRect, 48f, 0f, 0f, 0f);



            var logFeedView = logFeedGo.AddComponent<ExplorationLogFeedView>();

            var scrollRect = logFeedGo.AddComponent<ScrollRect>();

            scrollRect.horizontal = false;

            scrollRect.movementType = ScrollRect.MovementType.Clamped;



            var viewport = CreateRectChild(logFeedRect, "Viewport");

            StretchFull(viewport);

            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

            viewport.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.06f);

            scrollRect.viewport = viewport;



            var content = CreateRectChild(viewport, "Content");

            var contentRect = content;

            contentRect.anchorMin = new Vector2(0f, 1f);

            contentRect.anchorMax = new Vector2(1f, 1f);

            contentRect.pivot = new Vector2(0.5f, 1f);

            contentRect.anchoredPosition = Vector2.zero;

            contentRect.sizeDelta = Vector2.zero;

            var contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();

            contentLayout.spacing = 8f;

            contentLayout.childControlHeight = true;

            contentLayout.childForceExpandHeight = false;

            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRect;



            var itemTemplate = CreateLogItemTemplate(logFeedGo.transform, font, out var itemView, out var messageText, out var categoryIcon, out var accentImage);

            WireLogItemView(itemView, messageText, categoryIcon, accentImage);

            WireLogFeedView(logFeedView, scrollRect, contentRect, itemView);



            var emptyState = CreateRectChild(viewport, "LogEmptyState");

            StretchWithInsets(emptyState, 0f, 0f, 16f, 16f);

            var emptyLayout = emptyState.gameObject.AddComponent<VerticalLayoutGroup>();

            emptyLayout.spacing = 12f;

            emptyLayout.childAlignment = TextAnchor.UpperCenter;

            emptyLayout.padding = new RectOffset(8, 8, 24, 8);



            var emptyIcon = CreateRectChild(emptyState, "Illustration");

            emptyIcon.sizeDelta = new Vector2(120f, 120f);

            emptyIcon.gameObject.AddComponent<Image>().color = new Color(0.2f, 0.26f, 0.36f, 0.35f);



            emptyStateText = CreateLayoutText(emptyState, "Message",

                "탐험 시작 전입니다.\n\n중앙 「탐험 시작」 버튼을 누르면\n실시간 탐험 로그가 표시됩니다.",

                13, TextAnchor.UpperCenter);

            ModernUiStyle.ApplyMuted(emptyStateText, 13);



            return logFeedView;

        }



        private static RectTransform CreateIllustrationSlot(RectTransform parent, string name, float aspect, string spriteName)

        {

            var slot = CreateRectChild(parent, name);

            slot.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var fitter = slot.gameObject.AddComponent<AspectRatioFitter>();

            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;

            fitter.aspectRatio = aspect;



            var image = slot.gameObject.AddComponent<Image>();

            ApplySimpleSprite(image, spriteName, Color.white);

            return slot;

        }



        private static void CreateBottomTabBar(RectTransform root, Font font)

        {

            var barRoot = CreateRectChild(root, "BottomTabBar");

            barRoot.anchorMin = new Vector2(0f, 0f);

            barRoot.anchorMax = new Vector2(1f, 0f);

            barRoot.pivot = new Vector2(0.5f, 0f);

            barRoot.sizeDelta = new Vector2(0f, ExplorationHudLayoutMetrics.TabBarHeight);

            ApplyV2Sliced(barRoot.gameObject.AddComponent<Image>(), "ui_bar_bottom");



            var tabs = CreateRectChild(barRoot, "Tabs");

            StretchWithInsets(tabs, 4f, 4f, 8f, 8f);

            var tabLayout = tabs.gameObject.AddComponent<HorizontalLayoutGroup>();

            tabLayout.spacing = 4f;

            tabLayout.childAlignment = TextAnchor.MiddleCenter;

            tabLayout.childControlWidth = true;

            tabLayout.childForceExpandWidth = true;

            tabLayout.childControlHeight = true;

            tabLayout.childForceExpandHeight = true;



            var labels = new[] { "탐험", "강화/장비", "길드시설", "연대기", "도감" };

            var iconNames = new[] { "icon_tab_explore", "icon_tab_enhance", "icon_tab_guild", "icon_tab_chronicle", "icon_tab_compendium" };



            for (var i = 0; i < labels.Length; i++)

                CreateTabItem(tabs, labels[i], iconNames[i], font, i == 0);

        }



        private static void CreateOverlays(GameObject root, RectTransform rootRect, Font font)

        {

            var overlays = CreateRectChild(rootRect, "Overlays");

            StretchFull(overlays);



            CreateEnhanceOverlay(root, overlays, font);

            CreateChronicleOverlay(root, overlays, font);

            CreateSettingsOverlay(root, overlays, font);

            CreateCharacterDetailOverlay(root, overlays, font);

            CreateGuildFacilityOverlay(root, overlays, font);

            CreateDynamicEventOverlay(root, overlays, font);

        }



        private static GameObject CreateCenteredOverlayPanel(

            RectTransform parent,

            string name,

            Vector2 size,

            Font font)

        {

            var panelGo = new GameObject(name, typeof(RectTransform), typeof(Image));

            panelGo.transform.SetParent(parent, false);

            var panelRect = panelGo.GetComponent<RectTransform>();

            panelRect.anchorMin = new Vector2(0.5f, 0.5f);

            panelRect.anchorMax = new Vector2(0.5f, 0.5f);

            panelRect.sizeDelta = size;

            ApplyV2Sliced(panelGo.GetComponent<Image>(), "ui_panel_l");

            return panelGo;

        }



        private static Text CreateAnchoredText(

            Transform parent,

            string name,

            Vector2 anchoredPos,

            Vector2 size,

            int fontSize,

            string text,

            Font font,

            Color? color = null)

        {

            var go = new GameObject(name, typeof(RectTransform));

            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();

            rect.anchorMin = new Vector2(0f, 1f);

            rect.anchorMax = new Vector2(0f, 1f);

            rect.pivot = new Vector2(0f, 1f);

            rect.anchoredPosition = anchoredPos;

            rect.sizeDelta = size;



            var label = go.AddComponent<Text>();

            label.font = font;

            label.fontSize = fontSize;

            label.alignment = TextAnchor.UpperLeft;

            label.color = color ?? Color.white;

            label.supportRichText = true;

            label.horizontalOverflow = HorizontalWrapMode.Wrap;

            label.verticalOverflow = VerticalWrapMode.Overflow;

            label.text = text;

            return label;

        }



        private static Button CreateAnchoredActionButton(

            Transform parent,

            string name,

            Vector2 anchoredPos,

            Vector2 size,

            string labelText,

            Font font)

        {

            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));

            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();

            rect.anchorMin = new Vector2(0f, 0f);

            rect.anchorMax = new Vector2(0f, 0f);

            rect.pivot = new Vector2(0f, 0f);

            rect.anchoredPosition = anchoredPos;

            rect.sizeDelta = size;



            var image = go.GetComponent<Image>();

            image.color = new Color(0.24f, 0.18f, 0.12f, 0.95f);



            var button = go.GetComponent<Button>();

            button.targetGraphic = image;



            var labelGo = new GameObject("Label", typeof(RectTransform));

            labelGo.transform.SetParent(go.transform, false);

            StretchFull(labelGo.GetComponent<RectTransform>());

            var text = labelGo.AddComponent<Text>();

            text.font = font;

            text.fontSize = 13;

            text.alignment = TextAnchor.MiddleCenter;

            text.color = new Color(1f, 0.92f, 0.75f);

            text.text = labelText;

            return button;

        }



        private static void WireOverlayView(

            ExplorationOverlayView view,

            GameObject overlayRoot,

            Text contentText,

            Text pageText = null,

            Text hintText = null)

        {

            var so = new SerializedObject(view);

            so.FindProperty("_overlayRoot").objectReferenceValue = overlayRoot;

            so.FindProperty("_contentText").objectReferenceValue = contentText;

            if (pageText != null && so.FindProperty("_pageText") != null)

                so.FindProperty("_pageText").objectReferenceValue = pageText;

            if (hintText != null && so.FindProperty("_hintText") != null)

                so.FindProperty("_hintText").objectReferenceValue = hintText;

            so.ApplyModifiedPropertiesWithoutUndo();

            overlayRoot.SetActive(false);

        }



        private static void CreateEnhanceOverlay(GameObject root, RectTransform overlays, Font font)

        {

            var panelGo = CreateCenteredOverlayPanel(overlays, "EnhancePanel", new Vector2(960f, 480f), font);

            CreateAnchoredText(panelGo.transform, "Title", new Vector2(20f, -16f), new Vector2(920f, 32f), 22, "강화 / 장비", font);

            CreateAnchoredText(panelGo.transform, "Hint", new Vector2(20f, -44f), new Vector2(600f, 20f), 13,

                "1:전직  2:무기 강화  3:방어구 강화  (리더 기준 · 대장간 Lv.1+)", font, new Color(0.75f, 0.75f, 0.8f));

            var contentText = CreateAnchoredText(panelGo.transform, "Content", new Vector2(20f, -68f), new Vector2(600f, 300f), 16, string.Empty, font);

            WireOverlayView(root.GetComponent<EnhanceRuntimePanel>(), panelGo, contentText);

        }



        private static void CreateChronicleOverlay(GameObject root, RectTransform overlays, Font font)

        {

            var panelGo = CreateCenteredOverlayPanel(overlays, "ChroniclePanel", new Vector2(960f, 520f), font);

            CreateAnchoredText(panelGo.transform, "Title", new Vector2(20f, -16f), new Vector2(920f, 32f), 22, "[ 연대기 ]", font);

            CreateAnchoredText(panelGo.transform, "Hint", new Vector2(20f, -44f), new Vector2(920f, 20f), 13,

                "1:회차  2:즐겨찾기  3:캐릭터 일지  4:로어 도감  5:몬스터 도감  Q/E:캐릭터  PgUp/PgDn 또는 [/]:페이지", font, new Color(0.75f, 0.75f, 0.8f));

            var pageText = CreateAnchoredText(panelGo.transform, "Page", new Vector2(20f, -58f), new Vector2(920f, 18f), 12, string.Empty, font, new Color(0.65f, 0.65f, 0.72f));

            var contentText = CreateAnchoredText(panelGo.transform, "Content", new Vector2(20f, -78f), new Vector2(920f, 418f), 16, string.Empty, font);

            WireOverlayView(root.GetComponent<ChronicleRuntimePanel>(), panelGo, contentText, pageText);

        }



        private static void CreateSettingsOverlay(GameObject root, RectTransform overlays, Font font)

        {

            var panelGo = CreateCenteredOverlayPanel(overlays, "SettingsPanel", new Vector2(640f, 440f), font);

            CreateAnchoredText(panelGo.transform, "Title", new Vector2(20f, -16f), new Vector2(600f, 32f), 22, "[ 탐험 설정 ]", font);

            CreateAnchoredText(panelGo.transform, "Hint", new Vector2(20f, -44f), new Vector2(600f, 20f), 13,

                "1:LLM  2:이벤트  3:황금  4:로그  5:오프라인  6:서고  7:훈련  8:대장  9:여관  0:서점  -:스킬  (O:닫기)", font, new Color(0.75f, 0.75f, 0.8f));

            var contentText = CreateAnchoredText(panelGo.transform, "Content", new Vector2(20f, -68f), new Vector2(600f, 308f), 16, string.Empty, font);

            WireOverlayView(root.GetComponent<ExplorationSettingsRuntimePanel>(), panelGo, contentText);

        }



        private static void CreateCharacterDetailOverlay(GameObject root, RectTransform overlays, Font font)

        {

            var panelGo = CreateCenteredOverlayPanel(overlays, "CharacterDetailPanel", new Vector2(520f, 460f), font);

            CreateAnchoredText(panelGo.transform, "Title", new Vector2(20f, -16f), new Vector2(480f, 28f), 22, "[ 캐릭터 상세 ]", font);

            var hintText = CreateAnchoredText(panelGo.transform, "Hint", new Vector2(20f, -44f), new Vector2(480f, 20f), 13, "I:열기/닫기  Q/E:캐릭터", font, new Color(0.72f, 0.74f, 0.8f));

            var contentText = CreateAnchoredText(panelGo.transform, "Content", new Vector2(20f, -68f), new Vector2(480f, 340f), 15, string.Empty, font);

            WireOverlayView(root.GetComponent<CharacterDetailRuntimePanel>(), panelGo, contentText, hintText: hintText);

        }



        private static void CreateGuildFacilityOverlay(GameObject root, RectTransform overlays, Font font)

        {

            var panelGo = CreateCenteredOverlayPanel(overlays, "GuildFacilityPanel", new Vector2(640f, 440f), font);

            CreateAnchoredText(panelGo.transform, "Title", new Vector2(20f, -16f), new Vector2(600f, 32f), 22, "[ 길드 시설 ]", font);

            CreateAnchoredText(panelGo.transform, "Hint", new Vector2(20f, -44f), new Vector2(600f, 20f), 13,

                "6:서고  7:훈련  8:대장  9:여관  0:서점  -:스킬", font, new Color(0.75f, 0.75f, 0.8f));

            var contentText = CreateAnchoredText(panelGo.transform, "Content", new Vector2(20f, -68f), new Vector2(600f, 308f), 16, string.Empty, font);

            WireOverlayView(root.GetComponent<GuildFacilityRuntimePanel>(), panelGo, contentText);

        }



        private static void CreateDynamicEventOverlay(GameObject root, RectTransform overlays, Font font)

        {

            var panelGo = new GameObject("DynamicEventPopup", typeof(RectTransform), typeof(Image));

            panelGo.transform.SetParent(overlays, false);

            StretchFull(panelGo.GetComponent<RectTransform>());

            var dim = panelGo.GetComponent<Image>();

            dim.color = new Color(0f, 0f, 0f, 0.65f);

            dim.raycastTarget = true;



            var box = CreateRectChild(panelGo.GetComponent<RectTransform>(), "Box");

            box.anchorMin = new Vector2(0.5f, 0.5f);

            box.anchorMax = new Vector2(0.5f, 0.5f);

            box.sizeDelta = new Vector2(720f, 420f);

            var boxImage = box.gameObject.AddComponent<Image>();

            boxImage.color = new Color(0.12f, 0.12f, 0.16f, 0.95f);



            var titleText = CreateAnchoredText(box, "Title", new Vector2(24f, -20f), new Vector2(672f, 32f), 24, "[이벤트]", font);

            var narrationText = CreateAnchoredText(box, "Narration", new Vector2(24f, -60f), new Vector2(672f, 160f), 18, string.Empty, font);

            var choicesText = CreateAnchoredText(box, "Choices", new Vector2(24f, -240f), new Vector2(672f, 48f), 16, string.Empty, font, new Color(0.85f, 0.9f, 1f));



            var choiceRootGo = new GameObject("ChoiceButtons", typeof(RectTransform));

            choiceRootGo.transform.SetParent(box, false);

            var choiceButtonRoot = choiceRootGo.GetComponent<RectTransform>();

            choiceButtonRoot.anchorMin = new Vector2(0f, 0f);

            choiceButtonRoot.anchorMax = new Vector2(1f, 0f);

            choiceButtonRoot.pivot = new Vector2(0.5f, 0f);

            choiceButtonRoot.anchoredPosition = new Vector2(0f, 24f);

            choiceButtonRoot.sizeDelta = new Vector2(-48f, 96f);

            var layout = choiceRootGo.AddComponent<HorizontalLayoutGroup>();

            layout.spacing = 12f;

            layout.childAlignment = TextAnchor.MiddleCenter;

            layout.childControlWidth = true;

            layout.childControlHeight = true;

            layout.childForceExpandWidth = true;

            layout.childForceExpandHeight = true;

            layout.padding = new RectOffset(24, 24, 0, 0);



            var pauseButton = CreateAnchoredActionButton(

                box,

                "PauseForManualButton",

                new Vector2(24f, 132f),

                new Vector2(220f, 36f),

                "일시정지하고 직접 선택",

                font);



            var popup = root.GetComponent<DynamicEventRuntimePopup>();

            var so = new SerializedObject(popup);

            so.FindProperty("_panelRoot").objectReferenceValue = panelGo;

            so.FindProperty("_titleText").objectReferenceValue = titleText;

            so.FindProperty("_narrationText").objectReferenceValue = narrationText;

            so.FindProperty("_choicesText").objectReferenceValue = choicesText;

            so.FindProperty("_choiceButtonRoot").objectReferenceValue = choiceButtonRoot;

            so.FindProperty("_pauseForManualButton").objectReferenceValue = pauseButton;

            so.ApplyModifiedPropertiesWithoutUndo();

            panelGo.SetActive(false);

        }



        private static void CreateTabItem(RectTransform parent, string label, string iconName, Font font, bool active)

        {

            var go = new GameObject($"Tab_{label}", typeof(RectTransform), typeof(Image), typeof(Button));

            go.transform.SetParent(parent, false);

            go.AddComponent<LayoutElement>().preferredHeight = 64f;

            ApplyTabSprite(go.GetComponent<Image>(), active);

            go.GetComponent<Button>().targetGraphic = go.GetComponent<Image>();



            var content = CreateRectChild(go.GetComponent<RectTransform>(), "Content");

            StretchFull(content);

            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();

            layout.spacing = 2f;

            layout.childAlignment = TextAnchor.MiddleCenter;

            layout.childControlWidth = true;

            layout.childControlHeight = false;



            var iconGo = CreateRectChild(content, "Icon");

            iconGo.sizeDelta = new Vector2(ExplorationHudLayoutMetrics.TabIconDisplaySize, ExplorationHudLayoutMetrics.TabIconDisplaySize);

            ApplySimpleSprite(iconGo.gameObject.AddComponent<Image>(), iconName, Color.white);



            var labelText = CreateLayoutText(content, "Label", label, 11, TextAnchor.MiddleCenter);

            ModernUiStyle.ApplyTabLabel(labelText, active);

        }



        private static GameObject CreateLogItemTemplate(

            Transform parent,

            Font font,

            out ExplorationLogItemView itemView,

            out Text messageText,

            out Image categoryIcon,

            out Image accentImage)

        {

            var itemTemplateGo = new GameObject("LogItemTemplate", typeof(RectTransform));

            itemTemplateGo.transform.SetParent(parent, false);

            itemTemplateGo.SetActive(false);



            itemView = itemTemplateGo.AddComponent<ExplorationLogItemView>();

            var itemLayout = itemTemplateGo.AddComponent<LayoutElement>();

            itemLayout.minHeight = 48f;

            itemLayout.preferredHeight = 48f;



            var backgroundGo = new GameObject("Background", typeof(RectTransform), typeof(Image));

            backgroundGo.transform.SetParent(itemTemplateGo.transform, false);

            StretchFull(backgroundGo.GetComponent<RectTransform>());

            ApplyV2Sliced(backgroundGo.GetComponent<Image>(), "ui_panel_s");



            var row = CreateRectChild(itemTemplateGo.GetComponent<RectTransform>(), "Row");

            StretchWithInsets(row, 8f, 8f, 8f, 8f);

            var rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();

            rowLayout.spacing = 8f;

            rowLayout.childAlignment = TextAnchor.MiddleLeft;

            rowLayout.childControlWidth = false;

            rowLayout.childForceExpandWidth = false;



            var accentGo = CreateRectChild(row, "Accent");

            accentGo.sizeDelta = new Vector2(4f, 32f);

            accentImage = accentGo.gameObject.AddComponent<Image>();

            ApplySimpleSprite(accentImage, "log_accent_strip", Color.white);

            if (accentImage.sprite == null)

                accentImage.color = ModernUiStyle.AccentCyan;



            var iconGo = CreateRectChild(row, "CategoryIcon");

            iconGo.sizeDelta = new Vector2(ExplorationHudLayoutMetrics.LogIconDisplaySize, ExplorationHudLayoutMetrics.LogIconDisplaySize);

            categoryIcon = iconGo.gameObject.AddComponent<Image>();

            ApplySimpleSprite(categoryIcon, "icon_log_narrative", Color.white);



            var messageGo = CreateRectChild(row, "Message");

            messageGo.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            messageText = messageGo.gameObject.AddComponent<Text>();

            messageText.font = font;

            messageText.fontSize = 15;

            messageText.alignment = TextAnchor.UpperLeft;

            ModernUiStyle.ApplyBody(messageText, 15);

            messageText.supportRichText = true;

            messageText.horizontalOverflow = HorizontalWrapMode.Wrap;

            messageText.verticalOverflow = VerticalWrapMode.Overflow;

            return itemTemplateGo;

        }



        private static RectTransform CreatePanelColumn(RectTransform body, string name, float width, bool flexible = false)

        {

            var panel = CreateRectChild(body, name);

            panel.gameObject.AddComponent<LayoutElement>().preferredWidth = width;

            if (flexible)

                panel.gameObject.GetComponent<LayoutElement>().flexibleWidth = 1f;



            ApplyV2Sliced(panel.gameObject.AddComponent<Image>(), "ui_panel_l");

            return panel;

        }



        private static void AddRuntimeComponents(GameObject root)

        {

            EnsureComponent<ChronicleRuntimePanel>(root);

            EnsureComponent<ExplorationSettingsRuntimePanel>(root);

            EnsureComponent<PartyRuntimePanel>(root);

            EnsureComponent<ExplorationCenterRuntimePanel>(root);

            EnsureComponent<CharacterDetailRuntimePanel>(root);

            EnsureComponent<DynamicEventRuntimePopup>(root);

            EnsureComponent<EnhanceRuntimePanel>(root);

            EnsureComponent<GuildFacilityRuntimePanel>(root);

            EnsureComponent<GuildHudTabController>(root);

            EnsureComponent<ExplorationStartRuntimePanel>(root);

        }



        private static void EnsureComponent<T>(GameObject root) where T : Component

        {

            if (root.GetComponent<T>() == null)

                root.AddComponent<T>();

        }



        private static void WireHudPanel(

            ExplorationHudPanel panel,

            Text statusText,

            Text goldText,

            CommonButton pauseButton,

            CommonButton resumeButton,

            CommonButton returnButton,

            ExplorationLogFeedView logFeedView,

            Text filterText,

            Text helpText)

        {

            var so = new SerializedObject(panel);

            so.FindProperty("_zoneFloorText").objectReferenceValue = statusText;

            so.FindProperty("_goldText").objectReferenceValue = goldText;

            so.FindProperty("_pauseButton").objectReferenceValue = pauseButton;

            so.FindProperty("_resumeButton").objectReferenceValue = resumeButton;

            so.FindProperty("_returnButton").objectReferenceValue = returnButton;

            so.FindProperty("_logFeedView").objectReferenceValue = logFeedView;

            so.FindProperty("_filterText").objectReferenceValue = filterText;

            so.FindProperty("_helpText").objectReferenceValue = helpText;

            so.ApplyModifiedPropertiesWithoutUndo();

        }



        private static void WireLogItemView(ExplorationLogItemView view, Text messageText, Image categoryIcon, Image accentImage)

        {

            var so = new SerializedObject(view);

            so.FindProperty("_messageText").objectReferenceValue = messageText;

            so.FindProperty("_categoryIcon").objectReferenceValue = categoryIcon;

            if (so.FindProperty("_accentImage") != null)

                so.FindProperty("_accentImage").objectReferenceValue = accentImage;

            so.ApplyModifiedPropertiesWithoutUndo();

        }



        private static void WireLogFeedView(

            ExplorationLogFeedView view,

            ScrollRect scrollRect,

            RectTransform contentRoot,

            ExplorationLogItemView itemPrefab)

        {

            var so = new SerializedObject(view);

            so.FindProperty("_scrollRect").objectReferenceValue = scrollRect;

            so.FindProperty("_contentRoot").objectReferenceValue = contentRoot;

            so.FindProperty("_itemPrefab").objectReferenceValue = itemPrefab;

            if (so.FindProperty("_idlePlaceholderRoot") != null)

                so.FindProperty("_idlePlaceholderRoot").objectReferenceValue = contentRoot.parent.Find("LogEmptyState")?.gameObject;

            so.ApplyModifiedPropertiesWithoutUndo();

        }



        private static void WireLogFeedEmptyState(ExplorationLogFeedView view, Text emptyStateText)

        {

            var so = new SerializedObject(view);

            if (so.FindProperty("_idlePlaceholderRoot") != null)

            {

                var emptyRoot = view.transform.Find("Viewport/LogEmptyState")?.gameObject;

                so.FindProperty("_idlePlaceholderRoot").objectReferenceValue = emptyRoot;

            }



            if (so.FindProperty("_idlePlaceholderText") != null)

                so.FindProperty("_idlePlaceholderText").objectReferenceValue = emptyStateText;

            so.ApplyModifiedPropertiesWithoutUndo();

        }



        private static Text CreateLayoutText(Transform parent, string name, string text, int fontSize, TextAnchor alignment, Font font = null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var label = go.AddComponent<Text>();
            label.font = font
                ?? AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/BMJUA_ttf.ttf")
                ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            label.fontSize = fontSize;

            label.alignment = alignment;

            label.text = text;

            label.supportRichText = true;

            label.horizontalOverflow = HorizontalWrapMode.Wrap;

            label.verticalOverflow = VerticalWrapMode.Overflow;

            go.AddComponent<LayoutElement>().flexibleWidth = 1f;

            return label;

        }



        private static CommonButton CreateLayoutButton(
            RectTransform parent,
            string name,
            Font font,
            string label,
            bool primary,
            float width,
            float height)

        {

            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(CommonButton));

            go.transform.SetParent(parent, false);

            var layout = go.AddComponent<LayoutElement>();

            layout.preferredWidth = width;

            layout.preferredHeight = height;

            if (primary)

                ApplyV2Sliced(go.GetComponent<Image>(), "ui_btn_primary");

            else

                ApplyV2Sliced(go.GetComponent<Image>(), "ui_btn_secondary");



            var labelText = CreateLayoutText(go.transform, "Label", label, 16, TextAnchor.MiddleCenter);

            StretchFull(labelText.rectTransform);

            ModernUiStyle.ApplyButtonLabel(labelText, primary);

            return go.GetComponent<CommonButton>();

        }



        private static RectTransform CreateRectChild(RectTransform parent, string name)

        {

            var go = new GameObject(name, typeof(RectTransform));

            go.transform.SetParent(parent, false);

            return go.GetComponent<RectTransform>();

        }



        private static void StretchFull(RectTransform rect)

        {

            rect.anchorMin = Vector2.zero;

            rect.anchorMax = Vector2.one;

            rect.offsetMin = Vector2.zero;

            rect.offsetMax = Vector2.zero;

        }



        private static void StretchHorizontal(RectTransform rect, float height, float bottomInset)

        {

            rect.anchorMin = new Vector2(0f, 1f);

            rect.anchorMax = new Vector2(1f, 1f);

            rect.pivot = new Vector2(0.5f, 1f);

            rect.anchoredPosition = new Vector2(0f, -bottomInset);

            rect.sizeDelta = new Vector2(0f, height);

        }



        private static void StretchWithInsets(RectTransform rect, float top, float bottom, float left, float right)

        {

            rect.anchorMin = Vector2.zero;

            rect.anchorMax = Vector2.one;

            rect.offsetMin = new Vector2(left, bottom);

            rect.offsetMax = new Vector2(-right, -top);

        }



        private static string V2Path(string fileName) => $"{V2Folder}/{fileName}.png";



        private static void ApplyTabSprite(Image image, bool active) =>

            ApplyV2Sliced(image, active ? "ui_tab_on" : "ui_tab_off");



        private static void ApplySimpleSprite(Image image, string fileName, Color color)

        {

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(V2Path(fileName));

            if (sprite == null)

            {

                image.color = new Color(color.r, color.g, color.b, 0.35f);

                return;

            }



            image.sprite = sprite;

            image.type = Image.Type.Simple;

            image.preserveAspect = true;

            image.color = color;

        }



        private static void ApplyV2Sliced(Image image, string fileName)

        {

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(V2Path(fileName));

            if (sprite == null)

            {

                image.color = new Color(0.08f, 0.08f, 0.12f, 0.88f);

                return;

            }



            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = Color.white;

        }

    }

}

#endif

