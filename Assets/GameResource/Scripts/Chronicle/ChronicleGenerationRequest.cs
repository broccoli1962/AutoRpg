using System;
using System.Collections.Generic;

namespace Backend.Chronicle
{
    /// <summary>
    /// <see cref="ChronicleEngine"/> 문장 생성에 필요한 컨텍스트.
    /// </summary>
    public sealed class ChronicleGenerationRequest
    {
        /// <summary>
        /// 이벤트 타입 식별자 (예: combat_result, discovery).
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// 캐릭터 성격 태그 (예: brave, cautious).
        /// </summary>
        public IReadOnlyList<string> CharacterPersonalityTags { get; set; }

        /// <summary>
        /// 구역 톤 태그 (예: zone_forest, zone_abyss).
        /// </summary>
        public string ZoneToneTag { get; set; }

        /// <summary>
        /// {character}, {monster}, {amount} 등 변수 치환값.
        /// </summary>
        public IReadOnlyDictionary<string, string> Variables { get; set; }

        /// <summary>
        /// 현지화 키를 표시 문자열로 변환한다. null이면 키 문자열을 그대로 사용한다.
        /// </summary>
        public Func<string, string> ResolveText { get; set; }
    }
}
