using System.Collections.Generic;
using Backend.GameSystems.Prestige;
using Backend.GameSystems.Save;
using Backend.Util;
using UnityEngine;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 서점 — 유물조각·유산으로 로어 도감 항목을 해금한다 (09_성장과경제.md).
    /// </summary>
    public static class BookshopManager
    {
        public const int MaxLevel = 3;

        private static readonly string[] UnlockableLore =
        {
            "등불 수호단 창설 기록: \"빛은 거짓말을 하지 않는다\"는 맹세로 시작되었다.",
            "균사 미궁 속성: 포자 안개는 기억을 흐리게 하지만, 노래는 길을 기억한다.",
            "침묵의 폐허 비밀: 고대 문명은 심연을 봉인하려 했으나 스스로 그 안으로 사라졌다.",
            "심연 문턱 경고: \"아래로 갈수록 이름을 잃는다\" — 탐험대 공통 각인."
        };

        public static int Level
        {
            get
            {
                if (GameStateUtil.IsQuitting)
                    return 0;

                return PrestigeManager.GetMeta()?.BookshopLevel ?? 0;
            }
        }

        public static (int legacy, int relic) GetUpgradeCost(int targetLevel) =>
            targetLevel switch
            {
                1 => (3, 2),
                2 => (8, 4),
                3 => (15, 6),
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

            if (meta.BookshopLevel >= MaxLevel)
            {
                reason = "최대 레벨";
                return false;
            }

            if (FindNextLore(meta.LoreEntries) == null)
            {
                reason = "해금할 로어 없음";
                return false;
            }

            var cost = GetUpgradeCost(meta.BookshopLevel + 1);
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
            var nextLore = FindNextLore(meta.LoreEntries);
            if (nextLore == null)
            {
                message = "해금할 로어 없음";
                return false;
            }

            var nextLevel = meta.BookshopLevel + 1;
            var cost = GetUpgradeCost(nextLevel);
            meta.LegacyPoints -= cost.legacy;
            meta.RelicFragments -= cost.relic;
            meta.BookshopLevel = nextLevel;
            meta.LoreEntries.Add(nextLore);
            message = $"서점 Lv.{nextLevel} — 로어 해금";
            Debug.Log($"[BookshopManager] Upgraded to level {nextLevel}, unlocked lore entry");
            GameSaveManager.Save();
            return true;
        }

        public static string GetDisplayLabel()
        {
            if (Level >= MaxLevel)
                return "서점:MAX";

            var next = GetUpgradeCost(Level + 1);
            return $"서점:Lv.{Level} (다음 유산{next.legacy}/유물{next.relic})";
        }

        public static string GetBonusSummary()
        {
            var remaining = CountRemainingLore();
            return Level switch
            {
                0 => $"로어 해금 가능 {remaining}건",
                _ => $"Lv.{Level} · 남은 로어 {remaining}건"
            };
        }

        private static string FindNextLore(List<string> entries)
        {
            foreach (var lore in UnlockableLore)
            {
                if (entries == null || !entries.Contains(lore))
                    return lore;
            }

            return null;
        }

        private static int CountRemainingLore()
        {
            var meta = PrestigeManager.GetMeta();
            var count = 0;
            foreach (var lore in UnlockableLore)
            {
                if (meta?.LoreEntries == null || !meta.LoreEntries.Contains(lore))
                    count++;
            }

            return count;
        }
    }
}
