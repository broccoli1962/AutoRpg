using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Backend.GameSystems.Exploration;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Narration;
using Backend.Util;
using Backend.Util.Management;
using Cysharp.Threading.Tasks;
using ExplorationEventType = Backend.GameSystems.Exploration.Data.EventType;
using UnityEngine;

namespace Backend.GameSystems.LLM
{
    /// <summary>
    /// 로컬 LLM 추론 큐를 관리한다. 메인 스레드를 블로킹하지 않고 백그라운드에서 순차 처리하며,
    /// 타임아웃·실패 시 템플릿 fallback으로 전환한다. (Phase 2 PoC)
    /// </summary>
    public sealed class LlmNarrationManager : SingletonGameObject<LlmNarrationManager>
    {
        private const string ModelFileName = "Qwen2.5-1.5B-Instruct-Q4_K_M.gguf";
        private const float InferenceTimeoutSeconds = 8f;
        private const int MaxTokens = 128;

        private readonly Queue<LlmNarrationJob> _queue = new();
        private LlamaInferenceService _service;
        private bool _isModelReady;
        private bool _isModelLoading;
        private bool _isProcessing;

        public bool IsModelReady => _isModelReady;

        protected override void OnAwake()
        {
            base.OnAwake();
            LoadModelAsync().Forget();
        }

        private void OnApplicationQuit()
        {
            _service?.Dispose();
            _service = null;
        }

        /// <summary>
        /// 싱글톤을 미리 생성해 모델 로드를 시작한다.
        /// </summary>
        public static void EnsureInitialized()
        {
            if (GameStateUtil.IsQuitting)
                return;

            _ = Instance;
        }

        /// <summary>
        /// Significant 이상 전투 로그를 LLM 큐에 등록한다.
        /// </summary>
        public static void EnqueueJob(LlmNarrationJob job)
        {
            if (GameStateUtil.IsQuitting || job == null)
                return;

            Instance.Enqueue(job);
        }

        /// <summary>
        /// Phase 2 PoC: 전투 결과 + Significant 이상만 LLM 대상.
        /// </summary>
        public static bool ShouldUseLlm(ExplorationEvent explorationEvent)
        {
            return explorationEvent.EventType == ExplorationEventType.CombatResult &&
                   explorationEvent.Salience >= SalienceGrade.Significant;
        }

        private void Enqueue(LlmNarrationJob job)
        {
            lock (_queue)
            {
                _queue.Enqueue(job);
            }

            ProcessQueueAsync().Forget();
        }

        private async UniTaskVoid LoadModelAsync()
        {
            if (_isModelLoading || _isModelReady)
                return;

            _isModelLoading = true;
            var modelPath = Path.Combine(Application.streamingAssetsPath, "Models", ModelFileName);

            if (!File.Exists(modelPath))
            {
                Debug.LogWarning($"[LlmNarrationManager] Model not found: {modelPath}. LLM narration disabled.");
                _isModelLoading = false;
                return;
            }

            try
            {
                await UniTask.RunOnThreadPool(async () =>
                {
                    _service = new LlamaInferenceService();
                    await _service.LoadAsync(modelPath);
                });

                _isModelReady = true;
                Debug.Log("[LlmNarrationManager] Model ready for narration queue.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[LlmNarrationManager] Model load failed: {e.Message}");
            }
            finally
            {
                _isModelLoading = false;
            }
        }

        private async UniTaskVoid ProcessQueueAsync()
        {
            if (_isProcessing)
                return;

            _isProcessing = true;

            while (true)
            {
                LlmNarrationJob job;
                lock (_queue)
                {
                    if (_queue.Count == 0)
                    {
                        _isProcessing = false;
                        return;
                    }

                    job = _queue.Dequeue();
                }

                await ProcessJobAsync(job);
            }
        }

        private async UniTask ProcessJobAsync(LlmNarrationJob job)
        {
            if (!_isModelReady || _service == null)
            {
                await PublishFallbackAsync(job);
                return;
            }

            var prompt = LogPromptBuilder.BuildCombatLogPrompt(job.Event, job.Party);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            string resultText = null;
            var timedOut = false;

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(InferenceTimeoutSeconds));
                resultText = await UniTask.RunOnThreadPool(async () =>
                    await _service.GenerateAsync(prompt, MaxTokens, 0.8f, null, cts.Token));
            }
            catch (OperationCanceledException)
            {
                timedOut = true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LlmNarrationManager] Inference failed for {job.Event.EventId}: {e.Message}");
            }

            sw.Stop();

            if (timedOut || string.IsNullOrWhiteSpace(resultText))
            {
                Debug.LogWarning(
                    $"[LlmNarrationManager] Fallback for {job.Event.EventId} " +
                    $"(timedOut={timedOut}, elapsedMs={sw.ElapsedMilliseconds})");
                await PublishFallbackAsync(job);
                return;
            }

            var trimmed = resultText.Trim();
            Debug.Log(
                $"[LlmNarrationManager] Generated for {job.Event.EventId}: " +
                $"elapsedMs={sw.ElapsedMilliseconds}, chars={trimmed.Length}");

            job.PendingEntry.Text = trimmed;
            job.PendingEntry.IsPending = false;
            job.PendingEntry.UsedLlm = true;

            await UniTask.SwitchToMainThread();
            if (!GameStateUtil.IsQuitting)
                ExplorationChannels.PublishLogUpdated(job.PendingEntry);
        }

        private static async UniTask PublishFallbackAsync(LlmNarrationJob job)
        {
            job.PendingEntry.Text = job.FallbackEntry.Text;
            job.PendingEntry.IsPending = false;
            job.PendingEntry.UsedLlm = false;

            await UniTask.SwitchToMainThread();
            if (!GameStateUtil.IsQuitting)
                ExplorationChannels.PublishLogUpdated(job.PendingEntry);
        }
    }
}
