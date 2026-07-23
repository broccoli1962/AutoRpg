using System;
using System.Collections.Generic;
using Backend.GameSystems.Exploration.Data;
using R3;

namespace Backend.GameSystems.Exploration.Stage
{
    /// <summary>
    /// 시뮬 이벤트 → 스테이지 연출 큐. 연출·오버레이 중에는 탐험 Tick 진행을 막는다.
    /// </summary>
    public static class ExplorationStageSystem
    {
        private static readonly Queue<StageBeatRequest> Queue = new();
        private static readonly Subject<StageBeatRequest> BeatStartedSubject = new();
        private static StageBeatRequest _active;
        private static bool _directorReady;
        private static bool _overlayHold;

        /// <summary>연출 큐·재생·오버레이 홀드 중이면 true.</summary>
        public static bool IsBusy => _overlayHold || _active != null || Queue.Count > 0;

        public static bool IsOverlayHold => _overlayHold;

        public static Observable<StageBeatRequest> OnBeatStarted => BeatStartedSubject;

        public static void SetDirectorReady(bool ready)
        {
            _directorReady = ready;
            if (ready)
                TryStartNext();
        }

        /// <summary>동적 이벤트 팝업 등 — 스테이지 연출 일시 중단.</summary>
        public static void SetOverlayHold(bool hold)
        {
            _overlayHold = hold;
            if (hold)
                return;

            TryStartNext();
        }

        public static void Enqueue(ExplorationEvent explorationEvent, PartyState party, Action publishLog)
        {
            if (explorationEvent == null || publishLog == null)
                return;

            Queue.Enqueue(new StageBeatRequest(explorationEvent, party, publishLog));
            TryStartNext();
        }

        public static void EnqueueCombatBatch(StageCombatBatch batch)
        {
            if (batch == null || batch.Count == 0 || batch.PublishLogs == null)
                return;

            if (batch.Count == 1)
            {
                Enqueue(batch.Events[0], batch.Party, batch.PublishLogs);
                return;
            }

            Queue.Enqueue(new StageBeatRequest(batch));
            TryStartNext();
        }

        /// <summary>오버레이 등으로 현재 비트를 버리고 큐만 유지.</summary>
        public static void AbortCurrentBeat()
        {
            _active = null;
        }

        public static void CompleteCurrentBeat()
        {
            _active?.PublishLog?.Invoke();
            _active = null;
            TryStartNext();
        }

        public static void Clear()
        {
            Queue.Clear();
            _active = null;
            _overlayHold = false;
        }

        private static void TryStartNext()
        {
            if (!_directorReady || _overlayHold || _active != null || Queue.Count == 0)
                return;

            _active = Queue.Dequeue();
            BeatStartedSubject.OnNext(_active);
        }
    }
}
