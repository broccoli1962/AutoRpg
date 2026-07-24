using Backend.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 하단 탭바: 탐험 | 강화/장비 | 길드시설 | 연대기 | 도감 (12_UIUX.md).
    /// </summary>
    public sealed class GuildHudTabController : CachedMonobehaviour
    {
        private const float TabBarHeight = ExplorationHudLayoutMetrics.TabBarHeight;

        public static float BottomInsetPx => ExplorationHudLayoutMetrics.BottomInsetPx;

        private enum HudBottomTab
        {
            Explore,
            Enhance,
            Guild,
            Chronicle,
            Compendium
        }

        private readonly Color _activeColor = new(0.28f, 0.42f, 0.62f, 0.98f);
        private readonly Color _inactiveColor = new(0.14f, 0.16f, 0.22f, 0.95f);

        private HudBottomTab _currentTab = HudBottomTab.Explore;
        private ChronicleRuntimePanel _chroniclePanel;
        private EnhanceRuntimePanel _enhancePanel;
        private GuildFacilityRuntimePanel _guildPanel;
        private Button[] _tabButtons;
        private System.Action _onRefreshStatus;

        public void Initialize(
            ChronicleRuntimePanel chroniclePanel,
            EnhanceRuntimePanel enhancePanel,
            GuildFacilityRuntimePanel guildPanel,
            System.Action onRefreshStatus)
        {
            _chroniclePanel = chroniclePanel;
            _enhancePanel = enhancePanel;
            _guildPanel = guildPanel;
            _onRefreshStatus = onRefreshStatus;
            BuildTabBar();
            SelectTab(HudBottomTab.Explore);
        }

        private void Start()
        {
            if (_tabButtons == null || !HasAnyWiredTab())
                BuildTabBar();
        }

        private bool HasAnyWiredTab()
        {
            if (_tabButtons == null)
                return false;

            for (var i = 0; i < _tabButtons.Length; i++)
            {
                if (_tabButtons[i] != null)
                    return true;
            }

            return false;
        }

        private void Update()
        {
            if (_currentTab != HudBottomTab.Compendium ||
                _chroniclePanel == null ||
                !_chroniclePanel.IsVisible)
            {
                return;
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha4, KeyCode.Keypad4))
                _chroniclePanel.OpenTab(ChronicleRuntimePanel.ChroniclePanelTab.LoreCompendium);

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha5, KeyCode.Keypad5))
                _chroniclePanel.OpenTab(ChronicleRuntimePanel.ChroniclePanelTab.MonsterCompendium);
        }

        private void BuildTabBar()
        {
            var existingBar = transform.Find("BottomTabBar");
            if (existingBar == null)
            {
                Debug.LogError("[GuildHudTabController] Prefab BottomTabBar missing. Bake tabs via Unity MCP.");
                return;
            }

            var existingRect = existingBar.GetComponent<RectTransform>();
            if (existingRect != null)
            {
                existingRect.anchoredPosition = Vector2.zero;
                existingRect.sizeDelta = new Vector2(0f, TabBarHeight);
            }

            WireExistingTabButtons(existingBar);
        }

        private void WireExistingTabButtons(Transform barRoot)
        {
            var labels = new[] { "탐험", "강화/장비", "길드시설", "연대기", "도감" };
            _tabButtons = new Button[labels.Length];
            var tabsRoot = barRoot.Find("Tabs") ?? barRoot;

            for (var i = 0; i < labels.Length; i++)
            {
                var tabTransform = tabsRoot.Find($"Tab_{labels[i]}");
                if (tabTransform == null)
                    continue;

                var tabIndex = i;
                _tabButtons[i] = tabTransform.GetComponent<Button>();
                if (_tabButtons[i] == null)
                    continue;

                _tabButtons[i].onClick.RemoveAllListeners();
                _tabButtons[i].onClick.AddListener(() => SelectTab((HudBottomTab)tabIndex));
                StyleTabButton(_tabButtons[i], (int)_currentTab == i);
                StyleTabLabel(tabTransform, (int)_currentTab == i);
            }

            barRoot.SetAsLastSibling();
        }

        private void SelectTab(HudBottomTab tab)
        {
            _currentTab = tab;
            _enhancePanel?.Hide();
            _guildPanel?.Hide();
            _chroniclePanel?.ClosePanel();

            switch (tab)
            {
                case HudBottomTab.Enhance:
                    _enhancePanel?.Show();
                    break;
                case HudBottomTab.Guild:
                    _guildPanel?.Show();
                    break;
                case HudBottomTab.Chronicle:
                    _chroniclePanel?.OpenTab(ChronicleRuntimePanel.ChroniclePanelTab.Runs);
                    break;
                case HudBottomTab.Compendium:
                    _chroniclePanel?.OpenTab(ChronicleRuntimePanel.ChroniclePanelTab.LoreCompendium);
                    break;
            }

            UpdateTabHighlight();
            _onRefreshStatus?.Invoke();

            var startPanel = GetComponentInChildren<ExplorationStartRuntimePanel>(true);
            startPanel?.SetGuildTabActive(tab == HudBottomTab.Explore);
        }

        private void UpdateTabHighlight()
        {
            if (_tabButtons == null)
                return;

            for (var i = 0; i < _tabButtons.Length; i++)
            {
                if (_tabButtons[i] == null)
                    continue;

                StyleTabButton(_tabButtons[i], (int)_currentTab == i);
                StyleTabLabel(_tabButtons[i].transform, (int)_currentTab == i);
            }
        }

        private static void StyleTabLabel(Transform tabRoot, bool active)
        {
            var label = tabRoot.Find("Content/Label") ?? tabRoot.Find("Label");
            if (label == null)
                return;

            var text = label.GetComponent<TextMeshProUGUI>();
            if (text != null)
                text.color = active ? ModernUiStyle.TitleGold : ModernUiStyle.BodyText;
        }

        private void StyleTabButton(Button button, bool active)
        {
            if (button == null)
                return;

            var image = button.GetComponent<Image>();
            if (image != null)
                StyleTabButtonImage(image, active);
        }

        private void StyleTabButtonImage(Image image, bool active)
        {
            RuntimeUiSprites.ApplyTabBackground(image, active);
            if (image.sprite == null)
                image.color = active ? _activeColor : _inactiveColor;
        }
    }
}
