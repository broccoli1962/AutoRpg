namespace Backend.GameSystems.Exploration.Data
{
    public enum CharacterRole
    {
        Warrior,
        Rogue,
        Mage,
        Bard,
        Cleric
    }

    public enum PersonalityTag
    {
        Cautious,
        Greedy,
        Reckless,
        Cheerful,
        Loyal,
        Cynical
    }

    public enum InjurySeverity
    {
        None,
        Light,
        Moderate,
        Severe,
        Fatal
    }

    public enum EventType
    {
        Move,
        CombatResult,
        Discovery,
        Trap,
        Rest,
        Injury,
        Death,
        FloorClear,
        ZoneTransition,
        OfflineSummary
    }

    public enum SalienceGrade
    {
        Trivial,
        Notable,
        Significant,
        Milestone
    }

    public enum CombatOutcome
    {
        Victory,
        Defeat,
        Retreat
    }

    public enum MonsterRarity
    {
        Common,
        Notable,
        Rare,
        Boss
    }

    public enum LogCategory
    {
        Move,
        Combat,
        Discovery,
        Status,
        Milestone
    }
}
