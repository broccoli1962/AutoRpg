using UnityEngine;

namespace Backend.GameSystems.LLM
{
    /// <summary>
    /// Phase 3 PoC — 로컬 LLM 내레이션 응답 시간·성공률 집계.
    /// </summary>
    public static class LlmNarrationBenchmark
    {
        private static int _completedJobs;
        private static int _successJobs;
        private static int _fallbackJobs;
        private static long _totalElapsedMs;

        public static int CompletedJobs => _completedJobs;
        public static int SuccessJobs => _successJobs;
        public static int FallbackJobs => _fallbackJobs;

        public static float AverageElapsedMs =>
            _completedJobs <= 0 ? 0f : (float)_totalElapsedMs / _completedJobs;

        public static void RecordSuccess(long elapsedMs)
        {
            _completedJobs++;
            _successJobs++;
            _totalElapsedMs += elapsedMs;
            MaybeLogSummary();
        }

        public static void RecordFallback(long elapsedMs)
        {
            _completedJobs++;
            _fallbackJobs++;
            _totalElapsedMs += elapsedMs;
            MaybeLogSummary();
        }

        public static void Reset()
        {
            _completedJobs = 0;
            _successJobs = 0;
            _fallbackJobs = 0;
            _totalElapsedMs = 0;
        }

        private static void MaybeLogSummary()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_completedJobs % 5 != 0)
                return;

            Debug.Log($"[LlmNarrationBenchmark] jobs={_completedJobs} ok={_successJobs} fallback={_fallbackJobs} avgMs={AverageElapsedMs:0}");
#endif
        }
    }
}
