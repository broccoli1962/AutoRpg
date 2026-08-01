using Backend.Meta.Ads;

namespace Backend.Meta.Tutorial
{
    /// <summary>
    /// TutorialService 기반 광고·상점 차단 게이트.
    /// </summary>
    public sealed class TutorialServiceGate : ITutorialGate
    {
        private readonly TutorialService _service;

        public TutorialServiceGate(TutorialService service)
        {
            _service = service;
        }

        /// <summary>
        /// 튜토리얼 진행 중이면 true.
        /// </summary>
        public bool IsTutorialActive => _service != null && _service.IsTutorialActive;
    }
}
