using System;
using System.Collections.Generic;

namespace Backend.GameSystems.Prestige.Data
{
    [Serializable]
    public sealed class MetaProgressionState
    {
        public int LegacyPoints;
        public int PrestigeCount;
        public int DeepestFloorReached;
        public List<string> ChronicleEntries = new();
    }
}
