namespace Backend.GameSystems.Exploration.Simulation
{
    /// <summary>
    /// 시드 기반 결정론적 난수 생성기. 시뮬레이션 재현 및 오프라인 fast-forward에 사용한다.
    /// </summary>
    public sealed class DeterministicRandom
    {
        private uint _state;

        public DeterministicRandom(int seed)
        {
            _state = seed == 0 ? 1u : (uint)seed;
        }

        /// <summary>
        /// 내부 RNG 상태를 직렬화용으로 내보낸다.
        /// </summary>
        public uint ExportState() => _state;

        /// <summary>
        /// 직렬화된 RNG 상태로 생성기를 복원한다.
        /// </summary>
        public void ImportState(uint state)
        {
            _state = state == 0 ? 1u : state;
        }

        public int NextInt(int maxExclusive)
        {
            if (maxExclusive <= 0)
                return 0;

            return (int)(NextUInt() % (uint)maxExclusive);
        }

        public float NextFloat()
        {
            return NextUInt() / (float)uint.MaxValue;
        }

        public float NextRange(float minInclusive, float maxInclusive)
        {
            return minInclusive + (maxInclusive - minInclusive) * NextFloat();
        }

        public bool RollChance(float chance)
        {
            return NextFloat() < chance;
        }

        private uint NextUInt()
        {
            _state ^= _state << 13;
            _state ^= _state >> 17;
            _state ^= _state << 5;
            return _state;
        }
    }
}
