using System;
using System.Collections.Generic;
using Backend.Chronicle;
using Backend.Simulation;

namespace Backend.GameSystems.Offline
{
    /// <summary>
    /// 오프라인 복귀 요약용 연대기 하이라이트 3~5줄을 생성한다.
    /// </summary>
    public static class OfflineHighlightGenerator
    {
        private static readonly string[] HighlightEventTypes =
        {
            ChronicleEventTypes.Move,
            ChronicleEventTypes.CombatResult,
            ChronicleEventTypes.Discovery,
            ChronicleEventTypes.Rest,
            ChronicleEventTypes.FloorClear,
        };

        /// <summary>
        /// 정산 구간·층 기준 하이라이트 문장 목록을 생성한다.
        /// </summary>
        public static IReadOnlyList<string> Generate(
            int currentFloor,
            TimeSpan settledDuration,
            int randomSeed,
            Func<NarrationRequest, string> narrationBuilder = null)
        {
            narrationBuilder ??= NarrationProvider.BuildLine;

            var count = ResolveHighlightCount(settledDuration);
            if (count <= 0)
                return Array.Empty<string>();

            var zone = BalanceFormulas.GetZoneFromFloor(BalanceTableProvider.Get(), currentFloor);
            var zoneToneTag = $"zone_{zone}";
            var lines = new List<string>(count);

            for (var i = 0; i < count; i++)
            {
                var eventType = HighlightEventTypes[i % HighlightEventTypes.Length];
                var request = new NarrationRequest
                {
                    EventId = $"offline_{currentFloor}_{i}",
                    EventType = eventType,
                    Salience = SalienceGrade.Notable,
                    TimestampTick = (int)Math.Min(settledDuration.TotalSeconds, int.MaxValue),
                    RandomSeed = randomSeed + i * 997,
                    ZoneToneTag = zoneToneTag,
                    Slots = new Dictionary<string, string>
                    {
                        ["floor"] = currentFloor.ToString(),
                        ["amount"] = settledDuration.TotalHours.ToString("0.#"),
                    },
                };

                var line = narrationBuilder(request);
                if (!string.IsNullOrWhiteSpace(line))
                    lines.Add(line.Trim());
            }

            return lines;
        }

        private static int ResolveHighlightCount(TimeSpan settledDuration)
        {
            if (settledDuration.TotalMinutes < 1d)
                return 0;

            var hours = settledDuration.TotalHours;
            var count = OfflinePolicy.MinHighlightCount;
            if (hours >= 2d)
                count++;
            if (hours >= 4d)
                count++;

            return Math.Min(count, OfflinePolicy.MaxHighlightCount);
        }
    }
}
