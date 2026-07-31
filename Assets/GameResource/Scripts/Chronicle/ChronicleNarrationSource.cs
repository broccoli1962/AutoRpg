using System;
using System.Collections.Generic;

namespace Backend.Chronicle
{
    /// <summary>
    /// <see cref="ChronicleEngine"/>으로 문장 뱅크 조합 내레이션을 생성한다.
    /// </summary>
    public sealed class ChronicleNarrationSource : INarrationSource
    {
        private readonly ChronicleEngine _engine;
        private readonly PassthroughNarrationSource _fallback = new();

        /// <summary>
        /// 문장 뱅크를 사용하는 연대기 내레이션 소스를 생성한다.
        /// </summary>
        public ChronicleNarrationSource(PhraseBank bank)
        {
            _engine = new ChronicleEngine(bank);
        }

        /// <summary>
        /// 요청 컨텍스트에 맞는 연대기 문장을 생성한다.
        /// </summary>
        public string BuildLine(NarrationRequest request)
        {
            if (request == null)
                return PassthroughNarrationSource.DefaultFallbackLine;

            if (!string.IsNullOrWhiteSpace(request.TemplateLine))
                return request.TemplateLine.Trim();

            var eventType = request.EventType;
            if (string.IsNullOrWhiteSpace(eventType))
                return ResolveFallback(request);

            var seed = request.RandomSeed != 0 ? request.RandomSeed : StableSeed(request);
            var generationRequest = new ChronicleGenerationRequest
            {
                EventType = eventType,
                CharacterPersonalityTags = request.CharacterPersonalityTags,
                ZoneToneTag = request.ZoneToneTag,
                Variables = request.Slots,
                ResolveText = key => key
            };

            var line = _engine.Generate(generationRequest, new SeededRandomSource(seed));
            return string.IsNullOrWhiteSpace(line) ? ResolveFallback(request) : line;
        }

        private static int StableSeed(NarrationRequest request)
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 31 + (request.EventType?.GetHashCode(StringComparison.Ordinal) ?? 0);
                hash = hash * 31 + (request.EventId?.GetHashCode(StringComparison.Ordinal) ?? 0);
                hash = hash * 31 + request.TimestampTick;
                return hash;
            }
        }

        private string ResolveFallback(NarrationRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.FallbackLine))
                return request.FallbackLine.Trim();

            return _fallback.BuildLine(request);
        }
    }
}
