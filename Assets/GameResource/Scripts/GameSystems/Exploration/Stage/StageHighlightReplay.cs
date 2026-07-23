using System;
using System.Collections.Generic;
using Backend.GameSystems.Exploration.Data;

namespace Backend.GameSystems.Exploration.Stage
{
    /// <summary>
    /// 오프라인 복귀 시 상위 이벤트만 짧게 재생 (전체 스테이지 재생 없음).
    /// </summary>
    public static class StageHighlightReplay
    {
        private const int MaxHighlights = 3;

        /// <summary>
        /// Notable 이상 TopEvents 를 스테이지 큐에 넣는다. onBeatComplete 가 null 이면 visual-only.
        /// </summary>
        public static void EnqueueOfflineHighlights(
            IReadOnlyList<ExplorationEvent> topEvents,
            PartyState party,
            Action<ExplorationEvent> onBeatComplete = null)
        {
            if (topEvents == null || topEvents.Count == 0 || party == null)
                return;

            if (StageVfxDensitySettings.Current == StageVfxDensityMode.Low)
                return;

            var enqueued = 0;
            foreach (var explorationEvent in topEvents)
            {
                if (enqueued >= MaxHighlights)
                    break;

                if (explorationEvent.Salience < SalienceGrade.Notable)
                    continue;

                var captured = explorationEvent;
                ExplorationStageSystem.Enqueue(
                    explorationEvent,
                    party,
                    onBeatComplete == null ? static () => { } : () => onBeatComplete(captured));
                enqueued++;
            }
        }
    }
}
