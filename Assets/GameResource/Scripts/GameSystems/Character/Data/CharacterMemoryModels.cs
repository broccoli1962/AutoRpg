using System;
using System.Collections.Generic;

namespace Backend.GameSystems.Character.Data
{
    [Serializable]
    public sealed class CoreMemoryEntry
    {
        public string MemoryId;
        public string Description;
        public List<string> Tags = new();
        public string Weight = "normal";
    }

    [Serializable]
    public sealed class RelationshipEntry
    {
        public int Affinity;
        public bool BondUnlocked;
    }

    [Serializable]
    public sealed class CharacterMemory
    {
        public string CharacterId;
        public List<string> ShortTermBuffer = new();
        public string LongTermSummary;
        public List<CoreMemoryEntry> CoreMemories = new();
        public Dictionary<string, RelationshipEntry> Relationships = new();
    }
}
