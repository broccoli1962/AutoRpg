using Backend.GameSystems.Equipment.Data;
using Backend.GameSystems.Exploration;
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

            if (!random.RollChance(dropChance * BlacksmithSystem.GetDropChanceMultiplier()))
                return;

            var definition = EquipmentDefinitions.RollDrop(monsterRarity, random, state.ZoneId, state.CurrentFloor);
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
            SumBonus(member, EquipmentSlot.Armor, d => d.StrBonus) + GetSetStrBonus(member);

        public static int GetEffectiveAgi(CharacterState member) =>
            member.Agi + SumBonus(member, EquipmentSlot.Weapon, d => d.AgiBonus) +
            SumBonus(member, EquipmentSlot.Armor, d => d.AgiBonus) + GetSetAgiBonus(member);

        public static int GetEffectiveInt(CharacterState member) =>
            member.Int + SumBonus(member, EquipmentSlot.Weapon, d => d.IntBonus) +
            SumBonus(member, EquipmentSlot.Armor, d => d.IntBonus) + GetSetIntBonus(member);

        public static int GetEffectiveVit(CharacterState member) =>
            member.Vit + SumBonus(member, EquipmentSlot.Weapon, d => d.VitBonus) +
            SumBonus(member, EquipmentSlot.Armor, d => d.VitBonus) + GetSetVitBonus(member);

        public static string GetLeaderEquipmentSummary(PartyState party)
        {
            var leader = party?.Leader;
            if (leader == null)
                return "장비 없음";

            var weapon = GetDisplayName(leader.EquippedWeaponId);
            var armor = GetDisplayName(leader.EquippedArmorId);
            if (string.IsNullOrEmpty(weapon) && string.IsNullOrEmpty(armor))
                return "장비 없음";

            if (string.IsNullOrEmpty(armor))
                return weapon;

            if (string.IsNullOrEmpty(weapon))
                return armor;

            return $"{weapon}/{armor}";
        }

        public static string GetMemberEquipmentSummary(CharacterState member)
        {
            if (member == null)
                return null;

            var weapon = GetDisplayName(member.EquippedWeaponId);
            var armor = GetDisplayName(member.EquippedArmorId);
            if (string.IsNullOrEmpty(weapon) && string.IsNullOrEmpty(armor))
                return null;

            string summary;
            if (string.IsNullOrEmpty(armor))
                summary = weapon;
            else if (string.IsNullOrEmpty(weapon))
                summary = armor;
            else
                summary = $"{weapon} · {armor}";

            var setLabel = GetSetBonusLabel(member);
            return string.IsNullOrEmpty(setLabel) ? summary : $"{summary} · {setLabel}";
        }

        public static string GetSetBonusLabel(CharacterState member)
        {
            if (!TryGetActiveSetId(member, out var setId))
                return null;

            return setId switch
            {
                EquipmentDefinitions.GuardianSetId => "수호 세트(2)",
                EquipmentDefinitions.AbyssSetId => "심연 세트(2)",
                EquipmentDefinitions.PrismSetId => "프리즘 세트(2)",
                _ => "세트(2)"
            };
        }

        private static string GetDisplayName(string definitionId)
        {
            if (string.IsNullOrEmpty(definitionId))
                return null;

            var definition = EquipmentDefinitions.Get(definitionId);
            if (definition == null)
                return null;

            return $"{definition.DisplayName}[{EquipmentGradeUtil.GetLabel(definition.Grade)}]";
        }

        private static CharacterState SelectRecipient(PartyState party, EquipmentSlot slot)
        {
            CharacterState best = null;
            foreach (var member in party.Members)
            {
                if (member.CurrentHp <= 0)
                    continue;

                if (slot == EquipmentSlot.Weapon &&
                    (member.Role == CharacterRole.Warrior ||
                     member.Role == CharacterRole.Rogue ||
                     member.Role == CharacterRole.Bard ||
                     member.Role == CharacterRole.Mage ||
                     member.Role == CharacterRole.Cleric))
                {
                    best = member;
                    break;
                }

                if (slot == EquipmentSlot.Armor &&
                    (member.Role == CharacterRole.Warrior || member.Role == CharacterRole.Cleric))
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
            if (definition == null)
                return 0;

            var bonus = selector(definition);
            if (bonus <= 0)
                return 0;

            var enhanceLevel = slot == EquipmentSlot.Weapon
                ? member.WeaponEnhanceLevel
                : member.ArmorEnhanceLevel;

            return bonus + enhanceLevel;
        }

        private static bool TryGetActiveSetId(CharacterState member, out string setId)
        {
            setId = null;
            if (member == null)
                return false;

            var weapon = string.IsNullOrEmpty(member.EquippedWeaponId)
                ? null
                : EquipmentDefinitions.Get(member.EquippedWeaponId);
            var armor = string.IsNullOrEmpty(member.EquippedArmorId)
                ? null
                : EquipmentDefinitions.Get(member.EquippedArmorId);

            if (weapon == null || armor == null ||
                string.IsNullOrEmpty(weapon.SetId) ||
                weapon.SetId != armor.SetId)
            {
                return false;
            }

            setId = weapon.SetId;
            return true;
        }

        private static int GetSetStrBonus(CharacterState member)
        {
            if (!TryGetActiveSetId(member, out var setId))
                return 0;

            return setId switch
            {
                EquipmentDefinitions.GuardianSetId => 1,
                EquipmentDefinitions.AbyssSetId => 4,
                EquipmentDefinitions.PrismSetId => 2,
                _ => 0
            };
        }

        private static int GetSetAgiBonus(CharacterState member)
        {
            if (!TryGetActiveSetId(member, out var setId))
                return 0;

            return setId == EquipmentDefinitions.AbyssSetId ? 2 : 0;
        }

        private static int GetSetIntBonus(CharacterState member)
        {
            if (!TryGetActiveSetId(member, out var setId))
                return 0;

            return setId == EquipmentDefinitions.PrismSetId ? 4 : 0;
        }

        private static int GetSetVitBonus(CharacterState member)
        {
            if (!TryGetActiveSetId(member, out var setId))
                return 0;

            return setId == EquipmentDefinitions.GuardianSetId ? 4 : 0;
        }
    }
}
