namespace Backend.Services.RemoteConfig
{
    /// <summary>
    /// Remote Config 키 상수.
    /// </summary>
    public static class RemoteConfigKeys
    {
        public const string MonsterHpGrowth = "balance_monster_hp_growth";
        public const string MonsterAtkGrowth = "balance_monster_atk_growth";
        public const string MonsterDefGrowth = "balance_monster_def_growth";
        public const string GoldDropGrowth = "balance_gold_drop_growth";
        public const string UpgradeCostGrowth = "balance_upgrade_cost_growth";

        public const string TotalRewardedDailyLimit = "ads_total_rewarded_daily_limit";
        public const string InterstitialDailyLimit = "ads_interstitial_daily_limit";

        public const string AchievementGlobalRewardMultiplier = "achievement_global_reward_multiplier";

        public const string EventSeasonPassEnabled = "event_season_pass_enabled";
        public const string EventGachaBannerEnabled = "event_gacha_banner_enabled";
    }
}
