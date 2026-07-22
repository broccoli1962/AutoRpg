using System;

namespace Backend.GameSystems.Prestige.Data
{
    [Serializable]
    public sealed class CharacterTierRecord
    {
        public string CharacterId;
        public int TierIndex;
    }

    [Serializable]
    public sealed class EquipmentEnhanceRecord
    {
        public string CharacterId;
        public string Slot;
        public int Level;
    }
}
