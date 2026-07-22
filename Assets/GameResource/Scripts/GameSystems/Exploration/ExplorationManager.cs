using System;
using Backend.GameSystems.Character;
using Backend.GameSystems.DynamicEvent;
using Backend.GameSystems.Equipment;
using Backend.GameSystems.Prestige;
using Backend.GameSystems.Save;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Narration;
using Backend.GameSystems.Exploration.Simulation;
using Backend.GameSystems.LLM;
using Backend.Util;
using Backend.Util.Management;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Backend.GameSystems.Exploration
{
    public class ExplorationManager : SingletonGameObject<ExplorationManager>
    {
        private const float RealSecondsPerTick = 1f;

        private ExplorationSession _session;
        private float _tickAccumulator;

        public ExplorationState CurrentState => _session?.State;
        public bool IsRunning => _session?.State?.IsExploring == true;

        protected override void OnAwake()
        {
            base.OnAwake();
            LlmNarrationManager.EnsureInitialized();
            DynamicEventManager.EnsureInitialized();
            CharacterMemoryManager.EnsureInitialized();
            RelationshipManager.EnsureInitialized();
            ExplorationSessionLogArchive.EnsureInitialized();
            PrestigeManager.EnsureInitialized();
            GameSaveManager.EnsureInitialized();
            GameSaveManager.Load();
            _session = new ExplorationSession(new HybridLogNarrator());
        }

        private void Update()
        {
            if (GameStateUtil.IsQuitting || _session?.State == null)
                return;

            if (!_session.State.IsExploring || _session.State.IsPaused)
                return;

            _tickAccumulator += Time.deltaTime;
            while (_tickAccumulator >= RealSecondsPerTick)
            {
                _tickAccumulator -= RealSecondsPerTick;
                _session.ProcessTick();
            }
        }

        private void StartExploration_Internal(int seed)
        {
            var party = ZoneDefinitions.CreateDefaultParty();
            TrainingGroundManager.ApplyPartyBonuses(party);
            EquipmentService.ClearPartyEquipment(party);
            _session.StartNew(seed, party);
            _session.State.Gold = PrestigeManager.GetStartingGoldBonus();
            ExplorationSessionLogArchive.Clear();
            CharacterMemoryManager.BindParty(party);
            RelationshipManager.BindParty(party);
            _tickAccumulator = 0f;
            ExplorationChannels.PublishStateChanged(_session.State);
            Debug.Log($"[ExplorationManager] Exploration started. Seed={seed}");
        }

        private void ResumeExploration_Internal()
        {
            _session?.Resume();
        }

        private void PauseExploration_Internal()
        {
            _session?.Pause();
        }

        private void ReturnToGuild_Internal()
        {
            _session?.ReturnToGuild();
        }

        private void ProcessOfflineElapsed_Internal()
        {
            if (_session?.State == null)
                return;

            var elapsed = DateTime.UtcNow - _session.State.LastOnlineUtc;
            if (elapsed.TotalSeconds < ExplorationSimulator.TickDurationSeconds)
                return;

            _session.ProcessOffline(elapsed);
        }

#region Static Public Methods
        /// <summary>
        /// 현재 탐험 상태를 반환한다.
        /// </summary>
        public static ExplorationState GetCurrentState()
        {
            if (GameStateUtil.IsQuitting)
                return null;

            return Instance._session?.State;
        }

        /// <summary>
        /// 기본 파티로 새 탐험을 시작한다.
        /// </summary>
        public static void StartExploration(int seed = 0)
        {
            if (GameStateUtil.IsQuitting)
                return;

            var resolvedSeed = seed == 0 ? Environment.TickCount : seed;
            Instance.StartExploration_Internal(resolvedSeed);
        }

        /// <summary>
        /// 탐험 진행을 재개한다.
        /// </summary>
        public static void ResumeExploration()
        {
            if (GameStateUtil.IsQuitting)
                return;

            Instance.ResumeExploration_Internal();
        }

        /// <summary>
        /// 탐험 진행을 일시 정지한다.
        /// </summary>
        public static void PauseExploration()
        {
            if (GameStateUtil.IsQuitting)
                return;

            Instance.PauseExploration_Internal();
        }

        /// <summary>
        /// 수동 귀환을 수행한다.
        /// </summary>
        public static void ReturnToGuild()
        {
            if (GameStateUtil.IsQuitting)
                return;

            Instance.ReturnToGuild_Internal();
        }

        /// <summary>
        /// 마지막 접속 이후 경과 시간을 오프라인 시뮬레이션으로 처리한다.
        /// </summary>
        public static void ProcessOfflineElapsed()
        {
            if (GameStateUtil.IsQuitting)
                return;

            Instance.ProcessOfflineElapsed_Internal();
        }
#endregion
    }
}
