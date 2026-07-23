using System;
using Backend.GameSystems.Character;
using Backend.GameSystems.DynamicEvent;
using Backend.GameSystems.Equipment;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Narration;
using Backend.GameSystems.Exploration.Simulation;
using Backend.GameSystems.LLM;
using Backend.GameSystems.Prestige;
using Backend.GameSystems.Save;
using Backend.Object.Controller;
using Backend.Util;
using UnityEngine;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 탐험 세션·틱·오프라인 규칙을 담당하는 static System. 씬 오브젝트는 Controller 가 구동한다.
    /// </summary>
    public static class ExplorationSystem
    {
        private const float RealSecondsPerTick = 1f;

        private static ExplorationSession _session;
        private static float _tickAccumulator;
        private static bool _initialized;

        /// <summary>현재 탐험 상태.</summary>
        public static ExplorationState CurrentState => _session?.State;

        /// <summary>탐험 진행 중 여부.</summary>
        public static bool IsRunning => _session?.State?.IsExploring == true;

        /// <summary>System 을 초기화한다.</summary>
        public static void Initialize()
        {
            if (_initialized || GameStateUtil.IsQuitting)
                return;

            LlmNarrationManager.EnsureInitialized();
            DynamicEventSystem.EnsureInitialized();
            CharacterMemorySystem.EnsureInitialized();
            RelationshipSystem.EnsureInitialized();
            ExplorationSessionLogArchive.EnsureInitialized();
            PrestigeManager.EnsureInitialized();
            GameSaveManager.EnsureInitialized();
            GameSaveManager.Load();

            _session = new ExplorationSession(new HybridLogNarrator());
            _tickAccumulator = 0f;
            _initialized = true;
        }

        /// <summary>System 리소스를 해제한다.</summary>
        public static void Dispose()
        {
            _session = null;
            _tickAccumulator = 0f;
            _initialized = false;
        }

        /// <summary>현재 탐험 상태를 반환한다.</summary>
        public static ExplorationState GetCurrentState()
        {
            if (GameStateUtil.IsQuitting)
                return null;

            EnsureInitialized();
            return _session?.State;
        }

        /// <summary>실시간 deltaTime 을 누적해 틱을 처리한다. Controller 가 매 프레임 호출한다.</summary>
        public static void AccumulateTick(float deltaTime)
        {
            if (GameStateUtil.IsQuitting || _session?.State == null)
                return;

            if (!_session.State.IsExploring || _session.State.IsPaused)
                return;

            _tickAccumulator += deltaTime;
            while (_tickAccumulator >= RealSecondsPerTick)
            {
                _tickAccumulator -= RealSecondsPerTick;
                _session.ProcessTick();
            }
        }

        /// <summary>기본 파티로 새 탐험을 시작한다.</summary>
        public static void StartExploration(int seed = 0)
        {
            if (GameStateUtil.IsQuitting)
                return;

            EnsureInitialized();
            var resolvedSeed = seed == 0 ? Environment.TickCount : seed;
            StartExplorationInternal(resolvedSeed);
        }

        /// <summary>탐험 진행을 재개한다.</summary>
        public static void ResumeExploration()
        {
            if (GameStateUtil.IsQuitting)
                return;

            EnsureInitialized();
            _session?.Resume();
        }

        /// <summary>탐험 진행을 일시 정지한다.</summary>
        public static void PauseExploration()
        {
            if (GameStateUtil.IsQuitting)
                return;

            EnsureInitialized();
            _session?.Pause();
        }

        /// <summary>수동 귀환을 수행한다.</summary>
        public static void ReturnToGuild()
        {
            if (GameStateUtil.IsQuitting)
                return;

            EnsureInitialized();
            _session?.ReturnToGuild();
        }

        /// <summary>플레이어 입력으로 탐험을 시작한다.</summary>
        public static void BeginExplorationFromPlayer(int seed = 0)
        {
            if (GameStateUtil.IsQuitting)
                return;

            ProcessOfflineElapsed();
            StartExploration(seed);
        }

        /// <summary>마지막 접속 이후 경과 시간을 오프라인 시뮬레이션으로 처리한다.</summary>
        public static void ProcessOfflineElapsed()
        {
            if (GameStateUtil.IsQuitting)
                return;

            EnsureInitialized();
            if (_session?.State == null)
                return;

            var elapsed = DateTime.UtcNow - _session.State.LastOnlineUtc;
            if (elapsed.TotalSeconds < ExplorationSimulator.TickDurationSeconds)
                return;

            _session.ProcessOffline(elapsed);
        }

        /// <summary>System 과 Tick Controller 런타임을 보장한다.</summary>
        public static void EnsureRuntime()
        {
            EnsureInitialized();
            if (UnityEngine.Object.FindAnyObjectByType<ExplorationTickController>() != null)
                return;

            var tickObject = new GameObject(nameof(ExplorationTickController));
            tickObject.AddComponent<ExplorationTickController>();
            UnityEngine.Object.DontDestroyOnLoad(tickObject);
        }

        private static void EnsureInitialized()
        {
            if (!_initialized)
                Initialize();
        }

        private static void StartExplorationInternal(int seed)
        {
            var party = ZoneDefinitions.CreateDefaultParty();
            TrainingGroundSystem.ApplyPartyBonuses(party);
            SkillTreeSystem.ApplyPartyBonuses(party);
            CharacterTierSystem.ApplyPartyTiers(party);
            EquipmentEnhanceSystem.ApplyPartyEnhances(party);
            EquipmentService.ClearPartyEquipment(party);
            BlacksmithSystem.ApplyStartingEquipment(party);
            _session.StartNew(seed, party);
            _session.State.Gold = PrestigeManager.GetStartingGoldBonus();
            ExplorationSessionLogArchive.Clear();
            CharacterMemorySystem.BindParty(party);
            RelationshipSystem.BindParty(party);
            _tickAccumulator = 0f;
            ExplorationChannels.PublishStateChanged(_session.State);
            Debug.Log($"[ExplorationSystem] Exploration started. Seed={seed}");
        }
    }
}
