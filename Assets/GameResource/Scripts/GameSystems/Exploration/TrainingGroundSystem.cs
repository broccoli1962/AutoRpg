using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Prestige;
using Backend.GameSystems.Save;
using Backend.Util;
using UnityEngine;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 훈련소 — 메타 재화로 영구 스탯 보너스를 구매한다 (09_성장과경제.md).
    /// </summary>
    public static class TrainingGroundSystem
    {
        public const int MaxLevel = 3;

        public static int Level
        {
            get
            {
                if (GameStateUtil.IsQuitting)
                    return 0;

                return PrestigeManager.GetMeta()?.TrainingGroundLevel ?? 0;
            }
        }

        public static int GetStatBonusPerMember() => Level;

        public static (int legacy, int reputation) GetUpgradeCost(int targetLevel) =>
            targetLevel switch
            {
                1 => (3, 2),
                2 => (8, 5),
                3 => (18, 10),
                _ => (0, 0)
            };

        public static void ApplyPartyBonuses(PartyState party)
        {
            var bonus = GetStatBonusPerMember();
            if (party?.Members == null || bonus <= 0)
                return;

            foreach (var member in party.Members)
            {
                if (member == null)
                    continue;

                member.Str += bonus;
                member.Agi += bonus;
                member.Int += bonus;
                member.Vit += bonus;
                member.MaxHp += bonus * 6;
                member.CurrentHp = member.MaxHp;
            }
        }

        public static bool CanUpgrade(out string reason)
        {
            reason = null;
            if (GameStateUtil.IsQuitting)
            {
                reason = "종료 중";
                return false;
            }

            var meta = PrestigeManager.GetMeta();
            if (meta == null)
            {
                reason = "메타 데이터 없음";
                return false;
            }

            if (meta.TrainingGroundLevel >= MaxLevel)
            {
                reason = "최대 레벨";
                return false;
            }

            var cost = GetUpgradeCost(meta.TrainingGroundLevel + 1);
            if (meta.LegacyPoints < cost.legacy || meta.Reputation < cost.reputation)
            {
                reason = $"유산 {cost.legacy} · 명성 {cost.reputation} 필요";
                return false;
            }

            return true;
        }

        public static bool TryUpgrade(out string message)
        {
            if (!CanUpgrade(out message))
                return false;

            var meta = PrestigeManager.GetMeta();
            var nextLevel = meta.TrainingGroundLevel + 1;
            var cost = GetUpgradeCost(nextLevel);
            meta.LegacyPoints -= cost.legacy;
            meta.Reputation -= cost.reputation;
            meta.TrainingGroundLevel = nextLevel;
            message = $"훈련소 Lv.{nextLevel} 해금 (파티 스탯 +{nextLevel})";
            Debug.Log($"[TrainingGroundSystem] Upgraded to level {nextLevel}");
            GameSaveManager.Save();
            return true;
        }

        public static string GetDisplayLabel()
        {
            if (Level >= MaxLevel)
                return "훈련소:MAX";

            var next = GetUpgradeCost(Level + 1);
            return $"훈련소:Lv.{Level} (다음 유산{next.legacy}/명성{next.reputation})";
        }

        public static string GetBonusSummary() =>
            Level switch
            {
                0 => "보너스 없음",
                _ => $"새 탐험 시작 시 전 스탯 +{Level}, HP +{Level * 6}"
            };
    }
}
