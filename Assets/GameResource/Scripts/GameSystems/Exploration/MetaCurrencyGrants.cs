using Backend.GameSystems.Exploration.Data;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 명성·유물조각 등 메타 연동 런 재화 지급.
    /// </summary>
    public static class MetaCurrencyGrants
    {
        public static int GrantZoneClear(ExplorationState state, string completedZoneId)
        {
            if (state == null || string.IsNullOrEmpty(completedZoneId))
                return 0;

            var gain = ZoneDefinitions.GetZoneClearReputationBonus(completedZoneId);
            state.Reputation += gain;
            return gain;
        }

        public static int GrantDiscovery(ExplorationState state, ZoneDefinitions.DiscoveryDefinition discovery)
        {
            if (state == null || string.IsNullOrEmpty(discovery.ItemId))
                return 0;

            var gain = ZoneDefinitions.GetRelicFragmentQuantity(discovery);
            if (gain <= 0)
                return 0;

            state.RelicFragments += gain;
            return gain;
        }

        public static int GrantFactionReputation(ExplorationState state, int amount = 1)
        {
            if (state == null || amount <= 0)
                return 0;

            state.Reputation += amount;
            return amount;
        }
    }
}
