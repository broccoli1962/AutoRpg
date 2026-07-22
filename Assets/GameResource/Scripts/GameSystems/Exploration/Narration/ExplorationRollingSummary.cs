using System.Collections.Generic;
using System.Text;
using Backend.GameSystems.Character;
using Backend.GameSystems.Exploration.Data;

namespace Backend.GameSystems.Exploration.Narration
{
    /// <summary>
    /// 최근 탐험 사건 압축 요약을 유지해 LLM 로그 프롬프트 문맥을 연결한다 (05_자동탐험로그.md).
    /// </summary>
    public static class ExplorationRollingSummary
    {
        private const int Capacity = 5;

        private static readonly Queue<string> Entries = new();

        public static void Clear()
        {
            Entries.Clear();
        }

        public static void Record(ExplorationEvent explorationEvent, PartyState party)
        {
            if (explorationEvent == null || explorationEvent.Salience < SalienceGrade.Notable)
                return;

            var summary = CharacterMemoryRecorder.SummarizeExplorationEvent(explorationEvent, party);
            if (string.IsNullOrWhiteSpace(summary))
                return;

            Entries.Enqueue(summary);
            while (Entries.Count > Capacity)
                Entries.Dequeue();
        }

        public static string BuildPromptContext()
        {
            if (Entries.Count == 0)
                return null;

            var builder = new StringBuilder();
            builder.AppendLine("최근 사건 요약:");
            foreach (var entry in Entries)
            {
                builder.Append("- ");
                builder.AppendLine(entry);
            }

            return builder.ToString().TrimEnd();
        }
    }
}
