using System;
using System.Collections.Generic;

namespace Backend.GameSystems.Prestige.Data
{
    [Serializable]
    public sealed class MetaProgressionState
    {
        public int LegacyPoints;
        public int ManaShards;
        public int ScriptoriumLevel;
        public int PrestigeCount;
        public int DeepestFloorReached;
        public List<string> ChronicleEntries = new();
        public List<string> FavoriteMoments = new();
        public List<string> LoreEntries = new();
    }
}
