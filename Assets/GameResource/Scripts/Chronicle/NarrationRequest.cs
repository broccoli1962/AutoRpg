using System.Collections.Generic;

namespace Backend.Chronicle
{
    /// <summary>
    /// 내레이션 한 줄 생성에 필요한 컨텍스트.
    /// </summary>
    public sealed class NarrationRequest
    {
        /// <summary>
        /// 이벤트 고유 식별자 (결정론적 시드 보조).
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        /// 이벤트 타입 식별자 (예: combat_result, discovery).
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// Salience 등급. Trivial은 묶음 집계 대상이다.
        /// </summary>
        public SalienceGrade Salience { get; set; }

        /// <summary>
        /// 시뮬레이션 tick 타임스탬프.
        /// </summary>
        public int TimestampTick { get; set; }

        /// <summary>
        /// 연대기 생성용 결정론적 시드. 0이면 EventId/Timestamp로 파생한다.
        /// </summary>
        public int RandomSeed { get; set; }

        /// <summary>
        /// 캐릭터 성격 태그 (cautious, reckless 등).
        /// </summary>
        public IReadOnlyList<string> CharacterPersonalityTags { get; set; }

        /// <summary>
        /// 구역 톤 태그 (예: zone_forest).
        /// </summary>
        public string ZoneToneTag { get; set; }

        /// <summary>
        /// Passthrough 단계에서 그대로 반환할 템플릿 문장.
        /// </summary>
        public string TemplateLine { get; set; }

        /// <summary>
        /// 생성 실패 시 사용할 명시적 폴백. 비어 있으면 구현체 기본값을 사용한다.
        /// </summary>
        public string FallbackLine { get; set; }

        /// <summary>
        /// 슬롯 치환용 변수 (예: character, monster, amount, floor).
        /// </summary>
        public IReadOnlyDictionary<string, string> Slots { get; set; }
    }
}
