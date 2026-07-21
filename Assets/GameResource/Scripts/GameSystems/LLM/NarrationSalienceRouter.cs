using Backend.GameSystems.Exploration.Data;

namespace Backend.GameSystems.LLM
{
    /// <summary>
    /// 기획서 05_자동탐험로그.md Salience 등급에 따라 LLM/템플릿 분기를 결정한다.
    /// Trivial·Notable → 템플릿, Significant·Milestone → LLM (지원 이벤트 타입만).
    /// </summary>
    public static class NarrationSalienceRouter
    {
        public static bool ShouldUseLlm(ExplorationEvent explorationEvent)
        {
            if (explorationEvent == null || explorationEvent.Salience < SalienceGrade.Significant)
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
