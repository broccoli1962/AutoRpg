using System.Threading;
using Backend.GameSystems.DynamicEvent.Data;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.LLM;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Backend.GameSystems.DynamicEvent.LLM
{
    /// <summary>
    /// 동적 이벤트 장면·결과 연출을 LLM으로 생성한다. JSON 파싱 실패 시 1회 재시도한다.
    /// </summary>
    public static class DynamicEventLlmNarrator
    {
        private const float Temperature = 0.75f;

        public static async UniTask<DynamicEventLlmNarration> TryGenerateSceneAsync(
            DynamicEventTemplate template,
            PartyState party,
            int floor,
            CancellationToken externalCt = default)
        {
            if (template == null || !Application.isPlaying || !LlmQualitySettings.UseDynamicEventLlm)
                return null;

            var sceneMaxTokens = LlmQualitySettings.DynamicSceneMaxTokens;
            if (sceneMaxTokens <= 0)
                return null;

            LlmNarrationManager.EnsureInitialized();
            if (!LlmNarrationManager.IsModelReady)
                return null;

            var prompt = DynamicEventPromptBuilder.BuildScenePrompt(template, party, floor);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
            cts.CancelAfterSlim(System.TimeSpan.FromSeconds(LlmQualitySettings.InferenceTimeoutSeconds));

            var raw = await LlmNarrationManager.GenerateTextAsync(prompt, sceneMaxTokens, Temperature, cts.Token);
            if (TryParseScene(raw, template, out var narration))
            {
                Debug.Log($"[DynamicEventLlmNarrator] Scene LLM ok for {template.EventId}");
                return narration;
            }

            if (string.IsNullOrWhiteSpace(raw))
                return null;

            Debug.LogWarning($"[DynamicEventLlmNarrator] Scene JSON invalid for {template.EventId}, retrying once.");
            var repairPrompt = DynamicEventPromptBuilder.BuildSceneRepairPrompt(raw, template);
            var repaired = await LlmNarrationManager.GenerateTextAsync(
                repairPrompt,
                sceneMaxTokens,
                0.5f,
                cts.Token);

            if (TryParseScene(repaired, template, out narration))
            {
                Debug.Log($"[DynamicEventLlmNarrator] Scene LLM retry ok for {template.EventId}");
                return narration;
            }

            Debug.LogWarning($"[DynamicEventLlmNarrator] Scene JSON retry failed for {template.EventId}, fallback.");
            return null;
        }

        public static async UniTask<string> TryGenerateResultAsync(
            DynamicEventTemplate template,
            PartyState party,
            string choiceId,
            DynamicEventOutcomeEffect outcome,
            CancellationToken externalCt = default)
        {
            if (template == null || !Application.isPlaying || !LlmQualitySettings.UseDynamicEventResultLlm)
                return null;

            var resultMaxTokens = LlmQualitySettings.DynamicResultMaxTokens;
            if (resultMaxTokens <= 0)
                return null;

            LlmNarrationManager.EnsureInitialized();
            if (!LlmNarrationManager.IsModelReady)
                return null;

            var prompt = DynamicEventPromptBuilder.BuildResultPrompt(template, party, choiceId, outcome);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
            cts.CancelAfterSlim(System.TimeSpan.FromSeconds(LlmQualitySettings.InferenceTimeoutSeconds));

            var raw = await LlmNarrationManager.GenerateTextAsync(prompt, resultMaxTokens, Temperature, cts.Token);
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            var trimmed = raw.Trim();
            Debug.Log($"[DynamicEventLlmNarrator] Result LLM ok for {template.EventId}: chars={trimmed.Length}");
            return trimmed;
        }

        private static bool TryParseScene(string raw, DynamicEventTemplate template, out DynamicEventLlmNarration narration)
        {
            narration = null;
            if (string.IsNullOrWhiteSpace(raw))
                return false;

            return DynamicEventLlmParser.TryParseSceneJson(raw, template, out narration);
        }
    }
}
