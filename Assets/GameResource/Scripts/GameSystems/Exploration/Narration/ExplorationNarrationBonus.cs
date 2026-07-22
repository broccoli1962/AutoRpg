using Backend.GameSystems.Exploration.Data;
using ScriptoriumManager = Backend.GameSystems.Exploration.ScriptoriumManager;
using SkillTreeManager = Backend.GameSystems.Exploration.SkillTreeManager;

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

        public static int GetExtraLogMaxTokens(PartyState party)
        {
            var bonus = PartyHasBard(party) ? BardTokenBonus : 0;
            bonus += ScriptoriumManager.GetTokenBonus();
            bonus += SkillTreeManager.GetBardTokenBonus(party);
            return bonus;
        }

        public static int GetSalienceGradeReduction(PartyState party)
        {
            var reduction = PartyHasBard(party) ? BardSalienceReduction : 0;
            reduction += ScriptoriumManager.GetSalienceGradeReduction();
            return reduction;
        }

        public static float GetDynamicEventRateMultiplier(PartyState party)
        {
            var multiplier = PartyHasBard(party) ? BardEventRateMultiplier : 1f;
            multiplier *= ScriptoriumManager.GetEventRateMultiplier();
            return multiplier;
        }

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
