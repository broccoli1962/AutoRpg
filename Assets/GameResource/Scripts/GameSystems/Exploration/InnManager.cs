using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Prestige;
using Backend.GameSystems.Save;
using Backend.Util;
using UnityEngine;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 여관 — 휴식 회복·부상 치유 가속 (09_성장과경제.md).
    /// </summary>
    public static class InnManager
    {
        public const int MaxLevel = 3;

        public static int Level
        {
            get
            {
                if (GameStateUtil.IsQuitting)
                    return 0;

                return PrestigeManager.GetMeta()?.InnLevel ?? 0;
            }
        }

        public static float GetRestHealMultiplier() => 1f + Level * 0.3f;

        public static (int legacy, int relic) GetUpgradeCost(int targetLevel) =>
            targetLevel switch
            {
                1 => (4, 1),
                2 => (9, 2),
                3 => (18, 4),
                _ => (0, 0)
            };

        public static void ApplyRest(PartyState party)
        {
            if (party?.Members == null)
                return;

            var healMultiplier = GetRestHealMultiplier() * ExplorationSurvivalBonus.GetRestHealMultiplier(party) *
                                 SkillTreeManager.GetClericRestHealBonus(party);
            var injuryHpThreshold = Level >= 3 ? 0.3f : Level >= 2 ? 0.4f : 0.5f;
            injuryHpThreshold -= ExplorationSurvivalBonus.GetInjuryRecoveryThresholdBonus(party);
            injuryHpThreshold = Mathf.Clamp(injuryHpThreshold, 0.2f, 0.6f);

            foreach (var member in party.Members)
            {
                if (member == null || member.CurrentHp <= 0)
                    continue;

                var heal = Mathf.Max(1, Mathf.RoundToInt(member.MaxHp / 8f * healMultiplier));
                member.CurrentHp = Mathf.Min(member.MaxHp, member.CurrentHp + heal);

                if (member.Injury == InjurySeverity.Light &&
                    member.CurrentHp >= member.MaxHp * injuryHpThreshold)
                {
                    member.Injury = InjurySeverity.None;
                }
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

            if (meta.InnLevel >= MaxLevel)
            {
                reason = "최대 레벨";
                return false;
            }

            var cost = GetUpgradeCost(meta.InnLevel + 1);
            if (meta.LegacyPoints < cost.legacy || meta.RelicFragments < cost.relic)
            {
                reason = $"유산 {cost.legacy} · 유물 {cost.relic} 필요";
                return false;
            }

            return true;
        }

        public static bool TryUpgrade(out string message)
        {
            if (!CanUpgrade(out message))
                return false;

            var meta = PrestigeManager.GetMeta();
            var nextLevel = meta.InnLevel + 1;
            var cost = GetUpgradeCost(nextLevel);
            meta.LegacyPoints -= cost.legacy;
            meta.RelicFragments -= cost.relic;
            meta.InnLevel = nextLevel;
            message = $"여관 Lv.{nextLevel} 해금 (휴식 회복 x{GetRestHealMultiplier():0.#})";
            Debug.Log($"[InnManager] Upgraded to level {nextLevel}");
            GameSaveManager.Save();
            return true;
        }

        public static string GetDisplayLabel()
        {
            if (Level >= MaxLevel)
                return "여관:MAX";

            var next = GetUpgradeCost(Level + 1);
            return $"여관:Lv.{Level} (다음 유산{next.legacy}/유물{next.relic})";
        }

        public static string GetBonusSummary() =>
            Level switch
            {
                0 => "보너스 없음",
                1 => "휴식 회복 +30%",
                2 => "회복 +60% · 부상 회복 완화",
                _ => "회복 +90% · 부상 회복 강화"
            };
    }
}
