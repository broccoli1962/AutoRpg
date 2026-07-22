using System;
using System.Collections.Generic;

namespace Backend.GameSystems.Exploration.Data
{
    [Serializable]
    public sealed class CharacterState
    {
        public string CharacterId;
        public string DisplayName;
        public CharacterRole Role;
        public int Level = 1;
        public int Str;
        public int Agi;
        public int Int;
        public int Vit;
        public int Luk;
        public int CurrentHp;
        public int MaxHp;
        public List<PersonalityTag> PersonalityTags = new();
        public InjurySeverity Injury = InjurySeverity.None;
        public string EquippedWeaponId;
        public string EquippedArmorId;
    }

    [Serializable]
    public sealed class PartyState
    {
        public List<CharacterState> Members = new();

        public CharacterState Leader =>
            Members.Count > 0 ? Members[0] : null;
    }

    [Serializable]
    public sealed class LootEntry
    {
        public string ItemId;
        public int Quantity;
    }

    [Serializable]
    public sealed class CombatInjuryEntry
    {
        public string CharacterId;
        public InjurySeverity Severity;
    }

    [Serializable]
    public sealed class CombatResultPayload
    {
        public List<string> Party = new();
        public List<string> EnemyGroup = new();
        public CombatOutcome Outcome;
        public int DurationTicks;
        public int DamageDealt;
        public int DamageTaken;
        public List<CombatInjuryEntry> Injuries = new();
        public List<LootEntry> Loot = new();
        public Dictionary<string, int> ExpGained = new();
        public int GoldGained;
        public string MonsterDisplayName;
    }

    [Serializable]
    public sealed class ExplorationEvent
    {
        public string EventId;
        public EventType EventType;
        public SalienceGrade Salience;
        public string ZoneId;
        public int Floor;
        public List<string> Actors = new();
        public long TimestampTick;
        public CombatResultPayload Combat;
        public string DiscoveryItemId;
        public string DiscoveryDisplayName;
        public int GoldDelta;
        public int ManaShardDelta;
        public int RelicFragmentDelta;
        public int ReputationDelta;
        public string MoveDescriptionId;
    }

    [Serializable]
    public sealed class ExplorationState
    {
        public int Seed;
        public long CurrentTick;
        public string ZoneId = ZoneDefinitions.MossyHollowId;
        public int CurrentFloor = 1;
        public float FloorProgress;
        public int MaxFloor = ZoneDefinitions.MossyHollowMaxFloor;
        public PartyState Party = new();
        public int Gold;
        public int ManaShards;
        public int Reputation;
        public int RelicFragments;
        public bool IsExploring;
        public bool IsPaused;
        public DateTime LastOnlineUtc = DateTime.UtcNow;
    }
}
