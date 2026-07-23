using Backend.AddressableKey;
using Backend.Object.Management;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Util
{
    /// <summary>
    /// GameUI v2 스프라이트 로드·적용. PPU 100, multiplier 1(기본).
    /// </summary>
    public static class RuntimeUiSprites
    {
        public const string UiPanelL = "ui_panel_l";
        public const string UiPanelS = "ui_panel_s";
        public const string UiBarTop = "ui_bar_top";
        public const string UiBarBottom = "ui_bar_bottom";
        public const string UiBtnPrimary = "ui_btn_primary";
        public const string UiBtnSecondary = "ui_btn_secondary";
        public const string UiTabOn = "ui_tab_on";
        public const string UiTabOff = "ui_tab_off";
        public const string UiProgressTrack = "ui_progress_track";
        public const string UiProgressFill = "ui_progress_fill";
        public const string UiHpTrack = "ui_hp_track";
        public const string UiHpFill = "ui_hp_fill";
        public const string UiOverlayDim = "ui_overlay_dim";
        public const string UiLogCard = "ui_log_card";
        public const string PortraitFrame = "portrait_frame";
        public const string IllustZoneBanner = "illust_zone_banner";
        public const string IllustGuildStart = "illust_guild_start";
        public const string LogAccentStrip = "log_accent_strip";

        public const string IconTabExplore = "icon_tab_explore";
        public const string IconTabEnhance = "icon_tab_enhance";
        public const string IconTabGuild = "icon_tab_guild";
        public const string IconTabChronicle = "icon_tab_chronicle";
        public const string IconTabCompendium = "icon_tab_compendium";
        public const string IconLogCombat = "icon_log_combat";
        public const string IconLogDiscovery = "icon_log_discovery";
        public const string IconLogEvent = "icon_log_event";
        public const string IconLogNarrative = "icon_log_narrative";

        public static Sprite Get(string keyName)
        {
            if (string.IsNullOrEmpty(keyName))
                return null;

            var address = AddressableKeys.InGame.Get(keyName);
            if (string.IsNullOrEmpty(address))
                return null;

            return ResourceManager.LoadResource<Sprite>(address);
        }

        public static void ApplyPanelLarge(Image image) =>
            ApplySlicedSprite(image, Get(UiPanelL), Color.white);

        public static void ApplyPanelSmall(Image image) =>
            ApplySlicedSprite(image, Get(UiPanelS), Color.white);

        public static void ApplyHeaderBar(Image image) =>
            ApplySlicedSprite(image, Get(UiBarTop), Color.white);

        public static void ApplyTabBarBackground(Image image) =>
            ApplySlicedSprite(image, Get(UiBarBottom), Color.white);

        public static void ApplyPrimaryButton(Image image) =>
            ApplySlicedSprite(image, Get(UiBtnPrimary), Color.white);

        public static void ApplySecondaryButton(Image image) =>
            ApplySlicedSprite(image, Get(UiBtnSecondary), Color.white);

        public static void ApplyTabBackground(Image image, bool active) =>
            ApplySlicedSprite(image, Get(active ? UiTabOn : UiTabOff), Color.white);

        public static void ApplyLogCard(Image image)
        {
            var sprite = Get(UiLogCard);
            if (sprite != null)
                ApplySlicedSprite(image, sprite, Color.white);
            else
                ApplyPanelSmall(image);
        }

        public static void ApplyPortraitFrame(Image image) =>
            ApplySimpleImage(image, PortraitFrame, Color.white);

        public static void ApplyProgressTrack(Image image) =>
            ApplySlicedSprite(image, Get(UiProgressTrack), Color.white);

        public static void ApplyProgressFill(Image image)
        {
            ApplySlicedSprite(image, Get(UiProgressFill), Color.white);
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
        }

        public static void ApplyHpTrack(Image image) =>
            ApplySlicedSprite(image, Get(UiHpTrack), Color.white);

        public static void ApplyHpFill(Image image)
        {
            ApplySlicedSprite(image, Get(UiHpFill), Color.white);
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
        }

        public static void ApplyOverlayDim(Image image) =>
            ApplySlicedSprite(image, Get(UiOverlayDim), Color.white);

        public static void ApplySimpleImage(Image image, string keyName, Color color)
        {
            if (image == null)
                return;

            var sprite = Get(keyName);
            if (sprite == null)
                return;

            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.color = color;
        }

        public static void ApplySlicedSprite(Image image, Sprite sprite, Color color)
        {
            if (image == null || sprite == null)
                return;

            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = color;
        }

        public static string GetLogIconKey(LogIconCategory category) =>
            category switch
            {
                LogIconCategory.Combat => IconLogCombat,
                LogIconCategory.Discovery => IconLogDiscovery,
                LogIconCategory.Event => IconLogEvent,
                _ => IconLogNarrative
            };

        public enum LogIconCategory
        {
            Narrative,
            Combat,
            Discovery,
            Event
        }
    }
}
