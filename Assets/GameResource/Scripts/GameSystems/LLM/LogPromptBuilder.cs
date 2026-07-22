using System.Collections.Generic;
using System.Text;
using Backend.GameSystems.Character;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Narration;

namespace Backend.GameSystems.LLM
{
    /// <summary>
    /// 기획서 10_LLM프롬프트설계.md 표준 구조에 따라 탐험 로그용 Qwen 채팅 프롬프트를 조립한다.
    /// </summary>
    public static class LogPromptBuilder
    {
        private static readonly string ImEnd = "<|" + "im_end" + "|>";

        private const string SystemPrompt =
            "당신은 판타지 동굴 탐험 게임 《심연기록》의 전속 서기(書記)입니다.\n" +
            "탐험가들이 겪은 사건을 한국어로, 짧고 생동감 있는 문장으로 서술하세요.\n\n" +
            "규칙:\n" +
            "- 절대 게임 시스템, AI, 프롬프트, 메타적 표현을 언급하지 마세요.\n" +
            "- 제시된 사건의 결과(승패, 획득량, 수치)를 임의로 바꾸거나 새로운 사실을 창작하지 마세요.\n" +
            "- 2~4문장 이내로 작성하세요.\n" +
            "- 과도한 폭력적 묘사나 선정적 표현은 피하세요.\n" +
            "- 구역 분위기(습하고 어둑한 동굴)를 반영하세요.";

        /// <summary>
        /// Salience Significant+ 이벤트용 프롬프트를 이벤트 타입에 맞게 생성한다.
        /// </summary>
        public static string BuildLogPrompt(ExplorationEvent explorationEvent, PartyState party)
        {
            return explorationEvent.EventType switch
            {
                EventType.CombatResult => BuildCombatLogPrompt(explorationEvent, party),
                EventType.Discovery => BuildDiscoveryLogPrompt(explorationEvent, party),
                EventType.FloorClear => BuildFloorClearLogPrompt(explorationEvent, party),
                _ => BuildGenericLogPrompt(explorationEvent, party)
            };
        }

        /// <summary>
        /// Phase 2 PoC: 전투 결과(CombatResult) 이벤트용 프롬프트를 생성한다.
        /// </summary>
        public static string BuildCombatLogPrompt(ExplorationEvent explorationEvent, PartyState party)
        {
            var userContext = new StringBuilder();
            AppendWorldAndCharacterContext(userContext, explorationEvent, party);
            userContext.AppendLine("[이벤트 컨텍스트]");
            userContext.AppendLine(BuildCombatContextJson(explorationEvent));
            userContext.AppendLine();
            userContext.AppendLine("[출력 형식 지시]");
            userContext.Append("2~4문장, 한국어, 3인칭 서술. 결과 수치를 바꾸지 말 것.");
            return BuildChatPrompt(userContext.ToString());
        }

        public static string BuildDiscoveryLogPrompt(ExplorationEvent explorationEvent, PartyState party)
        {
            var userContext = new StringBuilder();
            AppendWorldAndCharacterContext(userContext, explorationEvent, party);
            userContext.AppendLine("[이벤트 컨텍스트]");
            userContext.Append("{ \"event_type\": \"discovery\", \"item\": \"");
            userContext.Append(explorationEvent.DiscoveryDisplayName ?? "보물");
            userContext.Append("\", \"gold_gained\": ");
            userContext.Append(explorationEvent.GoldDelta);
            userContext.AppendLine(" }");
            userContext.AppendLine();
            userContext.AppendLine("[출력 형식 지시]");
            userContext.Append("1~3문장, 한국어, 3인칭 서술. 캐릭터 성격을 반영하되 수치를 바꾸지 말 것.");
            return BuildChatPrompt(userContext.ToString());
        }

        public static string BuildFloorClearLogPrompt(ExplorationEvent explorationEvent, PartyState party)
        {
            var userContext = new StringBuilder();
            AppendWorldAndCharacterContext(userContext, explorationEvent, party);
            userContext.AppendLine("[이벤트 컨텍스트]");
            userContext.Append("{ \"event_type\": \"floor_clear\", \"floor\": ");
            userContext.Append(explorationEvent.Floor);
            userContext.AppendLine(" }");
            userContext.AppendLine();
            userContext.AppendLine("[출력 형식 지시]");
            userContext.Append("2~3문장, 한국어, 층 돌파의 성취감을 담아 서술.");
            return BuildChatPrompt(userContext.ToString());
        }

        public static string BuildGenericLogPrompt(ExplorationEvent explorationEvent, PartyState party)
        {
            var userContext = new StringBuilder();
            AppendWorldAndCharacterContext(userContext, explorationEvent, party);
            userContext.AppendLine("[이벤트 컨텍스트]");
            userContext.Append("{ \"event_type\": \"");
            userContext.Append(explorationEvent.EventType.ToString().ToLowerInvariant());
            userContext.AppendLine("\" }");
            userContext.AppendLine();
            userContext.AppendLine("[출력 형식 지시]");
            userContext.Append("2~4문장, 한국어, 3인칭 서술.");
            return BuildChatPrompt(userContext.ToString());
        }

        private static void AppendWorldAndCharacterContext(StringBuilder userContext, ExplorationEvent explorationEvent, PartyState party)
        {
            var leader = party?.Leader;
            userContext.AppendLine("[월드 컨텍스트]");
            userContext.Append("구역: ");
            userContext.Append(ZoneDefinitions.GetZoneDisplayName(explorationEvent.ZoneId));
            userContext.Append(' ');
            userContext.Append(explorationEvent.Floor);
            userContext.AppendLine("층 (습하고 어둑함, 감각적 묘사 위주)");
            userContext.AppendLine();
            userContext.AppendLine("[캐릭터 컨텍스트]");
            if (leader != null)
            {
                userContext.Append("이름: ");
                userContext.Append(leader.DisplayName);
                userContext.Append(" / 역할: ");
                userContext.Append(FormatRole(leader.Role));
                userContext.Append(" / 성격: ");
                userContext.AppendLine(FormatPersonalities(leader.PersonalityTags));

                var memoryContext = CharacterMemoryManager.BuildPromptContext(leader.CharacterId);
                if (!string.IsNullOrEmpty(memoryContext))
                {
                    userContext.AppendLine();
                    userContext.Append(memoryContext);
                }

                var relationshipContext = RelationshipManager.BuildPartyPromptContext(party);
                if (!string.IsNullOrEmpty(relationshipContext))
                {
                    userContext.AppendLine();
                    userContext.Append(relationshipContext);
                }

                var rollingSummary = ExplorationRollingSummary.BuildPromptContext();
                if (!string.IsNullOrEmpty(rollingSummary))
                {
                    userContext.AppendLine();
                    userContext.Append(rollingSummary);
                }
            }
            else
            {
                userContext.AppendLine("이름: 탐험대 / 역할: 미상 / 성격: 미상");
            }

            userContext.AppendLine();
        }

        private static string BuildChatPrompt(string userMessage)
        {
            return
                "<|im_start|>system\n" + SystemPrompt + "\n" + ImEnd + "\n" +
                "<|im_start|>user\n" + userMessage + "\n" + ImEnd + "\n" +
                "<|im_start|>assistant\n";
        }

        private static string BuildCombatContextJson(ExplorationEvent explorationEvent)
        {
            var combat = explorationEvent.Combat;
            if (combat == null)
                return "{ \"event_type\": \"combat_result\", \"note\": \"details unavailable\" }";

            var builder = new StringBuilder();
            builder.Append("{ ");
            builder.Append("\"event_type\": \"combat_result\", ");
            builder.Append("\"monster\": \"");
            builder.Append(combat.MonsterDisplayName ?? "적");
            builder.Append("\", ");
            builder.Append("\"result\": \"");
            builder.Append(FormatOutcome(combat.Outcome));
            builder.Append("\", ");
            builder.Append("\"gold_gained\": ");
            builder.Append(combat.GoldGained);
            builder.Append(", ");
            builder.Append("\"damage_taken\": ");
            builder.Append(combat.DamageTaken);
            builder.Append(", ");
            builder.Append("\"injuries\": \"");
            builder.Append(FormatInjuries(combat));
            builder.Append("\", ");
            builder.Append("\"loot\": \"");
            builder.Append(FormatLoot(combat));
            builder.Append("\" }");
            return builder.ToString();
        }

        private static string FormatRole(CharacterRole role)
        {
            return role switch
            {
                CharacterRole.Warrior => "전사",
                CharacterRole.Rogue => "도적",
                CharacterRole.Mage => "마법사",
                CharacterRole.Bard => "음유시인",
                _ => role.ToString()
            };
        }

        private static string FormatPersonalities(IReadOnlyList<PersonalityTag> tags)
        {
            if (tags == null || tags.Count == 0)
                return "보통";

            var parts = new List<string>(tags.Count);
            foreach (var tag in tags)
            {
                parts.Add(tag switch
                {
                    PersonalityTag.Cautious => "신중함",
                    PersonalityTag.Greedy => "탐욕적",
                    PersonalityTag.Reckless => "무모함",
                    PersonalityTag.Cheerful => "쾌활함",
                    PersonalityTag.Loyal => "충직함",
                    PersonalityTag.Cynical => "냉소적",
                    _ => tag.ToString()
                });
            }

            return string.Join(", ", parts);
        }

        private static string FormatOutcome(CombatOutcome outcome)
        {
            return outcome switch
            {
                CombatOutcome.Victory => "victory",
                CombatOutcome.Defeat => "defeat",
                CombatOutcome.Retreat => "retreat",
                _ => outcome.ToString().ToLowerInvariant()
            };
        }

        private static string FormatInjuries(CombatResultPayload combat)
        {
            if (combat.Injuries == null || combat.Injuries.Count == 0)
                return "none";

            var parts = new List<string>(combat.Injuries.Count);
            foreach (var injury in combat.Injuries)
            {
                parts.Add(injury.Severity switch
                {
                    InjurySeverity.Light => "light_injury",
                    InjurySeverity.Moderate => "moderate_injury",
                    InjurySeverity.Severe => "severe_injury",
                    InjurySeverity.Fatal => "fatal_injury",
                    _ => "none"
                });
            }

            return string.Join(", ", parts);
        }

        private static string FormatLoot(CombatResultPayload combat)
        {
            if (combat.Loot == null || combat.Loot.Count == 0)
                return "none";

            var parts = new List<string>(combat.Loot.Count);
            foreach (var loot in combat.Loot)
            {
                parts.Add($"{loot.ItemId} x{loot.Quantity}");
            }

            return string.Join(", ", parts);
        }
    }
}
