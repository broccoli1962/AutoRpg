using System.Collections.Generic;

namespace Backend.Chronicle
{
    /// <summary>
    /// Salience 등급에 따라 연대기/묶음 집계 내레이션을 라우팅한다. LLM 분기는 사용하지 않는다.
    /// </summary>
    public static class SalienceRouter
    {
        private static readonly TrivialCombatBatchAggregator _trivialBatch = new();

        /// <summary>
        /// Salience 등급에 따라 즉시 출력할 문장을 생성하거나 Trivial 전투를 누적한다.
        /// Trivial 전투는 <see cref="FlushPendingBatch"/> 호출 전까지 null을 반환할 수 있다.
        /// </summary>
        public static string Route(StageLogEvent stageEvent)
        {
            if (stageEvent == null)
                return null;

            if (ShouldBatchTrivialCombat(stageEvent))
            {
                if (_trivialBatch.TryAccumulate(stageEvent))
                    return null;

                var flushed = _trivialBatch.Flush();
                var previousLine = flushed != null ? BuildLine(flushed) : null;
                _trivialBatch.Begin(stageEvent);
                return previousLine;
            }

            var pending = _trivialBatch.Flush();
            if (pending != null && stageEvent.Salience == SalienceGrade.Trivial)
            {
                var batchLine = BuildLine(pending);
                var currentLine = BuildLine(stageEvent);
                return string.IsNullOrEmpty(currentLine) ? batchLine : $"{batchLine}\n{currentLine}";
            }

            if (pending != null)
            {
                var batchLine = BuildLine(pending);
                var routedLine = BuildLine(stageEvent);
                return string.IsNullOrEmpty(routedLine) ? batchLine : $"{batchLine}\n{routedLine}";
            }

            return BuildLine(stageEvent);
        }

        /// <summary>
        /// 누적 중인 Trivial 전투 묶음을 플러시한다.
        /// </summary>
        public static string FlushPendingBatch()
        {
            var pending = _trivialBatch.Flush();
            return pending == null ? null : BuildLine(pending);
        }

        /// <summary>
        /// 라우터 내부 누적 상태를 초기화한다.
        /// </summary>
        public static void Reset()
        {
            _trivialBatch.Clear();
        }

        private static bool ShouldBatchTrivialCombat(StageLogEvent stageEvent)
        {
            return stageEvent.Salience == SalienceGrade.Trivial &&
                   stageEvent.EventType == ChronicleEventTypes.CombatResult &&
                   !string.IsNullOrWhiteSpace(stageEvent.MonsterName);
        }

        private static string BuildLine(StageLogEvent stageEvent)
        {
            if (stageEvent.Salience == SalienceGrade.Trivial &&
                stageEvent.EventType == ChronicleEventTypes.CombatResult &&
                stageEvent.KillCount > 1)
            {
                return FormatTrivialBatch(stageEvent.MonsterName, stageEvent.KillCount);
            }

            var slots = BuildSlots(stageEvent);
            var request = new NarrationRequest
            {
                EventId = stageEvent.EventId,
                EventType = stageEvent.EventType,
                Salience = stageEvent.Salience,
                TimestampTick = stageEvent.TimestampTick,
                RandomSeed = stageEvent.RandomSeed,
                CharacterPersonalityTags = stageEvent.CharacterPersonalityTags,
                ZoneToneTag = stageEvent.ZoneToneTag,
                Slots = slots
            };

            return NarrationProvider.BuildLine(request);
        }

        private static Dictionary<string, string> BuildSlots(StageLogEvent stageEvent)
        {
            var slots = new Dictionary<string, string>();
            if (stageEvent.Slots != null)
            {
                foreach (var pair in stageEvent.Slots)
                    slots[pair.Key] = pair.Value;
            }

            if (!string.IsNullOrWhiteSpace(stageEvent.MonsterName) && !slots.ContainsKey("monster"))
                slots["monster"] = stageEvent.MonsterName;

            if (stageEvent.Floor > 0 && !slots.ContainsKey("floor"))
                slots["floor"] = stageEvent.Floor.ToString();

            return slots;
        }

        private static string FormatTrivialBatch(string monsterName, int count)
        {
            return $"{monsterName} ×{count}";
        }
    }
}
