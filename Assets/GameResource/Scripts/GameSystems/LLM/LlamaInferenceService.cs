using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using LLama;
using LLama.Common;
using LLama.Sampling;
using UnityEngine;

namespace Backend.GameSystems.LLM
{
    /// <summary>
    /// LlamaSharp(llama.cpp) 기반 로컬 LLM 추론 서비스. (Phase 2 PoC)
    /// StreamingAssets/Models 하위 GGUF 모델을 로드하여 단발성 텍스트를 생성한다.
    /// 추론은 CPU 기준이며, 호출자가 반드시 백그라운드 스레드에서 실행해 메인 스레드 블로킹을 피해야 한다.
    /// </summary>
    public sealed class LlamaInferenceService : IDisposable
    {
        private static readonly string ImEnd = "<|" + "im_end" + "|>";

        private LLamaWeights _weights;
        private ModelParams _modelParams;
        private bool _isLoaded;

        public bool IsLoaded => _isLoaded;

        /// <summary>
        /// GGUF 모델을 비동기로 로드한다. GpuLayerCount=0 으로 CPU 전용 추론을 사용한다.
        /// </summary>
        public async UniTask LoadAsync(string modelPath, uint contextSize = 2048, CancellationToken ct = default)
        {
            if (_isLoaded)
                return;

            _modelParams = new ModelParams(modelPath)
            {
                ContextSize = contextSize,
                GpuLayerCount = 0,
            };

            _weights = await LLamaWeights.LoadFromFileAsync(_modelParams, ct);
            _isLoaded = true;
            Debug.Log($"[LlamaInferenceService] Model loaded. parameters={_weights.ParameterCount}, contextSize={contextSize}");
        }

        /// <summary>
        /// 프롬프트로부터 텍스트를 생성한다. onToken 이 지정되면 토큰 스트리밍마다 호출한다.
        /// </summary>
        public async UniTask<string> GenerateAsync(
            string prompt,
            int maxTokens = 128,
            float temperature = 0.7f,
            Action<string> onToken = null,
            CancellationToken ct = default)
        {
            if (!_isLoaded)
                throw new InvalidOperationException("[LlamaInferenceService] Model is not loaded. Call LoadAsync first.");

            var executor = new StatelessExecutor(_weights, _modelParams);

            var inferenceParams = new InferenceParams
            {
                MaxTokens = maxTokens,
                AntiPrompts = new[] { ImEnd, "<|endoftext|>" },
                SamplingPipeline = new DefaultSamplingPipeline
                {
                    Temperature = temperature,
                },
            };

            var builder = new StringBuilder();
            await foreach (var token in executor.InferAsync(prompt, inferenceParams, ct))
            {
                builder.Append(token);
                onToken?.Invoke(token);
            }

            return builder.ToString();
        }

        public void Dispose()
        {
            _weights?.Dispose();
            _weights = null;
            _isLoaded = false;
        }
    }
}
