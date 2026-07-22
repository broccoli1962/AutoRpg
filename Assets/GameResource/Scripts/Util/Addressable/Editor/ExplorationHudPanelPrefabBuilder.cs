#if UNITY_EDITOR
using Backend.GameSystems.DynamicEvent;
using Backend.GameSystems.Exploration;
using Backend.Object.UI;
using Backend.Object.UI.Exploration;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Editor
{
    /// <summary>
    /// 12_UIUX 3-panel ExplorationHudPanel Addressable 프리팹을 에디터에서 생성한다.
    /// </summary>
    public static class ExplorationHudPanelPrefabBuilder
    {
        private const string PrefabPath = "Assets/GameResource/Prefabs/UI/ExplorationHudPanel.prefab";
        private const float TopBarHeight = ExplorationHudLayoutMetrics.TopBarHeight;
        private const float BottomInset = ExplorationHudLayoutMetrics.TabBarHeight + ExplorationHudLayoutMetrics.TabBarPadding;
        private const float LeftWidth = ExplorationHudLayoutMetrics.LeftPanelWidth;
        private const float RightWidth = ExplorationHudLayoutMetrics.RightPanelWidth;

        [MenuItem("Tools/Addressables/Build Exploration HUD Panel Prefab")]
        public static void BuildPrefab()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var root = new GameObject("ExplorationHudPanel", typeof(RectTransform));
            var rootRect = root.GetComponent<RectTransform>();
            StretchFull(rootRect);

            var hudPanel = root.AddComponent<ExplorationHudPanel>();
            AddRuntimeComponents(root);

            var topBar = CreateRectChild(rootRect, "TopBar");
            StretchHorizontal(topBar, TopBarHeight, 0f);

            var statusText = CreateText(topBar, "ZoneFloorText", font, 17, new Vector2(20f, -12f), new Vector2(1180f, 28f));
            statusText.supportRichText = true;
            var goldText = CreateText(topBar, "GoldText", font, 16, new Vector2(20f, -44f), new Vector2(220f, 20f));
            goldText.gameObject.SetActive(false);
            var filterText = CreateText(topBar, "FilterText", font, 13, new Vector2(20f, -68f), new Vector2(360f, 18f));
            filterText.color = new Color(0.8f, 0.8f, 0.85f);
            var helpText = CreateText(topBar, "HelpText", font, 12, new Vector2(400f, -68f), new Vector2(800f, 18f));
            helpText.alignment = TextAnchor.UpperLeft;
            helpText.text = "L:LLM  A:이벤트  G:황금  O:설정  C:연대기  I:캐릭터  R:귀환  F:필터  B:북마크  [/]:로그";
            var pauseButton = CreateButton(topBar, "PauseButton", font, "일시정지", new Vector2(20f, -92f));
            var resumeButton = CreateButton(topBar, "ResumeButton", font, "재개", new Vector2(132f, -92f));
            var returnButton = CreateButton(topBar, "ReturnButton", font, "귀환", new Vector2(244f, -92f));

            var body = CreateRectChild(rootRect, "Body");
            StretchWithInsets(body, TopBarHeight, BottomInset, 0f, 0f);

            var leftPanel = CreateRectChild(body, "LeftPanel");
            AnchorLeftColumn(leftPanel, LeftWidth);
            leftPanel.gameObject.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f, 0.88f);
            CreateStretchChild(leftPanel, "PartyContent");

            var centerPanel = CreateRectChild(body, "CenterPanel");
            AnchorCenterColumn(centerPanel, LeftWidth, RightWidth);
            centerPanel.gameObject.AddComponent<Image>().color = new Color(0.07f, 0.09f, 0.12f, 0.82f);
            CreateStretchChild(centerPanel, "CenterContent");
            CreatePortraitStripPlaceholder(centerPanel);

            var rightPanel = CreateRectChild(body, "RightPanel");
            AnchorRightColumn(rightPanel, RightWidth);
            rightPanel.gameObject.AddComponent<Image>().color = new Color(0.06f, 0.07f, 0.1f, 0.75f);
            CreateText(rightPanel, "LogHeader", font, 14, new Vector2(12f, -8f), new Vector2(ExplorationHudLayoutMetrics.RightPanelContentWidth, 20f)).text = "[ 로그 ]";

            var logFeedGo = new GameObject("LogFeedView", typeof(RectTransform));
            logFeedGo.transform.SetParent(rightPanel, false);
            var logFeedRect = logFeedGo.GetComponent<RectTransform>();
            StretchWithInsets(logFeedRect, 32f, 0f, 8f, 8f);

            var logFeedView = logFeedGo.AddComponent<ExplorationLogFeedView>();
            var scrollRect = logFeedGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            var viewportGo = new GameObject("Viewport", typeof(RectTransform));
            viewportGo.transform.SetParent(logFeedGo.transform, false);
            var viewportRect = viewportGo.GetComponent<RectTransform>();
            StretchFull(viewportRect);
            viewportGo.AddComponent<Mask>().showMaskGraphic = false;
            viewportGo.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.15f);
            scrollRect.viewport = viewportRect;

            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRect = contentGo.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;
            var layout = contentGo.AddComponent<VerticalLayoutGroup>();
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 6f;
            contentGo.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.content = contentRect;

            var itemTemplateGo = new GameObject("LogItemTemplate", typeof(RectTransform));
            itemTemplateGo.transform.SetParent(logFeedGo.transform, false);
            itemTemplateGo.SetActive(false);
            var itemView = itemTemplateGo.AddComponent<ExplorationLogItemView>();
            var itemLayout = itemTemplateGo.AddComponent<LayoutElement>();
            itemLayout.minHeight = 28f;
            itemLayout.preferredHeight = 28f;
            var itemText = CreateText(itemTemplateGo.transform, "Message", font, 15, Vector2.zero, new Vector2(ExplorationHudLayoutMetrics.RightPanelContentWidth, 28f));
            StretchFull(itemText.rectTransform);
            itemText.horizontalOverflow = HorizontalWrapMode.Wrap;
            itemText.verticalOverflow = VerticalWrapMode.Overflow;
            WireLogItemView(itemView, itemText, null);

            WireLogFeedView(logFeedView, scrollRect, contentRect, itemView);
            WireHudPanel(
                hudPanel,
                statusText,
                goldText,
                pauseButton,
                resumeButton,
                returnButton,
                logFeedView,
                filterText,
                helpText);

            CreateBottomTabBarPlaceholder(rootRect, font);

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[ExplorationHudPanelPrefabBuilder] Saved {PrefabPath}");
        }

        private static void CreatePortraitStripPlaceholder(RectTransform centerPanel)
        {
            var strip = CreateRectChild(centerPanel, "PortraitStrip");
            strip.anchorMin = new Vector2(0f, 0f);
            strip.anchorMax = new Vector2(1f, 0f);
            strip.pivot = new Vector2(0.5f, 0f);
            strip.anchoredPosition = new Vector2(0f, 12f);
            strip.sizeDelta = new Vector2(-32f, 80f);

            var layout = strip.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(16, 16, 0, 0);
        }

        private static void CreateStretchChild(RectTransform parent, string name)
        {
            var child = CreateRectChild(parent, name);
            StretchFull(child);
        }

        private static void CreateBottomTabBarPlaceholder(RectTransform rootRect, Font font)
        {
            var tabBarHeight = ExplorationHudLayoutMetrics.TabBarHeight;
            var barRoot = CreateRectChild(rootRect, "BottomTabBar");
            barRoot.anchorMin = new Vector2(0f, 0f);
            barRoot.anchorMax = new Vector2(1f, 0f);
            barRoot.pivot = new Vector2(0.5f, 0f);
            barRoot.sizeDelta = new Vector2(0f, tabBarHeight);
            barRoot.gameObject.AddComponent<Image>().color = new Color(0.06f, 0.07f, 0.1f, 0.94f);

            var labels = new[] { "탐험", "강화/장비", "길드시설", "연대기", "도감" };
            var activeColor = new Color(0.28f, 0.42f, 0.62f, 0.98f);
            var inactiveColor = new Color(0.14f, 0.16f, 0.22f, 0.95f);

            for (var i = 0; i < labels.Length; i++)
            {
                var go = new GameObject($"Tab_{labels[i]}", typeof(RectTransform));
                go.transform.SetParent(barRoot, false);

                var rect = go.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2((float)i / labels.Length, 0f);
                rect.anchorMax = new Vector2((float)(i + 1) / labels.Length, 1f);
                rect.offsetMin = new Vector2(2f, 4f);
                rect.offsetMax = new Vector2(-2f, -4f);

                var image = go.AddComponent<Image>();
                image.color = i == 0 ? activeColor : inactiveColor;
                var button = go.AddComponent<Button>();
                button.targetGraphic = image;

                var labelGo = new GameObject("Label", typeof(RectTransform));
                labelGo.transform.SetParent(go.transform, false);
                StretchFull(labelGo.GetComponent<RectTransform>());
                var text = labelGo.AddComponent<Text>();
                text.font = font;
                text.fontSize = 14;
                text.alignment = TextAnchor.MiddleCenter;
                text.color = Color.white;
                text.text = labels[i];
            }
        }

        private static void AddRuntimeComponents(GameObject root)
        {
            if (root.GetComponent<ChronicleRuntimePanel>() == null)
                root.AddComponent<ChronicleRuntimePanel>();
            if (root.GetComponent<ExplorationSettingsRuntimePanel>() == null)
                root.AddComponent<ExplorationSettingsRuntimePanel>();
            if (root.GetComponent<PartyRuntimePanel>() == null)
                root.AddComponent<PartyRuntimePanel>();
            if (root.GetComponent<ExplorationCenterRuntimePanel>() == null)
                root.AddComponent<ExplorationCenterRuntimePanel>();
            if (root.GetComponent<CharacterDetailRuntimePanel>() == null)
                root.AddComponent<CharacterDetailRuntimePanel>();
            if (root.GetComponent<DynamicEventRuntimePopup>() == null)
                root.AddComponent<DynamicEventRuntimePopup>();
            if (root.GetComponent<EnhanceRuntimePanel>() == null)
                root.AddComponent<EnhanceRuntimePanel>();
            if (root.GetComponent<GuildFacilityRuntimePanel>() == null)
                root.AddComponent<GuildFacilityRuntimePanel>();
            if (root.GetComponent<GuildHudTabController>() == null)
                root.AddComponent<GuildHudTabController>();
            if (root.GetComponent<ExplorationStartRuntimePanel>() == null)
                root.AddComponent<ExplorationStartRuntimePanel>();
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

        private static void WireLogItemView(ExplorationLogItemView view, Text messageText, Image categoryIcon)
        {
            var so = new SerializedObject(view);
            so.FindProperty("_messageText").objectReferenceValue = messageText;
            so.FindProperty("_categoryIcon").objectReferenceValue = categoryIcon;
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
            so.ApplyModifiedPropertiesWithoutUndo();
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

        private static void AnchorLeftColumn(RectTransform rect, float width)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(ExplorationHudLayoutMetrics.HorizontalPadding, 0f);
            rect.sizeDelta = new Vector2(width, 0f);
        }

        private static void AnchorRightColumn(RectTransform rect, float width)
        {
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(-ExplorationHudLayoutMetrics.HorizontalPadding, 0f);
            rect.sizeDelta = new Vector2(width, 0f);
        }

        private static void AnchorCenterColumn(RectTransform rect, float leftWidth, float rightWidth)
        {
            var leftInset = ExplorationHudLayoutMetrics.HorizontalPadding + leftWidth + ExplorationHudLayoutMetrics.ColumnGap;
            var rightInset = ExplorationHudLayoutMetrics.HorizontalPadding + rightWidth + ExplorationHudLayoutMetrics.ColumnGap;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(leftInset, 0f);
            rect.offsetMax = new Vector2(-rightInset, 0f);
        }

        private static Text CreateText(
            Transform parent,
            string name,
            Font font,
            int fontSize,
            Vector2 anchoredPos,
            Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            var text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.UpperLeft;
            text.color = Color.white;
            text.supportRichText = true;
            return text;
        }

        private static CommonButton CreateButton(
            Transform parent,
            string name,
            Font font,
            string label,
            Vector2 anchoredPos)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(CommonButton));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(104f, 32f);
            go.GetComponent<Image>().color = new Color(0.18f, 0.22f, 0.3f, 0.95f);

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(go.transform, false);
            StretchFull(labelGo.GetComponent<RectTransform>());
            var text = labelGo.AddComponent<Text>();
            text.font = font;
            text.fontSize = 14;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = label;
            return go.GetComponent<CommonButton>();
        }
    }
}
#endif
