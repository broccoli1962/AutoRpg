using Backend.GameSystems.Exploration.Data;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 성직자 등 파티 역할에 따른 생존·회복 보너스 (07_캐릭터시스템.md).
    /// </summary>
    public static class ExplorationSurvivalBonus
    {
        private const float ClericRestHealMultiplier = 1.25f;
        private const float WarriorClericRetreatBonus = 0.05f;
        private const float WarriorClericDamageReduction = 0.92f;
        private const float ClericInjuryChance = 0.08f;
        private const float BaseInjuryChance = 0.12f;

        public static bool PartyHasCleric(PartyState party)
        {
            if (party?.Members == null)
                return false;

            foreach (var member in party.Members)
            {
                if (member != null && member.Role == CharacterRole.Cleric)
                    return true;
            }

            return false;
        }

        public static bool PartyHasWarriorClericSynergy(PartyState party)
        {
            if (party?.Members == null)
                return false;

            var hasWarrior = false;
            var hasCleric = false;

            foreach (var member in party.Members)
            {
                if (member == null || member.CurrentHp <= 0)
                    continue;

                if (member.Role == CharacterRole.Warrior)
                    hasWarrior = true;
                else if (member.Role == CharacterRole.Cleric)
                    hasCleric = true;
            }

            return hasWarrior && hasCleric;
        }

        public static float GetRestHealMultiplier(PartyState party)
        {
            return PartyHasCleric(party) ? ClericRestHealMultiplier : 1f;
        }

        public static float GetRetreatHpThreshold(PartyState party, float baseThreshold)
        {
            return PartyHasWarriorClericSynergy(party)
                ? baseThreshold + WarriorClericRetreatBonus
                : baseThreshold;
        }

        public static float GetIncomingDamageMultiplier(PartyState party)
        {
            return PartyHasWarriorClericSynergy(party) ? WarriorClericDamageReduction : 1f;
        }

        public static float GetInjuryRollChance(PartyState party)
        {
            return PartyHasCleric(party) ? ClericInjuryChance : BaseInjuryChance;
        }

        public static float GetInjuryRecoveryThresholdBonus(PartyState party)
        {
            return PartyHasCleric(party) ? 0.1f : 0f;
        }
    }
}
