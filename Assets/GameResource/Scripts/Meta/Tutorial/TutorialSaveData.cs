using System;

namespace Backend.Meta.Tutorial
{
    /// <summary>
    /// 튜토리얼 진행 상태 세이브 스냅샷.
    /// </summary>
    [Serializable]
    public sealed class TutorialSaveData
    {
        public int CurrentStep;
        public int[] CompletedStepIds = Array.Empty<int>();
        public bool InitialGrantsApplied;
        public int[] GuidanceSkippedStepIds = Array.Empty<int>();
    }
}
