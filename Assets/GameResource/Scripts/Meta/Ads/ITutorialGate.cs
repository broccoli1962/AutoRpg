namespace Backend.Meta.Ads
{
    /// <summary>
    /// 튜토리얼 진행 중 광고 차단 여부를 제공한다.
    /// </summary>
    public interface ITutorialGate
    {
        /// <summary>
        /// 튜토리얼 진행 중이면 true. 이 경우 광고를 노출하지 않는다.
        /// </summary>
        bool IsTutorialActive { get; }
    }
}
