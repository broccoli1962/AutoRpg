namespace Backend.Chronicle
{
    /// <summary>
    /// 런타임에 주입된 <see cref="INarrationSource"/>에 대한 정적 접근점.
    /// </summary>
    public static class NarrationProvider
    {
        private static INarrationSource _source;

        /// <summary>
        /// 현재 등록된 내레이션 소스. 미등록 시 <see cref="PassthroughNarrationSource"/> 폴백을 사용한다.
        /// </summary>
        public static INarrationSource Source => _source ??= new PassthroughNarrationSource();

        /// <summary>
        /// 부트스트랩 시 내레이션 소스를 주입한다.
        /// </summary>
        public static void SetSource(INarrationSource source)
        {
            _source = source;
        }

        /// <summary>
        /// 등록된 소스로 내레이션 한 줄을 생성한다.
        /// </summary>
        public static string BuildLine(NarrationRequest request) => Source.BuildLine(request);
    }
}
