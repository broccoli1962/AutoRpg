namespace Backend.Meta.Characters
{
    /// <summary>
    /// 한계돌파 시도 결과.
    /// </summary>
    public readonly struct LimitBreakResult
    {
        public bool Success { get; }
        public string CharacterId { get; }
        public int PreviousStage { get; }
        public int NewStage { get; }
        public int FragmentsSpent { get; }
        public string FailureReason { get; }

        private LimitBreakResult(
            bool success,
            string characterId,
            int previousStage,
            int newStage,
            int fragmentsSpent,
            string failureReason)
        {
            Success = success;
            CharacterId = characterId;
            PreviousStage = previousStage;
            NewStage = newStage;
            FragmentsSpent = fragmentsSpent;
            FailureReason = failureReason;
        }

        /// <summary>
        /// 한계돌파 성공 결과를 생성한다.
        /// </summary>
        public static LimitBreakResult Succeeded(
            string characterId,
            int previousStage,
            int newStage,
            int fragmentsSpent)
        {
            return new LimitBreakResult(
                true,
                characterId,
                previousStage,
                newStage,
                fragmentsSpent,
                null);
        }

        /// <summary>
        /// 한계돌파 실패 결과를 생성한다.
        /// </summary>
        public static LimitBreakResult Failed(
            string characterId,
            int currentStage,
            string failureReason)
        {
            return new LimitBreakResult(
                false,
                characterId,
                currentStage,
                currentStage,
                0,
                failureReason);
        }
    }
}
