namespace Backend.Chronicle
{
    /// <summary>
    /// 스테이지 로그·연대기 문장을 생성하는 추상 소스.
    /// </summary>
    public interface INarrationSource
    {
        /// <summary>
        /// 요청 컨텍스트에 맞는 내레이션 한 줄을 동기적으로 생성한다.
        /// </summary>
        string BuildLine(NarrationRequest request);
    }
}
