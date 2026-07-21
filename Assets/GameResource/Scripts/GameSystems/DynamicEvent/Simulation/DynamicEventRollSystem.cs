using Backend.GameSystems.DynamicEvent.Data;
using Backend.GameSystems.Exploration.Simulation;

namespace Backend.GameSystems.DynamicEvent.Simulation
{
    public static class DynamicEventRollSystem
    {
        public static DynamicEventTemplate TryRollFloorEnter(string zoneId, int floor, DeterministicRandom random)
        {
            foreach (var template in DynamicEventDefinitions.All)
            {
                if (template.Trigger.Type != DynamicEventTriggerType.FloorEnter)
                    continue;

                if (template.Trigger.ZoneIds.Count > 0 && !template.Trigger.ZoneIds.Contains(zoneId))
                    continue;

                if (floor < template.Trigger.MinFloor || floor > template.Trigger.MaxFloor)
                    continue;

                if (random.RollChance(template.Trigger.Probability))
                    return template;
            }

            return null;
        }
    }
}
