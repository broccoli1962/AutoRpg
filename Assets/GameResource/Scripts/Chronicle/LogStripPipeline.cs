using System;
using System.Collections.Generic;
using R3;

namespace Backend.Chronicle
{
    /// <summary>
    /// 스테이지 연출 확정 이후 로그 스트립에 문장을 append 하는 파이프라인.
    /// </summary>
    public static class LogStripPipeline
    {
        private static readonly Queue<string> _pendingLines = new();
        private static readonly Subject<string> _onAppended = new();

        /// <summary>
        /// 로그 스트립에 한 줄이 append 되었을 때 발행된다.
        /// </summary>
        public static Observable<string> OnAppended => _onAppended;

        /// <summary>
        /// 스테이지 사건을 연출 확정 대기열에 등록한다.
        /// </summary>
        public static void Enqueue(StageLogEvent stageEvent)
        {
            if (stageEvent == null)
                return;

            var line = SalienceRouter.Route(stageEvent);
            if (!string.IsNullOrWhiteSpace(line))
                _pendingLines.Enqueue(line.Trim());
        }

        /// <summary>
        /// 스테이지 연출이 확정된 뒤 호출해 대기 중인 문장을 append 한다.
        /// </summary>
        public static void NotifyPresentationComplete()
        {
            while (_pendingLines.Count > 0)
            {
                var line = _pendingLines.Dequeue();
                _onAppended.OnNext(line);
            }

            var flushed = SalienceRouter.FlushPendingBatch();
            if (!string.IsNullOrWhiteSpace(flushed))
                _onAppended.OnNext(flushed.Trim());
        }

        /// <summary>
        /// 파이프라인과 Salience 누적 상태를 초기화한다.
        /// </summary>
        public static void Reset()
        {
            _pendingLines.Clear();
            SalienceRouter.Reset();
        }
    }
}
