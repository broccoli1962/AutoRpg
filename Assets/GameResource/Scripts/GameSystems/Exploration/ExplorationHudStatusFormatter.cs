using Backend.GameSystems.DynamicEvent;
using Backend.GameSystems.Equipment;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Narration;
using Backend.GameSystems.LLM;
using Backend.GameSystems.Prestige;

namespace Backend.GameSystems.Exploration
{
    public static class ExplorationHudStatusFormatter
    {
        public static string Build(ExplorationState state)
        {
            if (state == null)
                return "탐험 상태 없음";

            var meta = PrestigeManager.GetMeta();
            var equipment = EquipmentService.GetLeaderEquipmentSummary(state.Party);
            return
                $"{ZoneDefinitions.GetZoneDisplayName(state.ZoneId)} {state.CurrentFloor}층 · 진행 {state.FloorProgress:0.#}% · " +
                $"골드 {state.Gold} · 마나 {state.ManaShards + (meta?.ManaShards ?? 0)} · 명성 {state.Reputation + (meta?.Reputation ?? 0)} · 유물 {state.RelicFragments + (meta?.RelicFragments ?? 0)} · 유산 {meta?.LegacyPoints ?? 0} · {SkillTreeManager.GetLeaderDisplayLabel()} · {BookshopManager.GetDisplayLabel()} · {InnManager.GetDisplayLabel()} · {BlacksmithManager.GetDisplayLabel()} · {TrainingGroundManager.GetDisplayLabel()} · {ScriptoriumManager.GetDisplayLabel()} · {LlmQualitySettings.GetDisplayLabel()} · {LogFrequencySettings.GetDisplayLabel()} · {OfflineSummaryDetailSettings.GetDisplayLabel()} · {DynamicEventAutoPolicySettings.GetDisplayLabel()} · {GoldenEventSettings.GetDisplayLabel()}\n" +
                $"장비 {equipment} · Tick {state.CurrentTick}";
        }
    }
}
