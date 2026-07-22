using System.Text;
using Backend.GameSystems.DynamicEvent.Data;
using LLama.Sampling;

namespace Backend.GameSystems.DynamicEvent.LLM
{
    /// <summary>
    /// 동적 이벤트 장면 JSON 출력을 GBNF로 제한한다. (Phase 4)
    /// </summary>
    public static class DynamicEventSceneGrammarBuilder
    {
        public static Grammar Build(DynamicEventTemplate template)
        {
            if (template?.Choices == null || template.Choices.Count == 0)
                return null;

            var gbnf = new StringBuilder();
            gbnf.AppendLine("root ::= object");
            gbnf.AppendLine(
                "object ::= \"{\" space \"\\\"narration\\\"\" space \":\" space string space \",\" space " +
                "\"\\\"choices\\\"\" space \":\" space choices \"}\" space");
            gbnf.Append("choices ::= \"[\" space ");

            for (var i = 0; i < template.Choices.Count; i++)
            {
                if (i > 0)
                    gbnf.Append(" space \",\" space ");

                gbnf.Append("choice");
                gbnf.Append(i);
            }

            gbnf.AppendLine(" space \"]\" space");

            for (var i = 0; i < template.Choices.Count; i++)
            {
                var choiceId = EscapeLiteral(template.Choices[i].Id);
                gbnf.Append("choice");
                gbnf.Append(i);
                gbnf.Append(" ::= \"{\" space \"\\\"id\\\"\" space \":\" space \"\\\"");
                gbnf.Append(choiceId);
                gbnf.AppendLine(
                    "\\\"\" space \",\" space \"\\\"text\\\"\" space \":\" space string space \"}\" space");
            }

            gbnf.AppendLine("string ::= \"\\\"\" char* \"\\\"\" space");
            gbnf.AppendLine("char ::= [^\"\\\\]");
            gbnf.AppendLine("space ::= [ \\t\\n]*");

            return new Grammar(gbnf.ToString(), "root");
        }

        private static string EscapeLiteral(string value) =>
            string.IsNullOrEmpty(value) ? string.Empty : value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
