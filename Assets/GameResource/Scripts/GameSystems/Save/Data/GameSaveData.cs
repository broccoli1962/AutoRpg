using System;
using System.Collections.Generic;
using Backend.GameSystems.Character.Data;
using Backend.GameSystems.Prestige.Data;

namespace Backend.GameSystems.Save.Data
{
    [Serializable]
    public sealed class GameSaveData
    {
        public string SaveVersion = "0.1.0";
        public MetaProgressionState Meta = new();
        public List<CharacterMemory> CharacterMemories = new();
        public Dictionary<string, int> Affinities = new();
    }
}
