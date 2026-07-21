using Backend.GameSystems.Equipment.Data;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Simulation;
using UnityEngine;

namespace Backend.GameSystems.Equipment
{
    /// <summary>
    /// MVP 장비 드롭·자동 장착·스탯 보정을 처리한다.
    /// </summary>
    public static class EquipmentService
    {
        public static void TryProcessCombatDrop(
            ExplorationState state,
            CombatResultPayload combat,
            MonsterRarity monsterRarity,
            DeterministicRandom random)
        {
            if (state?.Party == null || combat == null || combat.Outcome != CombatOutcome.Victory)
                return;

            var dropChance = monsterRarity switch
            {
                MonsterRarity.Boss => 0.85f,
                MonsterRarity.Rare => 0.55f,
                MonsterRarity.Notable => 0.28f,
                _ => 0.12f
            };

            if (!random.RollChance(dropChance))
                return;

            var definition = EquipmentDefinitions.RollDrop(monsterRarity, random);
            if (definition == null)
                return;

            var recipient = SelectRecipient(state.Party, definition.Slot);
            if (recipient == null)
                return;

            EquipIfBetter(recipient, definition);
            combat.Loot.Add(new LootEntry { ItemId = definition.Id, Quantity = 1 });
            Debug.Log($"[EquipmentService] {recipient.DisplayName} equipped {definition.DisplayName} ({definition.Grade})");
        }

        public static void ClearPartyEquipment(PartyState party)
        {
            if (party?.Members == null)
                return;

            foreach (var member in party.Members)
            {
                member.EquippedWeaponId = null;
                member.EquippedArmorId = null;
            }
        }

        public static int GetEffectiveStr(CharacterState member) =>
            member.Str + SumBonus(member, EquipmentSlot.Weapon, d => d.StrBonus) +
            SumBonus(member, EquipmentSlot.Armor, d => d.StrBonus);

        public static int GetEffectiveAgi(CharacterState member) =>
            member.Agi + SumBonus(member, EquipmentSlot.Weapon, d => d.AgiBonus) +
            SumBonus(member, EquipmentSlot.Armor, d => d.AgiBonus);

        public static int GetEffectiveInt(CharacterState member) =>
            member.Int + SumBonus(member, EquipmentSlot.Weapon, d => d.IntBonus) +
            SumBonus(member, EquipmentSlot.Armor, d => d.IntBonus);

        public static int GetEffectiveVit(CharacterState member) =>
            member.Vit + SumBonus(member, EquipmentSlot.Weapon, d => d.VitBonus) +
            SumBonus(member, EquipmentSlot.Armor, d => d.VitBonus);

        private static CharacterState SelectRecipient(PartyState party, EquipmentSlot slot)
        {
            CharacterState best = null;
            foreach (var member in party.Members)
            {
                if (member.CurrentHp <= 0)
                    continue;

                if (slot == EquipmentSlot.Weapon &&
                    (member.Role == CharacterRole.Warrior || member.Role == CharacterRole.Rogue))
                {
                    best = member;
                    break;
                }

                if (slot == EquipmentSlot.Armor && member.Role == CharacterRole.Warrior)
                {
                    best = member;
                    break;
                }
            }

            return best ?? (party.Members.Count > 0 ? party.Members[0] : null);
        }

        private static void EquipIfBetter(CharacterState member, EquipmentDefinition definition)
        {
            var currentId = definition.Slot == EquipmentSlot.Weapon
                ? member.EquippedWeaponId
                : member.EquippedArmorId;

            var current = string.IsNullOrEmpty(currentId) ? null : EquipmentDefinitions.Get(currentId);
            if (current != null && (int)current.Grade >= (int)definition.Grade)
                return;

            if (definition.Slot == EquipmentSlot.Weapon)
                member.EquippedWeaponId = definition.Id;
            else
                member.EquippedArmorId = definition.Id;
        }

        private static int SumBonus(
            CharacterState member,
            EquipmentSlot slot,
            System.Func<EquipmentDefinition, int> selector)
        {
            var id = slot == EquipmentSlot.Weapon ? member.EquippedWeaponId : member.EquippedArmorId;
            if (string.IsNullOrEmpty(id))
                return 0;

            var definition = EquipmentDefinitions.Get(id);
            return definition == null ? 0 : selector(definition);
        }
    }
}
