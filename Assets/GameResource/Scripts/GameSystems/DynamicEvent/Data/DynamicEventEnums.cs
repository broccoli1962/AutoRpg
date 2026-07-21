namespace Backend.GameSystems.DynamicEvent.Data
{
    public enum DynamicEventCategory
    {
        ForkChoice,
        Encounter,
        Hazard,
        Artifact,
        Faction,
        PersonalStory
    }

    public enum DynamicEventTriggerType
    {
        FloorEnter,
        ConditionBased,
        TickGuarantee,
        RareGolden
    }

    public enum DynamicEventIntensity
    {
        Standard,
        Golden
    }

    public enum DynamicEventOutcomeEffect
    {
        MinorResource,
        MinorTrapDamage,
        RareEncounter,
        SafePass,
        GoldBonus,
        InjuryLight
    }
}
