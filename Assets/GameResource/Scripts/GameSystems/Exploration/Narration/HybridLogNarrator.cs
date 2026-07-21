using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.LLM;
using Backend.Util;
using UnityEngine;

namespace Backend.GameSystems.Exploration.Narration
{
    /// <summary>
    /// 템플릿 내레이터 + LLM 내레이터 하이브리드.
    /// Phase 2: Significant 이상 전투(CombatResult)만 LLM 큐에 위임하고, 나머지는 즉시 템플릿 처리.
    /// </summary>
    public sealed class HybridLogNarrator : ILogNarrator
    {
        private readonly TemplateLogNarrator _template = new();

        public LogEntry Narrate(ExplorationEvent explorationEvent, PartyState party)
        {
            var templateEntry = _template.Narrate(explorationEvent, party);

            if (!LlmNarrationManager.ShouldUseLlm(explorationEvent))
                return templateEntry;

            if (!Application.isPlaying || GameStateUtil.IsQuitting)
                return templateEntry;

            var pending = CloneEntry(templateEntry);
            pending.Text = "…";
            pending.IsPending = true;
            pending.UsedLlm = false;

            LlmNarrationManager.EnqueueJob(new LlmNarrationJob
            {
                Event = explorationEvent,
                Party = party,
                PendingEntry = pending,
                FallbackEntry = templateEntry
            });

            return pending;
        }

        public LogEntry NarrateOfflineSummary(OfflineSummaryContext context)
        {
            return _template.NarrateOfflineSummary(context);
        }

        private static LogEntry CloneEntry(LogEntry source)
        {
            return new LogEntry
            {
                EventId = source.EventId,
                EventType = source.EventType,
                Salience = source.Salience,
                Category = source.Category,
                Text = source.Text,
                TimestampTick = source.TimestampTick,
                Floor = source.Floor,
                IsBookmarked = source.IsBookmarked,
                IsPending = source.IsPending,
                UsedLlm = source.UsedLlm
            };
        }
    }
}
