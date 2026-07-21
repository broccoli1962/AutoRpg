using Backend.GameSystems.Exploration.Data;

namespace Backend.GameSystems.Exploration.Simulation
{
    public static class SalienceCalculator
    {
        public static SalienceGrade Calculate(
            EventType eventType,
            MonsterRarity? monsterRarity,
            bool isFirstDiscovery,
            float partyHpRatio,
            int recentRepeatCount)
        {
            var score = GetBaseWeight(eventType, monsterRarity);

            if (isFirstDiscovery)
                score += 8;

            if (partyHpRatio <= 0.35f)
                score += 10;

            if (recentRepeatCount >= 3)
                score -= 6;

            return score switch
            {
                >= 40 => SalienceGrade.Milestone,
                >= 25 => SalienceGrade.Significant,
                >= 12 => SalienceGrade.Notable,
                _ => SalienceGrade.Trivial
            };
        }

        private static int GetBaseWeight(EventType eventType, MonsterRarity? monsterRarity)
        {
            return eventType switch
            {
                EventType.Move => 2,
                EventType.CombatResult => monsterRarity switch
                {
                    MonsterRarity.Boss => 45,
                    MonsterRarity.Rare => 28,
                    MonsterRarity.Notable => 16,
                    _ => 8
                },
                EventType.Discovery => 14,
                EventType.Trap => 18,
                EventType.Rest => 6,
                EventType.Injury => 22,
                EventType.Death => 42,
                EventType.FloorClear => 35,
                EventType.OfflineSummary => 30,
                _ => 5
            };
        }
    }
}
