using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Narration;

namespace Backend.GameSystems.LLM
{
    /// <summary>
    /// 기획서 05_자동탐험로그.md Salience 등급에 따라 LLM/템플릿 분기를 결정한다.
    /// Trivial·Notable → 템플릿, Significant·Milestone → LLM (지원 이벤트 타입만).
    /// </summary>
    public static class NarrationSalienceRouter
    {
        public static bool ShouldUseLlm(ExplorationEvent explorationEvent, PartyState party = null)
        {
            if (explorationEvent == null)
                return false;

            var minimumSalience = SalienceGrade.Significant;
            if (ExplorationNarrationBonus.PartyHasBard(party))
                minimumSalience = SalienceGrade.Notable;

            if (explorationEvent.Salience < minimumSalience)
                return false;

            if (!LlmQualitySettings.ShouldUseLogLlm(explorationEvent))
                return false;

            return explorationEvent.EventType switch
            {
                EventType.CombatResult => true,
                EventType.Discovery => true,
                EventType.Trap => true,
                EventType.Injury => true,
                EventType.Death => true,
                EventType.FloorClear => true,
                _ => false
            };
        }
    }
}
