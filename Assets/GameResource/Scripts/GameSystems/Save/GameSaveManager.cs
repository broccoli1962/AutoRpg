using System.Collections.Generic;
using System.IO;
using Backend.GameSystems.Character;
using Backend.GameSystems.Character.Data;
using Backend.GameSystems.DynamicEvent;
using Backend.GameSystems.LLM;
using Backend.GameSystems.Exploration.Narration;
using Backend.GameSystems.Exploration.Stage;
using Backend.GameSystems.Prestige;
using Backend.GameSystems.Prestige.Data;
using Backend.GameSystems.Save.Data;
using Backend.Util;
using Backend.Util.Management;
using Newtonsoft.Json;
using UnityEngine;

namespace Backend.GameSystems.Save
{
    /// <summary>
    /// 메타 진행·캐릭터 기억·관계 데이터를 JSON으로 영속화한다.
    /// </summary>
    public sealed class GameSaveManager : SingletonGameObject<GameSaveManager>
    {
        private const string SaveFileName = "abyss_chronicle_save.json";

        private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public static void EnsureInitialized()
        {
            if (GameStateUtil.IsQuitting)
                return;

            _ = Instance;
        }

        protected override void OnAwake()
        {
            base.OnAwake();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                Save();
        }

        private void OnApplicationQuit()
        {
            Save();
        }

        public static void Save()
        {
            if (GameStateUtil.IsQuitting)
                return;

            Instance.SaveInternal();
        }

        public static void Load()
        {
            if (GameStateUtil.IsQuitting)
                return;

            Instance.LoadInternal();
        }

        private void SaveInternal()
        {
            try
            {
                var data = new GameSaveData
                {
                    Meta = CloneMeta(PrestigeManager.GetMeta()),
                    CharacterMemories = CharacterMemorySystem.ExportMemories(),
                    Affinities = RelationshipSystem.ExportAffinities(),
                    LlmQualityMode = LlmQualitySettings.ExportMode(),
                    DynamicEventAutoPolicy = DynamicEventAutoPolicySettings.ExportPolicy(),
                    GoldenEventAutoPause = GoldenEventSettings.ExportSetting(),
                    LogFrequencyMode = LogFrequencySettings.ExportMode(),
                    OfflineSummaryDetailMode = OfflineSummaryDetailSettings.ExportMode(),
                    StageVfxDensityMode = StageVfxDensitySettings.ExportMode()
                };

                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(SavePath, json);
                Debug.Log($"[GameSaveManager] Saved to {SavePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[GameSaveManager] Save failed: {e.Message}");
            }
        }

        private void LoadInternal()
        {
            if (!File.Exists(SavePath))
                return;

            try
            {
                var json = File.ReadAllText(SavePath);
                var data = JsonConvert.DeserializeObject<GameSaveData>(json);
                if (data == null)
                    return;

                if (data.Meta != null)
                    PrestigeManager.ImportMeta(data.Meta);

                CharacterMemorySystem.ImportMemories(data.CharacterMemories);
                RelationshipSystem.ImportAffinities(data.Affinities);
                LlmQualitySettings.ImportMode(data.LlmQualityMode);
                DynamicEventAutoPolicySettings.ImportPolicy(data.DynamicEventAutoPolicy);
                GoldenEventSettings.ImportSetting(data.GoldenEventAutoPause);
                LogFrequencySettings.ImportMode(data.LogFrequencyMode);
                OfflineSummaryDetailSettings.ImportMode(data.OfflineSummaryDetailMode);
                StageVfxDensitySettings.ImportMode(data.StageVfxDensityMode);
                Debug.Log($"[GameSaveManager] Loaded save (legacy={data.Meta?.LegacyPoints ?? 0})");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[GameSaveManager] Load failed: {e.Message}");
            }
        }

        private static MetaProgressionState CloneMeta(MetaProgressionState source)
        {
            if (source == null)
                return new MetaProgressionState();

            return new MetaProgressionState
            {
                LegacyPoints = source.LegacyPoints,
                ManaShards = source.ManaShards,
                Reputation = source.Reputation,
                RelicFragments = source.RelicFragments,
                ScriptoriumLevel = source.ScriptoriumLevel,
                TrainingGroundLevel = source.TrainingGroundLevel,
                BlacksmithLevel = source.BlacksmithLevel,
                InnLevel = source.InnLevel,
                BookshopLevel = source.BookshopLevel,
                PrestigeCount = source.PrestigeCount,
                DeepestFloorReached = source.DeepestFloorReached,
                ChronicleEntries = new List<string>(source.ChronicleEntries),
                FavoriteMoments = new List<string>(source.FavoriteMoments ?? new List<string>()),
                LoreEntries = new List<string>(source.LoreEntries ?? new List<string>()),
                MonsterEntries = new List<string>(source.MonsterEntries ?? new List<string>()),
                UnlockedSkillIds = new List<string>(source.UnlockedSkillIds ?? new List<string>()),
                CharacterTiers = CloneTierRecords(source.CharacterTiers),
                EquipmentEnhances = CloneEnhanceRecords(source.EquipmentEnhances)
            };
        }

        private static List<CharacterTierRecord> CloneTierRecords(List<CharacterTierRecord> source)
        {
            var clone = new List<CharacterTierRecord>();
            if (source == null)
                return clone;

            foreach (var record in source)
            {
                if (record == null)
                    continue;

                clone.Add(new CharacterTierRecord
                {
                    CharacterId = record.CharacterId,
                    TierIndex = record.TierIndex
                });
            }

            return clone;
        }

        private static List<EquipmentEnhanceRecord> CloneEnhanceRecords(List<EquipmentEnhanceRecord> source)
        {
            var clone = new List<EquipmentEnhanceRecord>();
            if (source == null)
                return clone;

            foreach (var record in source)
            {
                if (record == null)
                    continue;

                clone.Add(new EquipmentEnhanceRecord
                {
                    CharacterId = record.CharacterId,
                    Slot = record.Slot,
                    Level = record.Level
                });
            }

            return clone;
        }
    }
}
