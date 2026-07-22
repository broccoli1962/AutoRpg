using System.Collections.Generic;
using Backend.GameSystems.DynamicEvent.Data;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Simulation;

namespace Backend.GameSystems.DynamicEvent.Simulation
{
    public static class DynamicEventRollSystem
    {
        public static DynamicEventTemplate TryRollFloorEnter(string zoneId, int floor, DeterministicRandom random)
        {
            var golden = TryRollRareGolden(zoneId, floor, random);
            if (golden != null)
                return golden;

            foreach (var template in DynamicEventDefinitions.All)
            {
                if (!IsFloorEnterEligible(template, zoneId, floor))
                    continue;

                if (random.RollChance(template.Trigger.Probability))
                    return template;
            }

            return null;
        }

        private static DynamicEventTemplate TryRollRareGolden(string zoneId, int floor, DeterministicRandom random)
        {
            foreach (var template in DynamicEventDefinitions.All)
            {
                if (!IsRareGoldenEligible(template, zoneId, floor))
                    continue;

                if (random.RollChance(template.Trigger.Probability))
                    return template;
            }

            return null;
        }

        /// <summary>
        /// 확률 롤 없이 현재 층에서 발생 가능한 템플릿 중 하나를 고른다 (Tick 보장용).
        /// </summary>
        public static DynamicEventTemplate RollGuaranteed(string zoneId, int floor, DeterministicRandom random)
        {
            var eligible = new List<DynamicEventTemplate>();
            foreach (var template in DynamicEventDefinitions.All)
            {
                if (!IsGuaranteedEligible(template, zoneId, floor))
                    continue;

                eligible.Add(template);
            }

            if (eligible.Count == 0)
                return null;

            return eligible[random.NextInt(eligible.Count)];
        }

        private static bool IsFloorEnterEligible(DynamicEventTemplate template, string zoneId, int floor)
        {
            if (template.Trigger.Type != DynamicEventTriggerType.FloorEnter)
                return false;

            if (template.Trigger.ZoneIds.Count > 0 && !template.Trigger.ZoneIds.Contains(zoneId))
                return false;

            var relativeFloor = ZoneDefinitions.GetZoneRelativeFloor(zoneId, floor);
            return relativeFloor >= template.Trigger.MinFloor && relativeFloor <= template.Trigger.MaxFloor;
        }

        private static bool IsRareGoldenEligible(DynamicEventTemplate template, string zoneId, int floor)
        {
            if (template.Trigger.Type != DynamicEventTriggerType.RareGolden)
                return false;

            if (template.Trigger.ZoneIds.Count > 0 && !template.Trigger.ZoneIds.Contains(zoneId))
                return false;

            var relativeFloor = ZoneDefinitions.GetZoneRelativeFloor(zoneId, floor);
            return relativeFloor >= template.Trigger.MinFloor && relativeFloor <= template.Trigger.MaxFloor;
        }

        private static bool IsGuaranteedEligible(DynamicEventTemplate template, string zoneId, int floor)
        {
            if (template.Intensity == DynamicEventIntensity.Golden ||
                template.Trigger.Type == DynamicEventTriggerType.RareGolden)
            {
                return false;
            }

            return IsFloorEnterEligible(template, zoneId, floor);
        }
    }
}
