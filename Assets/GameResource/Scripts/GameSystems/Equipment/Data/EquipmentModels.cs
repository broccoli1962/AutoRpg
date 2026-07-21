using Backend.GameSystems.Equipment.Data;

namespace Backend.GameSystems.Equipment.Data
{
    public sealed class EquipmentDefinition
    {
        public string Id;
        public string DisplayName;
        public EquipmentGrade Grade;
        public EquipmentSlot Slot;
        public int StrBonus;
        public int AgiBonus;
        public int IntBonus;
        public int VitBonus;
    }
}
