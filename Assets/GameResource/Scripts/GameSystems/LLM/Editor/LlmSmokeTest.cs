using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Backend.GameSystems.LLM.EditorTools
{
    /// <summary>
    /// LlamaSharp 로컬 추론이 정상 동작하는지 검증하는 에디터 전용 스모크 테스트.
    /// 메뉴: AbyssChronicle/LLM/Run Smoke Test
    /// 추론은 백그라운드 스레드에서 실행되어 에디터 메인 스레드를 블로킹하지 않는다.
    /// </summary>
    public static class LlmSmokeTest
    {
        private const string ModelFileName = "Qwen2.5-1.5B-Instruct-Q4_K_M.gguf";

        [MenuItem("AbyssChronicle/LLM/Run Smoke Test")]
        public static void RunSmokeTest()
        {
            var modelPath = Path.Combine(Application.streamingAssetsPath, "Models", ModelFileName);
            if (!File.Exists(modelPath))
            {
                Debug.LogError($"[LlmSmokeTest] Model not found: {modelPath}");
                return;
            }

            Debug.Log("[LlmSmokeTest] Started. Running inference on a background thread...");
            _ = Task.Run(async () => await RunAsync(modelPath));
        }

        private static async Task RunAsync(string modelPath)
        {
            try
            {
                using var service = new LlamaInferenceService();

                var sw = Stopwatch.StartNew();
                await service.LoadAsync(modelPath);
                var loadMs = sw.ElapsedMilliseconds;

                const string prompt =
                    "<|im_start|>system\n당신은 판타지 RPG의 이야기꾼입니다. 짧고 생생하게 한국어로 묘사하세요.<|im_end|>\n" +
                    "<|im_start|>user\n검사 '레온'이 어두운 던전 입구에 도착했다. 한 문장으로 묘사해줘.<|im_end|>\n" +
                    "<|im_start|>assistant\n";

                sw.Restart();
                var result = await service.GenerateAsync(prompt, maxTokens: 96, temperature: 0.7f);
                var genMs = sw.ElapsedMilliseconds;

                Debug.Log(
                    $"[LlmSmokeTest] SUCCESS. Load={loadMs}ms, Generate={genMs}ms\n" +
                    $"--- OUTPUT ---\n{result.Trim()}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[LlmSmokeTest] FAILED: {e.GetType().Name}: {e.Message}\n{e}");
            }
        }
    }
}
