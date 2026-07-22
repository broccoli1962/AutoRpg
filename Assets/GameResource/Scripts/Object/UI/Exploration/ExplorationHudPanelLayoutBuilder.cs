using Backend.GameSystems.Exploration;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Object.UI.Exploration
{
    /// <summary>
    /// Addressable 프리팹이 비어 있어도 ExplorationHudPanel을 구성할 수 있게 런타임 UI를 생성한다.
    /// </summary>
    internal static class ExplorationHudPanelLayoutBuilder
    {
        public static bool TryBuild(ExplorationHudPanel panel)
        {
            if (panel == null || panel.LogFeedView != null)
                return false;

            var rootRect = panel.GetComponent<RectTransform>();
            if (rootRect == null)
                rootRect = panel.gameObject.AddComponent<RectTransform>();

            StretchFull(rootRect);

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var statusText = CreateText(rootRect, "ZoneFloorText", new Vector2(24f, -20f), new Vector2(920f, 56f), 17, font);
            statusText.supportRichText = true;
            var goldText = CreateText(rootRect, "GoldText", new Vector2(24f, -72f), new Vector2(220f, 22f), 16, font);
            goldText.gameObject.SetActive(false);

            var helpText = CreateText(rootRect, "HelpText", new Vector2(24f, -88f), new Vector2(920f, 18f), 13, font);
            helpText.text = "L:LLM  A:이벤트  G:황금  O:설정  C:연대기  R:귀환  F:필터  B:북마크  -:스킬  [/]:로그  하단탭";
            var filterText = CreateText(rootRect, "FilterText", new Vector2(24f, -108f), new Vector2(420f, 18f), 13, font);
            filterText.color = new Color(0.8f, 0.8f, 0.85f);

            var pauseButton = CreateButton(rootRect, "PauseButton", new Vector2(24f, -132f), "일시정지", font);
            var resumeButton = CreateButton(rootRect, "ResumeButton", new Vector2(140f, -132f), "재개", font);
            var returnButton = CreateButton(rootRect, "ReturnButton", new Vector2(256f, -132f), "귀환", font);

            var logFeedGo = new GameObject("LogFeedView");
            logFeedGo.transform.SetParent(rootRect, false);
            var logFeedRect = logFeedGo.AddComponent<RectTransform>();
            StretchFull(logFeedRect);
            logFeedRect.offsetMin = new Vector2(
                ExplorationHudLayoutMetrics.RightPanelLeft,
                GuildHudTabController.BottomInsetPx);
            logFeedRect.offsetMax = new Vector2(
                -ExplorationHudLayoutMetrics.HorizontalPadding,
                -ExplorationHudLayoutMetrics.TopBarHeight);

            var logFeedView = logFeedGo.AddComponent<ExplorationLogFeedView>();
            var scrollRect = logFeedGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            var viewportGo = new GameObject("Viewport");
            viewportGo.transform.SetParent(logFeedGo.transform, false);
            var viewportRect = viewportGo.AddComponent<RectTransform>();
            StretchFull(viewportRect);
            viewportGo.AddComponent<Mask>().showMaskGraphic = false;
            var viewportImage = viewportGo.AddComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.15f);
            scrollRect.viewport = viewportRect;

            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRect = contentGo.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 0f);
            var layout = contentGo.AddComponent<VerticalLayoutGroup>();
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 6f;
            contentGo.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.content = contentRect;

            var itemTemplateGo = new GameObject("LogItemTemplate");
            itemTemplateGo.transform.SetParent(logFeedGo.transform, false);
            itemTemplateGo.SetActive(false);
            var itemView = itemTemplateGo.AddComponent<ExplorationLogItemView>();
            var itemLayout = itemTemplateGo.AddComponent<LayoutElement>();
            itemLayout.minHeight = 28f;
            itemLayout.preferredHeight = 28f;

            var itemTextGo = new GameObject("Message");
            itemTextGo.transform.SetParent(itemTemplateGo.transform, false);
            var itemTextRect = itemTextGo.AddComponent<RectTransform>();
            StretchFull(itemTextRect);
            var itemText = itemTextGo.AddComponent<Text>();
            itemText.font = font;
            itemText.fontSize = 15;
            itemText.color = Color.white;
            itemText.supportRichText = true;
            itemText.horizontalOverflow = HorizontalWrapMode.Wrap;
            itemText.verticalOverflow = VerticalWrapMode.Overflow;
            itemView.ConfigureRuntime(itemText, null);

            logFeedView.ConfigureRuntime(scrollRect, contentRect, itemView);

            CreateText(
                rootRect,
                "LogHeader",
                new Vector2(ExplorationHudLayoutMetrics.RightPanelLeft, -ExplorationHudLayoutMetrics.TopBarHeight + 8f),
                new Vector2(ExplorationHudLayoutMetrics.RightPanelWidth - 16f, 20f),
                14,
                font).text = "[ 로그 ]";

            var chroniclePanel = panel.gameObject.GetComponent<ChronicleRuntimePanel>();
            if (chroniclePanel == null)
                chroniclePanel = panel.gameObject.AddComponent<ChronicleRuntimePanel>();

            var settingsPanel = panel.gameObject.GetComponent<ExplorationSettingsRuntimePanel>();
            if (settingsPanel == null)
                settingsPanel = panel.gameObject.AddComponent<ExplorationSettingsRuntimePanel>();

            if (panel.gameObject.GetComponent<PartyRuntimePanel>() == null)
                panel.gameObject.AddComponent<PartyRuntimePanel>();

            if (panel.gameObject.GetComponent<ExplorationCenterRuntimePanel>() == null)
                panel.gameObject.AddComponent<ExplorationCenterRuntimePanel>();

            if (panel.gameObject.GetComponent<Backend.GameSystems.DynamicEvent.DynamicEventRuntimePopup>() == null)
                panel.gameObject.AddComponent<Backend.GameSystems.DynamicEvent.DynamicEventRuntimePopup>();

            var enhancePanel = panel.gameObject.GetComponent<EnhanceRuntimePanel>();
            if (enhancePanel == null)
                enhancePanel = panel.gameObject.AddComponent<EnhanceRuntimePanel>();

            var guildPanel = panel.gameObject.GetComponent<GuildFacilityRuntimePanel>();
            if (guildPanel == null)
                guildPanel = panel.gameObject.AddComponent<GuildFacilityRuntimePanel>();

            var tabController = panel.gameObject.GetComponent<GuildHudTabController>();
            if (tabController == null)
                tabController = panel.gameObject.AddComponent<GuildHudTabController>();

            panel.ConfigureRuntime(
                statusText,
                goldText,
                null,
                null,
                pauseButton,
                resumeButton,
                returnButton,
                logFeedView,
                filterText,
                helpText,
                chroniclePanel,
                settingsPanel);

            return true;
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Text CreateText(
            RectTransform parent,
            string name,
            Vector2 anchoredPos,
            Vector2 size,
            int fontSize,
            Font font)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
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
            RectTransform parent,
            string name,
            Vector2 anchoredPos,
            string label,
            Font font)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(104f, 32f);

            go.AddComponent<Image>().color = new Color(0.18f, 0.22f, 0.3f, 0.95f);
            var button = go.AddComponent<CommonButton>();

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            StretchFull(labelRect);
            var text = labelGo.AddComponent<Text>();
            text.font = font;
            text.fontSize = 14;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = label;
            return button;
        }
    }
}
