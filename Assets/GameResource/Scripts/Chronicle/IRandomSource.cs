namespace Backend.Chronicle
{
    /// <summary>
    /// 연대기 엔진용 결정론적 난수 소스.
    /// </summary>
    public interface IRandomSource
    {
        /// <summary>
        /// [minInclusive, maxExclusive) 범위의 정수를 반환한다.
        /// </summary>
        int NextInt(int minInclusive, int maxExclusive);

        /// <summary>
        /// [0.0, 1.0) 범위의 실수를 반환한다.
        /// </summary>
        double NextDouble();
    }
}
