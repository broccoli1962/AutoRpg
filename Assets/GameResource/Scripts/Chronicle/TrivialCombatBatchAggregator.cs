namespace Backend.Chronicle
{
    /// <summary>
    /// Trivial 전투 처치를 동일 몬스터 단위로 묶는다.
    /// </summary>
    internal sealed class TrivialCombatBatchAggregator
    {
        private string _monsterName;
        private int _count;
        private StageLogEvent _lastEvent;

        /// <summary>
        /// Trivial 전투 이벤트를 누적하거나 새 배치를 시작한다.
        /// </summary>
        public bool TryAccumulate(StageLogEvent stageEvent)
        {
            if (stageEvent == null ||
                stageEvent.Salience != SalienceGrade.Trivial ||
                stageEvent.EventType != ChronicleEventTypes.CombatResult)
            {
                return false;
            }

            var monsterName = stageEvent.MonsterName;
            if (string.IsNullOrWhiteSpace(monsterName))
                return false;

            if (_count > 0 && _monsterName == monsterName)
            {
                _count += stageEvent.KillCount > 0 ? stageEvent.KillCount : 1;
                _lastEvent = stageEvent;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 새 Trivial 전투 배치를 시작한다.
        /// </summary>
        public void Begin(StageLogEvent stageEvent)
        {
            _monsterName = stageEvent.MonsterName;
            _count = stageEvent.KillCount > 0 ? stageEvent.KillCount : 1;
            _lastEvent = stageEvent;
        }

        /// <summary>
        /// 누적 중인 배치가 있으면 true.
        /// </summary>
        public bool HasPending => _count > 0;

        /// <summary>
        /// 누적 배치를 묶음 집계 문장용 이벤트로 변환하고 초기화한다.
        /// </summary>
        public StageLogEvent Flush()
        {
            if (_count <= 0)
                return null;

            var flushed = new StageLogEvent
            {
                EventId = _lastEvent?.EventId,
                EventType = ChronicleEventTypes.CombatResult,
                Salience = SalienceGrade.Trivial,
                TimestampTick = _lastEvent?.TimestampTick ?? 0,
                Floor = _lastEvent?.Floor ?? 0,
                MonsterName = _monsterName,
                KillCount = _count,
                CharacterPersonalityTags = _lastEvent?.CharacterPersonalityTags,
                ZoneToneTag = _lastEvent?.ZoneToneTag,
                RandomSeed = _lastEvent?.RandomSeed ?? 0
            };

            _monsterName = null;
            _count = 0;
            _lastEvent = null;
            return flushed;
        }

        /// <summary>
        /// 진행 중인 배치를 버린다.
        /// </summary>
        public void Clear()
        {
            _monsterName = null;
            _count = 0;
            _lastEvent = null;
        }
    }
}
