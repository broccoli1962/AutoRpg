using Backend.GameSystems.Exploration.Data;

namespace Backend.GameSystems.Exploration.Narration
{
    /// <summary>
    /// 파티 역할(음유시인 등)에 따른 LLM·로그·동적 이벤트 보너스.
    /// </summary>
    public static class ExplorationNarrationBonus
    {
        private const int BardTokenBonus = 24;
        private const int BardSalienceReduction = 1;
        private const float BardEventRateMultiplier = 1.12f;

        public static bool PartyHasBard(PartyState party)
        {
            if (party?.Members == null)
                return false;

            foreach (var member in party.Members)
            {
                if (member != null && member.Role == CharacterRole.Bard)
                    return true;
            }

            return false;
        }

        public static int GetExtraLogMaxTokens(PartyState party) =>
            PartyHasBard(party) ? BardTokenBonus : 0;

        public static int GetSalienceGradeReduction(PartyState party) =>
            PartyHasBard(party) ? BardSalienceReduction : 0;

        public static float GetDynamicEventRateMultiplier(PartyState party) =>
            PartyHasBard(party) ? BardEventRateMultiplier : 1f;

        public static SalienceGrade LowerSalienceGrade(SalienceGrade grade, int steps)
        {
            for (var i = 0; i < steps; i++)
            {
                grade = grade switch
                {
                    SalienceGrade.Milestone => SalienceGrade.Significant,
                    SalienceGrade.Significant => SalienceGrade.Notable,
                    SalienceGrade.Notable => SalienceGrade.Trivial,
                    _ => SalienceGrade.Trivial
                };
            }

            return grade;
        }
    }
}
