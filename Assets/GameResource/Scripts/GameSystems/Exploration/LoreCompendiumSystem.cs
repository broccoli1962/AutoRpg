using System.Collections.Generic;
using Backend.GameSystems.DynamicEvent.Data;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Prestige;
using Backend.GameSystems.Save;
using Backend.Util;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 03_세계관 로어 조각 도감 — 발견/이벤트로 해금된 텍스트를 메타에 축적한다.
    /// </summary>
    public static class LoreCompendiumSystem
    {
        public static IReadOnlyList<string> GetEntries()
        {
            if (GameStateUtil.IsQuitting)
                return System.Array.Empty<string>();

            var entries = PrestigeManager.GetMeta()?.LoreEntries;
            return entries != null ? entries : System.Array.Empty<string>();
        }

        public static void RecordDiscovery(ExplorationEvent explorationEvent)
        {
            if (explorationEvent == null ||
                explorationEvent.EventType != EventType.Discovery ||
                string.IsNullOrEmpty(explorationEvent.DiscoveryItemId))
            {
                return;
            }

            var entry = BuildDiscoveryLore(explorationEvent.DiscoveryItemId, explorationEvent.DiscoveryDisplayName);
            if (!string.IsNullOrEmpty(entry))
                TryAdd(entry);
        }

        public static void RecordDynamicEvent(DynamicEventInstance instance)
        {
            if (instance == null)
                return;

            if (instance.TemplateId != DynamicEventDefinitions.ArtifactLoreFragmentId)
                return;

            TryAdd("고대 벽화에는 등불 수호단 이전의 탐험대 흔적이 새겨져 있었다. \"심연은 기억을 먹는다\"는 문장만이 선명했다.");
        }

        private static string BuildDiscoveryLore(string itemId, string displayName)
        {
            return itemId switch
            {
                "rusty_ring" => "낡은 반지 안쪽에 희미한 문양 — 등불 수호단의 초기 문장으로 추정된다.",
                "resonance_stone" => "공명석 표면에 짧은 각인: \"빛은 거짓말을 하지 않는다.\"",
                "crystal_lens" => "수정 렌즈를 통해 본 동굴 벽 — 평소와 다른 층이 겹쳐 보인다.",
                "artifact_lore_fragment" => "유물 조각에 새겨진 파편 문장: \"아래로 갈수록 이름을 잃는다.\"",
                ZoneDefinitions.RelicFragmentItemId =>
                    "유물조각 표면의 문양 — 등불 수호단이 심연 유물을 수집하던 흔적이다.",
                _ when itemId.Contains("lore") =>
                    $"{displayName ?? "로어 조각"}에서 세계관 단서를 발견했다.",
                _ => null
            };
        }

        private static void TryAdd(string entry)
        {
            if (GameStateUtil.IsQuitting || string.IsNullOrEmpty(entry))
                return;

            var meta = PrestigeManager.GetMeta();
            if (meta?.LoreEntries == null)
                return;

            if (meta.LoreEntries.Contains(entry))
                return;

            meta.LoreEntries.Add(entry);
            GameSaveManager.Save();
        }
    }
}
