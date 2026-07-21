using Backend.GameSystems.DynamicEvent.Data;
using Backend.GameSystems.Exploration.Simulation;

namespace Backend.GameSystems.DynamicEvent.Simulation
{
    public static class DynamicEventResolver
    {
        public static DynamicEventOutcomeEffect ResolveChoice(
            DynamicEventTemplate template,
            string choiceId,
            DeterministicRandom random)
        {
            foreach (var choice in template.Choices)
            {
                if (choice.Id != choiceId)
                    continue;

                return RollEffectPool(choice.EffectPool, random);
            }

            return DynamicEventOutcomeEffect.SafePass;
        }

        private static DynamicEventOutcomeEffect RollEffectPool(
            System.Collections.Generic.Dictionary<DynamicEventOutcomeEffect, float> effectPool,
            DeterministicRandom random)
        {
            var roll = random.NextFloat();
            var cumulative = 0f;

            foreach (var pair in effectPool)
            {
                cumulative += pair.Value;
                if (roll <= cumulative)
                    return pair.Key;
            }

            foreach (var pair in effectPool)
                return pair.Key;

            return DynamicEventOutcomeEffect.SafePass;
        }
    }
}
