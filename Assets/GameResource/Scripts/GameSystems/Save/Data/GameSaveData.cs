using System;
using System.Collections.Generic;
using Backend.GameSystems.Character.Data;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Prestige.Data;

namespace Backend.GameSystems.Save.Data
{
    [Serializable]
    public sealed class GameSaveData
    {
        public string SaveVersion = "0.1.0";
        public MetaProgressionState Meta = new();
        public List<CharacterMemory> CharacterMemories = new();
        public Dictionary<string, int> Affinities = new();
        public int LlmQualityMode;
        public int DynamicEventAutoPolicy;
        public int GoldenEventAutoPause = 1;
        public int LogFrequencyMode;
        public int OfflineSummaryDetailMode;
        public int StageVfxDensityMode;
        public ExplorationRunSaveData ExplorationRun;
    }

    /// <summary>
    /// 진행 중 탐험 세션 스냅샷. 앱 재시작 후 오프라인 보정·재개에 사용한다.
    /// </summary>
    [Serializable]
    public sealed class ExplorationRunSaveData
    {
        public bool HasActiveRun;
        public ExplorationState State;
        public uint RandomState;
        public int LastFloor = 1;
        public int TicksSinceLastDynamicEvent;
    }
}
