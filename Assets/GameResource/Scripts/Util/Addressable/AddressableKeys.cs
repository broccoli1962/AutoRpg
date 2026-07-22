// Auto Generate Code — v2 GameUI keys added manually.
using System.Collections.Generic;

namespace Backend.AddressableKey
{
    public static class AddressableKeys
    {
        public static class UI
        {
            private static readonly Dictionary<string, string> Keys = new Dictionary<string, string>()
            {
                { "UIRoot", "UI/UIRoot.prefab" },
                { "UI_ExplorationHudPanel_prefab", "UI/ExplorationHudPanel.prefab" },
                { "ExplorationHudPanel", "UI/ExplorationHudPanel.prefab" },
            };

            public static string Get<T>() => Keys.TryGetValue(typeof(T).Name, out var key) ? key : null;
            public static string Get(string keyName) => Keys.TryGetValue(keyName, out var key) ? key : null;
        }

        public static class Sounds
        {
            private static readonly Dictionary<string, string> Keys = new Dictionary<string, string>()
            {
                { "AudioMixer", "Assets/GameResource/Sounds/AudioMixer.mixer" },
            };

            public static string Get<T>() => Keys.TryGetValue(typeof(T).Name, out var key) ? key : null;
            public static string Get(string keyName) => Keys.TryGetValue(keyName, out var key) ? key : null;
        }

        public static class InGame
        {
            private const string V2 = "Assets/GameResource/Images/GameUI/v2/";

            private static readonly Dictionary<string, string> Keys = new Dictionary<string, string>()
            {
                { "BMJUA", "Assets/Fonts/BMJUA_ttf.ttf" },
                { "ui_panel_l", V2 + "ui_panel_l.png" },
                { "ui_panel_s", V2 + "ui_panel_s.png" },
                { "ui_bar_top", V2 + "ui_bar_top.png" },
                { "ui_bar_bottom", V2 + "ui_bar_bottom.png" },
                { "ui_btn_primary", V2 + "ui_btn_primary.png" },
                { "ui_btn_secondary", V2 + "ui_btn_secondary.png" },
                { "ui_tab_on", V2 + "ui_tab_on.png" },
                { "ui_tab_off", V2 + "ui_tab_off.png" },
                { "ui_progress_track", V2 + "ui_progress_track.png" },
                { "ui_progress_fill", V2 + "ui_progress_fill.png" },
                { "portrait_frame", V2 + "portrait_frame.png" },
                { "illust_zone_banner", V2 + "illust_zone_banner.png" },
                { "illust_guild_start", V2 + "illust_guild_start.png" },
                { "log_accent_strip", V2 + "log_accent_strip.png" },
                { "icon_tab_explore", V2 + "icon_tab_explore.png" },
                { "icon_tab_enhance", V2 + "icon_tab_enhance.png" },
                { "icon_tab_guild", V2 + "icon_tab_guild.png" },
                { "icon_tab_chronicle", V2 + "icon_tab_chronicle.png" },
                { "icon_tab_compendium", V2 + "icon_tab_compendium.png" },
                { "icon_log_combat", V2 + "icon_log_combat.png" },
                { "icon_log_discovery", V2 + "icon_log_discovery.png" },
                { "icon_log_event", V2 + "icon_log_event.png" },
                { "icon_log_narrative", V2 + "icon_log_narrative.png" },
            };

            public static string Get<T>() => Keys.TryGetValue(typeof(T).Name, out var key) ? key : null;
            public static string Get(string keyName) => Keys.TryGetValue(keyName, out var key) ? key : null;
        }
    }
}
