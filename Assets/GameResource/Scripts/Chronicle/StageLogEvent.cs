using System.Collections.Generic;

namespace Backend.Chronicle
{
    /// <summary>
    /// 로그 스트립에 기록할 스테이지 사건 컨텍스트.
    /// </summary>
    public sealed class StageLogEvent
    {
        public string EventId { get; set; }
        public string EventType { get; set; }
        public SalienceGrade Salience { get; set; }
        public int TimestampTick { get; set; }
        public int Floor { get; set; }
        public string MonsterName { get; set; }
        public int KillCount { get; set; } = 1;
        public IReadOnlyList<string> CharacterPersonalityTags { get; set; }
        public string ZoneToneTag { get; set; }
        public IReadOnlyDictionary<string, string> Slots { get; set; }
        public int RandomSeed { get; set; }
    }
}
