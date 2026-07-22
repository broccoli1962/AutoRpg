using Backend.GameSystems.Prestige;
using Backend.GameSystems.Save;
using Backend.Util;
using UnityEngine;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 필사가의 서고 — 메타 재화로 영구 업그레이드하여 로그·LLM·이벤트 보너스를 제공한다.
    /// </summary>
    public static class ScriptoriumManager
    {
        public const int MaxLevel = 3;

        public static int Level
        {
            get
            {
                if (GameStateUtil.IsQuitting)
                    return 0;

                return PrestigeManager.GetMeta()?.ScriptoriumLevel ?? 0;
            }
        }

        public static int GetTokenBonus() =>
            Level switch
            {
                1 => 16,
                2 => 32,
                3 => 48,
                _ => 0
            };

        public static int GetSalienceGradeReduction() => Level >= 2 ? 1 : 0;

        public static float GetEventRateMultiplier() => Level >= 3 ? 1.15f : 1f;

        public static (int legacy, int mana) GetUpgradeCost(int targetLevel) =>
            targetLevel switch
            {
                1 => (5, 3),
                2 => (12, 8),
                3 => (25, 15),
                _ => (0, 0)
            };

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

            if (meta.ScriptoriumLevel >= MaxLevel)
            {
                reason = "최대 레벨";
                return false;
            }

            var cost = GetUpgradeCost(meta.ScriptoriumLevel + 1);
            if (meta.LegacyPoints < cost.legacy || meta.ManaShards < cost.mana)
            {
                reason = $"유산 {cost.legacy} · 마나 {cost.mana} 필요";
                return false;
            }

            return true;
        }

        public static bool TryUpgrade(out string message)
        {
            if (!CanUpgrade(out message))
                return false;

            var meta = PrestigeManager.GetMeta();
            var nextLevel = meta.ScriptoriumLevel + 1;
            var cost = GetUpgradeCost(nextLevel);
            meta.LegacyPoints -= cost.legacy;
            meta.ManaShards -= cost.mana;
            meta.ScriptoriumLevel = nextLevel;
            message = $"필사가의 서고 Lv.{nextLevel} 해금";
            Debug.Log($"[ScriptoriumManager] Upgraded to level {nextLevel}");
            GameSaveManager.Save();
            return true;
        }

        public static string GetDisplayLabel()
        {
            if (Level >= MaxLevel)
                return "서고:MAX";

            var next = GetUpgradeCost(Level + 1);
            return $"서고:Lv.{Level} (다음 유산{next.legacy}/마나{next.mana})";
        }

        public static string GetBonusSummary()
        {
            return Level switch
            {
                0 => "보너스 없음",
                1 => "LLM 토큰 +16",
                2 => "LLM 토큰 +32 · 로그 Salience -1",
                _ => "LLM 토큰 +48 · Salience -1 · 이벤트 +15%"
            };
        }
    }
}
