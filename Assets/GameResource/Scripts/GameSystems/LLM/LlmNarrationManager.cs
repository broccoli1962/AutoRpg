using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Backend.GameSystems.Exploration;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Narration;
using Backend.Util;
using Backend.Util.Management;
using Cysharp.Threading.Tasks;
using LLama.Sampling;
using UnityEngine;

namespace Backend.GameSystems.LLM
{
    /// <summary>
    /// 로컬 LLM 추론 큐를 관리한다. 메인 스레드를 블로킹하지 않고 백그라운드에서 순차 처리하며,
    /// 캐시 재사용·스트리밍·타임아웃·템플릿 fallback을 제공한다.
    /// </summary>
    public sealed class LlmNarrationManager : SingletonGameObject<LlmNarrationManager>
    {
        private const string ModelFileName = "Qwen2.5-1.5B-Instruct-Q4_K_M.gguf";
        private const float InferenceTimeoutSeconds = 8f;
        private const int MaxTokens = 128;

        private readonly Queue<LlmNarrationJob> _queue = new();
        private readonly SemaphoreSlim _inferenceLock = new(1, 1);
        private LlamaInferenceService _service;
        private bool _isModelReady;
        private bool _isModelLoading;
        private bool _isProcessing;

        public static bool IsModelReady => !GameStateUtil.IsQuitting && Instance._isModelReady;

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
        /// Salience Significant+ 이벤트를 LLM 큐에 등록한다.
        /// </summary>
        public static void EnqueueJob(LlmNarrationJob job)
        {
            if (GameStateUtil.IsQuitting || job == null)
                return;

            Instance.Enqueue(job);
        }

        /// <summary>
        /// 동적 이벤트 등 단발성 LLM 호출용. 로그 큐와 동일한 추론 락을 공유한다.
        /// </summary>
        public static async UniTask<string> GenerateTextAsync(
            string prompt,
            int maxTokens,
            float temperature,
            CancellationToken cancellationToken = default)
        {
            return await GenerateTextAsync(prompt, maxTokens, temperature, null, cancellationToken);
        }

        public static async UniTask<string> GenerateTextAsync(
            string prompt,
            int maxTokens,
            float temperature,
            Grammar grammar,
            CancellationToken cancellationToken = default)
        {
            if (GameStateUtil.IsQuitting || string.IsNullOrWhiteSpace(prompt))
                return null;

            return await Instance.GenerateTextInternalAsync(prompt, maxTokens, temperature, grammar, cancellationToken);
        }

        private async UniTask<string> GenerateTextInternalAsync(
            string prompt,
            int maxTokens,
            float temperature,
            Grammar grammar,
            CancellationToken cancellationToken)
        {
            if (!_isModelReady || _service == null)
                return null;

            await _inferenceLock.WaitAsync(cancellationToken);
            try
            {
                return await UniTask.RunOnThreadPool(async () =>
                    await _service.GenerateAsync(prompt, maxTokens, temperature, null, grammar, cancellationToken));
            }
            finally
            {
                _inferenceLock.Release();
            }
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
            if (NarrationResultCache.TryGetCached(job.Event, job.Party, out var cachedText))
            {
                job.PendingEntry.Text = cachedText;
                job.PendingEntry.IsPending = false;
                job.PendingEntry.UsedLlm = true;

                await UniTask.SwitchToMainThread();
                if (!GameStateUtil.IsQuitting)
                    ExplorationChannels.PublishLogUpdated(job.PendingEntry);

                LlmNarrationBenchmark.RecordSuccess(0);
                Debug.Log($"[LlmNarrationManager] Cache hit for {job.Event.EventId}");
                return;
            }

            if (!_isModelReady || _service == null)
            {
                await PublishFallbackAsync(job, 0);
                return;
            }

            var prompt = LogPromptBuilder.BuildLogPrompt(job.Event, job.Party);
            var maxTokens = LlmQualitySettings.GetLogMaxTokens(job.Event.Salience) +
                            ExplorationNarrationBonus.GetExtraLogMaxTokens(job.Party);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            string resultText = null;
            var timedOut = false;
            var accumulated = new StringBuilder();

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(LlmQualitySettings.InferenceTimeoutSeconds));
                await _inferenceLock.WaitAsync(cts.Token);
                try
                {
                    resultText = await UniTask.RunOnThreadPool(async () =>
                        await _service.GenerateAsync(
                            prompt,
                            maxTokens,
                            0.8f,
                            token => PublishStreamingToken(job, accumulated, token),
                            null,
                            cts.Token));
                }
                finally
                {
                    _inferenceLock.Release();
                }
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
                await PublishFallbackAsync(job, sw.ElapsedMilliseconds);
                return;
            }

            var trimmed = resultText.Trim();
            NarrationResultCache.Store(job.Event, job.Party, trimmed);

            Debug.Log(
                $"[LlmNarrationManager] Generated for {job.Event.EventId}: " +
                $"elapsedMs={sw.ElapsedMilliseconds}, chars={trimmed.Length}");

            job.PendingEntry.Text = trimmed;
            job.PendingEntry.IsPending = false;
            job.PendingEntry.UsedLlm = true;

            LlmNarrationBenchmark.RecordSuccess(sw.ElapsedMilliseconds);

            await UniTask.SwitchToMainThread();
            if (!GameStateUtil.IsQuitting)
                ExplorationChannels.PublishLogUpdated(job.PendingEntry);
        }

        private static void PublishStreamingToken(LlmNarrationJob job, StringBuilder accumulated, string token)
        {
            accumulated.Append(token);
            PublishStreamingOnMainThread(job, accumulated.ToString()).Forget();
        }

        private static async UniTaskVoid PublishStreamingOnMainThread(LlmNarrationJob job, string snapshot)
        {
            await UniTask.SwitchToMainThread();
            if (GameStateUtil.IsQuitting)
                return;

            job.PendingEntry.Text = snapshot;
            job.PendingEntry.IsPending = true;
            ExplorationChannels.PublishLogStreaming(job.PendingEntry);
        }

        private static async UniTask PublishFallbackAsync(LlmNarrationJob job, long elapsedMs)
        {
            job.PendingEntry.Text = job.FallbackEntry.Text;
            job.PendingEntry.IsPending = false;
            job.PendingEntry.UsedLlm = false;

            LlmNarrationBenchmark.RecordFallback(elapsedMs);

            await UniTask.SwitchToMainThread();
            if (!GameStateUtil.IsQuitting)
                ExplorationChannels.PublishLogUpdated(job.PendingEntry);
        }
    }
}
