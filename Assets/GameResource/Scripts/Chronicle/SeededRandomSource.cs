using System;

namespace Backend.Chronicle
{
    /// <summary>
    /// 고정 시드 기반 <see cref="IRandomSource"/> 구현.
    /// </summary>
    public sealed class SeededRandomSource : IRandomSource
    {
        private readonly Random _random;

        /// <summary>
        /// 지정 시드로 난수 소스를 생성한다.
        /// </summary>
        public SeededRandomSource(int seed)
        {
            _random = new Random(seed);
        }

        /// <summary>
        /// [minInclusive, maxExclusive) 범위의 정수를 반환한다.
        /// </summary>
        public int NextInt(int minInclusive, int maxExclusive) =>
            _random.Next(minInclusive, maxExclusive);

        /// <summary>
        /// [0.0, 1.0) 범위의 실수를 반환한다.
        /// </summary>
        public double NextDouble() => _random.NextDouble();
    }
}
