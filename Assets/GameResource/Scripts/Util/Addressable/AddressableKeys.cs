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
                { "sfx_button", "Assets/GameResource/Sounds/sfx_button.wav" },
                { "popSound", "Assets/GameResource/Sounds/popSound.wav" },
            };

            public static string Get<T>() => Keys.TryGetValue(typeof(T).Name, out var key) ? key : null;
            public static string Get(string keyName) => Keys.TryGetValue(keyName, out var key) ? key : null;
        }

        public static class InGame
        {
            private const string V2 = "Assets/GameResource/Images/GameUI/v2/";
            private const string Stage = "Assets/GameResource/Images/GameUI/stage/";

            private static readonly Dictionary<string, string> Keys = new Dictionary<string, string>()
            {
                { "BMJUA", "Assets/Fonts/BMJUA_ttf.ttf" },
                { "BMJUA_SDF", "Assets/Fonts/BMJUA_ttf SDF.asset" },
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
                { "ui_hp_track", V2 + "ui_hp_track.png" },
                { "ui_hp_fill", V2 + "ui_hp_fill.png" },
                { "ui_overlay_dim", V2 + "ui_overlay_dim.png" },
                { "ui_log_card", V2 + "ui_log_card.png" },
                { "stage_party_warrior", Stage + "stage_party_warrior.png" },
                { "stage_party_rogue", Stage + "stage_party_rogue.png" },
                { "stage_party_mage", Stage + "stage_party_mage.png" },
                { "stage_party_cleric", Stage + "stage_party_cleric.png" },
                { "stage_party_bard", Stage + "stage_party_bard.png" },
                { "stage_monster_slime", Stage + "stage_monster_slime.png" },
                { "stage_monster_beast", Stage + "stage_monster_beast.png" },
                { "stage_monster_elite", Stage + "stage_monster_elite.png" },
                { "stage_monster_boss", Stage + "stage_monster_boss.png" },
                { "stage_vfx_slash", Stage + "stage_vfx_slash.png" },
                { "AudioSource", "Assets/GameResource/Prefabs/AudioSource.prefab" },
            };

            public static string Get<T>() => Keys.TryGetValue(typeof(T).Name, out var key) ? key : null;
            public static string Get(string keyName) => Keys.TryGetValue(keyName, out var key) ? key : null;
        }
    }
}
