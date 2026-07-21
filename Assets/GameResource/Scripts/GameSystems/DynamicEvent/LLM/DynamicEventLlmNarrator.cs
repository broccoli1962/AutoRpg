using System.Threading;
using Backend.GameSystems.DynamicEvent.Data;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.LLM;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Backend.GameSystems.DynamicEvent.LLM
{
    /// <summary>
    /// 동적 이벤트 장면·결과 연출을 LLM으로 생성한다. 실패 시 null을 반환한다.
    /// </summary>
    public static class DynamicEventLlmNarrator
    {
        private const int SceneMaxTokens = 220;
        private const int ResultMaxTokens = 96;
        private const float Temperature = 0.75f;
        private const float InferenceTimeoutSeconds = 10f;

        public static async UniTask<DynamicEventLlmNarration> TryGenerateSceneAsync(
            DynamicEventTemplate template,
            PartyState party,
            int floor,
            CancellationToken externalCt = default)
        {
            if (template == null || !Application.isPlaying)
                return null;

            LlmNarrationManager.EnsureInitialized();
            if (!LlmNarrationManager.IsModelReady)
                return null;

            var prompt = DynamicEventPromptBuilder.BuildScenePrompt(template, party, floor);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
            cts.CancelAfterSlim(System.TimeSpan.FromSeconds(InferenceTimeoutSeconds));

            var raw = await LlmNarrationManager.GenerateTextAsync(prompt, SceneMaxTokens, Temperature, cts.Token);
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            if (DynamicEventLlmParser.TryParseSceneJson(raw, template, out var narration))
            {
                Debug.Log($"[DynamicEventLlmNarrator] Scene LLM ok for {template.EventId}");
                return narration;
            }

            Debug.LogWarning($"[DynamicEventLlmNarrator] Scene JSON invalid for {template.EventId}, fallback.");
            return null;
        }

        public static async UniTask<string> TryGenerateResultAsync(
            DynamicEventTemplate template,
            PartyState party,
            string choiceId,
            DynamicEventOutcomeEffect outcome,
            CancellationToken externalCt = default)
        {
            if (template == null || !Application.isPlaying)
                return null;

            LlmNarrationManager.EnsureInitialized();
            if (!LlmNarrationManager.IsModelReady)
                return null;

            var prompt = DynamicEventPromptBuilder.BuildResultPrompt(template, party, choiceId, outcome);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
            cts.CancelAfterSlim(System.TimeSpan.FromSeconds(InferenceTimeoutSeconds));

            var raw = await LlmNarrationManager.GenerateTextAsync(prompt, ResultMaxTokens, Temperature, cts.Token);
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            var trimmed = raw.Trim();
            Debug.Log($"[DynamicEventLlmNarrator] Result LLM ok for {template.EventId}: chars={trimmed.Length}");
            return trimmed;
        }
    }
}
