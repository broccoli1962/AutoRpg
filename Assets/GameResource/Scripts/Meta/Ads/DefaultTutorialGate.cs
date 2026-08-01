namespace Backend.Meta.Ads
{
    /// <summary>
    /// 튜토리얼 미구현 시 광고를 허용하는 기본 게이트.
    /// </summary>
    public sealed class DefaultTutorialGate : ITutorialGate
    {
        /// <summary>
        /// 튜토리얼 진행 중이면 true.
        /// </summary>
        public bool IsTutorialActive { get; set; }
    }
}
