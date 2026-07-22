using System.Text;
using Backend.GameSystems.Character;
using Backend.GameSystems.DynamicEvent.Data;
using Backend.GameSystems.Exploration.Data;

namespace Backend.GameSystems.LLM
{
    /// <summary>
    /// 동적 이벤트 LLM 연출용 JSON 출력 프롬프트를 생성한다. (Phase 4)
    /// </summary>
    public static class DynamicEventPromptBuilder
    {
        private static readonly string ImEnd = "<|" + "im_end" + "|>";

        private const string SystemPrompt =
            "당신은 판타지 동굴 탐험 게임 《심연기록》의 이벤트 연출 작가입니다.\n" +
            "제시된 이벤트 뼈대의 선택지 id와 의미를 유지하면서, 한국어로 장면을 연출하세요.\n\n" +
            "규칙:\n" +
            "- 반드시 아래 JSON 스키마만 출력하세요. 다른 텍스트를 포함하지 마세요.\n" +
            "- choices 배열의 id는 뼈대와 동일해야 합니다.\n" +
            "- 게임 밸런스 수치를 임의로 바꾸지 마세요.";

        public static string BuildScenePrompt(DynamicEventTemplate template, PartyState party, int floor)
        {
            var leader = party?.Leader;
            var user = new StringBuilder();
            user.AppendLine("[이벤트 뼈대]");
            user.Append("event_id: ");
            user.AppendLine(template.EventId);
            user.Append("category: ");
            user.AppendLine(template.Category.ToString());
            user.Append("floor: ");
            user.AppendLine(floor.ToString());
            user.Append("leader: ");
            user.AppendLine(leader?.DisplayName ?? "탐험대");

            if (leader != null)
            {
                var memoryContext = CharacterMemoryManager.BuildPromptContext(leader.CharacterId);
                if (!string.IsNullOrEmpty(memoryContext))
                {
                    user.AppendLine(memoryContext);
                }
            }

            user.AppendLine("choices:");
            foreach (var choice in template.Choices)
            {
                user.Append("- id: ");
                user.AppendLine(choice.Id);
            }

            user.AppendLine();
            user.AppendLine("[출력 JSON 스키마]");
            user.AppendLine("{");
            user.AppendLine("  \"narration\": \"상황 묘사 (2~4문장)\",");
            user.AppendLine("  \"choices\": [");
            user.AppendLine("    { \"id\": \"choice_id\", \"text\": \"선택지 문구\" }");
            user.AppendLine("  ]");
            user.AppendLine("}");

            return
                "<|im_start|>system\n" + SystemPrompt + "\n" + ImEnd + "\n" +
                "<|im_start|>user\n" + user + ImEnd + "\n" +
                "<|im_start|>assistant\n";
        }

        public static string BuildResultPrompt(
            DynamicEventTemplate template,
            PartyState party,
            string choiceId,
            DynamicEventOutcomeEffect outcome)
        {
            var leader = party?.Leader;
            var user = new StringBuilder();
            user.AppendLine("[이벤트 결과 연출]");
            user.Append("event_id: ");
            user.AppendLine(template.EventId);
            user.Append("leader: ");
            user.AppendLine(leader?.DisplayName ?? "탐험대");
            user.Append("choice_id: ");
            user.AppendLine(choiceId);
            user.Append("outcome: ");
            user.AppendLine(outcome.ToString());
            user.AppendLine();
            user.AppendLine("선택 결과를 1~2문장 한국어로 서술하세요. JSON 없이 문장만 출력하세요.");

            return
                "<|im_start|>system\n" + SystemPrompt + "\n" + ImEnd + "\n" +
                "<|im_start|>user\n" + user + ImEnd + "\n" +
                "<|im_start|>assistant\n";
        }

        public static string BuildSceneRepairPrompt(string invalidJson, DynamicEventTemplate template)
        {
            var user = new StringBuilder();
            user.AppendLine("[JSON 수정 요청]");
            user.AppendLine("아래 출력은 JSON 스키마를 따르지 않았습니다. 유효한 JSON만 다시 출력하세요.");
            user.AppendLine(invalidJson);
            user.AppendLine();
            user.AppendLine("필수 choice id:");
            foreach (var choice in template.Choices)
            {
                user.Append("- ");
                user.AppendLine(choice.Id);
            }

            return
                "<|im_start|>system\n" + SystemPrompt + "\n" + ImEnd + "\n" +
                "<|im_start|>user\n" + user + ImEnd + "\n" +
                "<|im_start|>assistant\n";
        }
    }
}
