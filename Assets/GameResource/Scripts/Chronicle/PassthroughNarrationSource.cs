namespace Backend.Chronicle
{
    /// <summary>
    /// 템플릿 문장을 그대로 반환하는 임시 내레이션 소스.
    /// Step 3에서 ChronicleNarrationSource로 교체된다.
    /// </summary>
    public sealed class PassthroughNarrationSource : INarrationSource
    {
        public const string DefaultFallbackLine = "탐험대는 조용히 전진한다.";

        /// <summary>
        /// 템플릿이 있으면 그대로 반환하고, 없으면 폴백 경로를 사용한다.
        /// </summary>
        public string BuildLine(NarrationRequest request)
        {
            if (request == null)
                return DefaultFallbackLine;

            if (!string.IsNullOrWhiteSpace(request.TemplateLine))
                return request.TemplateLine.Trim();

            if (!string.IsNullOrWhiteSpace(request.FallbackLine))
                return request.FallbackLine.Trim();

            return DefaultFallbackLine;
        }
    }
}
