using System.Text;
using Backend.GameSystems.Exploration;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Simulation;
using Backend.GameSystems.Prestige.Data;
using Backend.GameSystems.Save;
using Backend.Util;
using Backend.Util.Management;
using R3;
using UnityEngine;

namespace Backend.GameSystems.Prestige
{
    /// <summary>
    /// MVP 프레스티지(귀환) 시스템. 탐험 종료 시 유산 포인트를 누적하고 다음 회차 보너스를 제공한다.
    /// </summary>
    public sealed class PrestigeManager : SingletonGameObject<PrestigeManager>
    {
        private readonly MetaProgressionState _meta = new();
        private CompositeDisposable _disposables;

        public MetaProgressionState Meta => _meta;

        public static MetaProgressionState GetMeta()
        {
            if (GameStateUtil.IsQuitting)
                return null;

            return Instance._meta;
        }

        public static void EnsureInitialized()
        {
            if (GameStateUtil.IsQuitting)
                return;

            _ = Instance;
        }

        public static int GetStartingGoldBonus()
        {
            if (GameStateUtil.IsQuitting)
                return 0;

            return Instance._meta.LegacyPoints * 8;
        }

        public static void ImportMeta(MetaProgressionState meta)
        {
            if (GameStateUtil.IsQuitting || meta == null)
                return;

            Instance._meta.LegacyPoints = meta.LegacyPoints;
            Instance._meta.PrestigeCount = meta.PrestigeCount;
            Instance._meta.DeepestFloorReached = meta.DeepestFloorReached;
            Instance._meta.ChronicleEntries = meta.ChronicleEntries ?? new System.Collections.Generic.List<string>();
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            _disposables = new CompositeDisposable();

            ExplorationChannels.OnExplorationEnded
                .Subscribe(OnExplorationEnded)
                .AddTo(_disposables);
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }

        private void OnExplorationEnded(ExplorationEndReason reason)
        {
            var state = ExplorationManager.GetCurrentState();
            if (state == null)
                return;

            ProcessRunEnd(reason, state);
        }

        private void ProcessRunEnd(ExplorationEndReason reason, ExplorationState state)
        {
            if (state.CurrentFloor < 2 && reason == ExplorationEndReason.PartyDefeated)
                return;

            var legacyGain = CalculateLegacyGain(state, reason);
            if (legacyGain <= 0)
                return;

            _meta.LegacyPoints += legacyGain;
            _meta.PrestigeCount++;
            _meta.DeepestFloorReached = Mathf.Max(_meta.DeepestFloorReached, state.CurrentFloor);
            _meta.ChronicleEntries.Add(BuildChronicleEntry(state, reason, legacyGain, _meta.PrestigeCount));

            Debug.Log(
                $"[PrestigeManager] Prestige #{_meta.PrestigeCount}: +{legacyGain} legacy " +
                $"(total={_meta.LegacyPoints}, floor={state.CurrentFloor}, reason={reason})");

            GameSaveManager.Save();
            ScheduleAutoRestart().Forget();
        }

        private static int CalculateLegacyGain(ExplorationState state, ExplorationEndReason reason)
        {
            var basePoints = Mathf.Max(1, state.CurrentFloor / 2);
            return reason switch
            {
                ExplorationEndReason.ZoneComplete => basePoints + 6,
                ExplorationEndReason.ManualReturn => basePoints + 1,
                ExplorationEndReason.PartyDefeated => basePoints,
                _ => 0
            };
        }

        private static string BuildChronicleEntry(
            ExplorationState state,
            ExplorationEndReason reason,
            int legacyGain,
            int prestigeCount)
        {
            var builder = new StringBuilder();
            builder.Append("제 ");
            builder.Append(prestigeCount);
            builder.Append("회차: ");
            builder.Append(ZoneDefinitions.GetZoneDisplayName(state.ZoneId));
            builder.Append(' ');
            builder.Append(state.CurrentFloor);
            builder.Append("층까지 진행");

            builder.Append(reason switch
            {
                ExplorationEndReason.ZoneComplete => " 후 구역을 정복했다.",
                ExplorationEndReason.ManualReturn => " 후 귀환했다.",
                ExplorationEndReason.PartyDefeated => " 중 전멸했다.",
                _ => "."
            });

            builder.Append(" (+");
            builder.Append(legacyGain);
            builder.Append(" 유산)");
            return builder.ToString();
        }

        private async Cysharp.Threading.Tasks.UniTaskVoid ScheduleAutoRestart()
        {
            await Cysharp.Threading.Tasks.UniTask.Delay(System.TimeSpan.FromSeconds(1.5f));
            if (GameStateUtil.IsQuitting)
                return;

            ExplorationManager.StartExploration();
        }
    }
}
