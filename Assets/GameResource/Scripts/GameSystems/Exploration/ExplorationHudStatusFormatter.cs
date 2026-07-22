using Backend.GameSystems.DynamicEvent;
using Backend.GameSystems.Equipment;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Narration;
using Backend.GameSystems.LLM;
using Backend.GameSystems.Prestige;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 12_UIUX 상단바·중앙 진행 표시용 HUD 문자열 포맷.
    /// </summary>
    public static class ExplorationHudStatusFormatter
    {
        public const string GuildDisplayName = "등불 수호단";

        public static string Build(ExplorationState state)
        {
            if (state == null)
                return "탐험 상태 없음";

            return $"{BuildTopResourceBar(state)}\n{BuildExplorationLine(state)}";
        }

        public static string BuildTopResourceBar(ExplorationState state)
        {
            var meta = PrestigeManager.GetMeta();
            var gold = state.Gold;
            var mana = state.ManaShards + (meta?.ManaShards ?? 0);
            var reputation = state.Reputation + (meta?.Reputation ?? 0);
            var relic = state.RelicFragments + (meta?.RelicFragments ?? 0);
            var legacy = meta?.LegacyPoints ?? 0;

            return
                $"<b>{GuildDisplayName}</b>  ·  " +
                $"<color=#e8c547>G {gold}</color>  ·  " +
                $"<color=#6ec5ff>◆ {mana}</color>  ·  " +
                $"<color=#c9a0ff>★ {reputation}</color>  ·  " +
                $"<color=#ffb366>◈ {relic}</color>  ·  " +
                $"<color=#9fd89f>유산 {legacy}</color>";
        }

        public static string BuildExplorationLine(ExplorationState state)
        {
            var equipment = EquipmentService.GetLeaderEquipmentSummary(state.Party);
            return
                $"{ZoneDefinitions.GetZoneDisplayName(state.ZoneId)} {state.CurrentFloor}층 · " +
                $"진행 {state.FloorProgress:0.#}% · 장비 {equipment} · Tick {state.CurrentTick}";
        }

        public static string BuildSettingsSummary()
        {
            return
                $"{SkillTreeManager.GetLeaderDisplayLabel()} · {BookshopManager.GetDisplayLabel()} · " +
                $"{InnManager.GetDisplayLabel()} · {BlacksmithManager.GetDisplayLabel()} · " +
                $"{TrainingGroundManager.GetDisplayLabel()} · {ScriptoriumManager.GetDisplayLabel()} · " +
                $"{LlmQualitySettings.GetDisplayLabel()} · {LogFrequencySettings.GetDisplayLabel()} · " +
                $"{OfflineSummaryDetailSettings.GetDisplayLabel()} · {DynamicEventAutoPolicySettings.GetDisplayLabel()} · " +
                $"{GoldenEventSettings.GetDisplayLabel()}";
        }
    }
}
