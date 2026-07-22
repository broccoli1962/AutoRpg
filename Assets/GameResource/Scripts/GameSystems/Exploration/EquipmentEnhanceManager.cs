using Backend.GameSystems.Equipment;
using Backend.GameSystems.Equipment.Data;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Prestige;
using Backend.GameSystems.Prestige.Data;
using Backend.GameSystems.Save;
using Backend.Util;
using UnityEngine;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 대장간 장비 강화 — 메타에 영구 저장 (09_성장과경제.md).
    /// </summary>
    public static class EquipmentEnhanceManager
    {
        public const int MaxEnhanceLevel = 5;

        public static int GetMaxEnhanceLevel() =>
            Mathf.Min(MaxEnhanceLevel, BlacksmithManager.Level + 2);

        public static int GetEnhanceLevel(string characterId, EquipmentSlot slot)
        {
            if (GameStateUtil.IsQuitting || string.IsNullOrEmpty(characterId))
                return 0;

            var meta = PrestigeManager.GetMeta();
            if (meta?.EquipmentEnhances == null)
                return 0;

            var slotName = SlotToKey(slot);
            foreach (var record in meta.EquipmentEnhances)
            {
                if (record.CharacterId == characterId && record.Slot == slotName)
                    return record.Level;
            }

            return 0;
        }

        public static void ApplyPartyEnhances(PartyState party)
        {
            if (party?.Members == null)
                return;

            foreach (var member in party.Members)
            {
                if (member == null)
                    continue;

                member.WeaponEnhanceLevel = GetEnhanceLevel(member.CharacterId, EquipmentSlot.Weapon);
                member.ArmorEnhanceLevel = GetEnhanceLevel(member.CharacterId, EquipmentSlot.Armor);
            }
        }

        public static (int legacy, int mana) GetEnhanceCost(int targetLevel) =>
            targetLevel switch
            {
                1 => (2, 1),
                2 => (4, 2),
                3 => (7, 4),
                4 => (12, 7),
                5 => (18, 12),
                _ => (0, 0)
            };

        public static bool TryEnhance(string characterId, EquipmentSlot slot, out string message)
        {
            message = null;
            if (BlacksmithManager.Level < 1)
            {
                message = "대장간 Lv.1 필요";
                return false;
            }

            if (GameStateUtil.IsQuitting || string.IsNullOrEmpty(characterId))
            {
                message = "캐릭터 없음";
                return false;
            }

            var meta = PrestigeManager.GetMeta();
            if (meta?.EquipmentEnhances == null)
            {
                message = "메타 데이터 없음";
                return false;
            }

            var current = GetEnhanceLevel(characterId, slot);
            if (current >= GetMaxEnhanceLevel())
            {
                message = "강화 상한";
                return false;
            }

            var next = current + 1;
            var cost = GetEnhanceCost(next);
            if (meta.LegacyPoints < cost.legacy || meta.ManaShards < cost.mana)
            {
                message = $"유산 {cost.legacy} · 마나 {cost.mana} 필요";
                return false;
            }

            meta.LegacyPoints -= cost.legacy;
            meta.ManaShards -= cost.mana;

            var slotKey = SlotToKey(slot);
            EquipmentEnhanceRecord record = null;
            foreach (var entry in meta.EquipmentEnhances)
            {
                if (entry.CharacterId == characterId && entry.Slot == slotKey)
                {
                    record = entry;
                    break;
                }
            }

            if (record == null)
            {
                record = new EquipmentEnhanceRecord { CharacterId = characterId, Slot = slotKey };
                meta.EquipmentEnhances.Add(record);
            }

            record.Level = next;
            message = $"{SlotLabel(slot)} +{next} 강화";
            Debug.Log($"[EquipmentEnhanceManager] {characterId} {slotKey} -> +{next}");
            GameSaveManager.Save();
            return true;
        }

        public static bool TryEnhanceLeaderWeapon(out string message)
        {
            var leader = ExplorationManager.GetCurrentState()?.Party?.Leader;
            if (leader == null)
            {
                message = "파티 리더 없음";
                return false;
            }

            return TryEnhance(leader.CharacterId, EquipmentSlot.Weapon, out message);
        }

        public static bool TryEnhanceLeaderArmor(out string message)
        {
            var leader = ExplorationManager.GetCurrentState()?.Party?.Leader;
            if (leader == null)
            {
                message = "파티 리더 없음";
                return false;
            }

            return TryEnhance(leader.CharacterId, EquipmentSlot.Armor, out message);
        }

        private static string SlotToKey(EquipmentSlot slot) =>
            slot == EquipmentSlot.Armor ? "armor" : "weapon";

        private static string SlotLabel(EquipmentSlot slot) =>
            slot == EquipmentSlot.Armor ? "방어구" : "무기";
    }
}
