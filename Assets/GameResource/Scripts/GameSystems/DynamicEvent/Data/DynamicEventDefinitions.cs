using System.Collections.Generic;
using Backend.GameSystems.Exploration.Data;

namespace Backend.GameSystems.DynamicEvent.Data
{
    public static class DynamicEventDefinitions
    {
        public const string Fork002Id = "fork_002";

        private static readonly DynamicEventTemplate Fork002 = new()
        {
            EventId = Fork002Id,
            Category = DynamicEventCategory.ForkChoice,
            Intensity = DynamicEventIntensity.Standard,
            Trigger = new DynamicEventTrigger
            {
                Type = DynamicEventTriggerType.FloorEnter,
                ZoneIds = new List<string> { ZoneDefinitions.MossyHollowId },
                Probability = 0.12f,
                MinFloor = 1,
                MaxFloor = 15
            },
            Choices = new List<DynamicEventChoice>
            {
                new()
                {
                    Id = "left_path",
                    EffectPool = new Dictionary<DynamicEventOutcomeEffect, float>
                    {
                        { DynamicEventOutcomeEffect.MinorResource, 0.6f },
                        { DynamicEventOutcomeEffect.MinorTrapDamage, 0.4f }
                    }
                },
                new()
                {
                    Id = "right_path",
                    EffectPool = new Dictionary<DynamicEventOutcomeEffect, float>
                    {
                        { DynamicEventOutcomeEffect.RareEncounter, 0.35f },
                        { DynamicEventOutcomeEffect.SafePass, 0.65f }
                    }
                }
            }
        };

        public static IReadOnlyList<DynamicEventTemplate> All { get; } = new List<DynamicEventTemplate> { Fork002 };

        public static DynamicEventTemplate Get(string eventId)
        {
            foreach (var template in All)
            {
                if (template.EventId == eventId)
                    return template;
            }

            return null;
        }
    }
}
