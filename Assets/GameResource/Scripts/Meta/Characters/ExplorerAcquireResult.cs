namespace Backend.Meta.Characters
{
    /// <summary>
    /// 탐험가 획득 처리 결과.
    /// </summary>
    public readonly struct ExplorerAcquireResult
    {
        public bool Success { get; }
        public string CharacterId { get; }
        public ExplorerGrade Grade { get; }
        public bool IsNewCharacter { get; }
        public int FragmentsGranted { get; }
        public string FailureReason { get; }

        private ExplorerAcquireResult(
            bool success,
            string characterId,
            ExplorerGrade grade,
            bool isNewCharacter,
            int fragmentsGranted,
            string failureReason)
        {
            Success = success;
            CharacterId = characterId;
            Grade = grade;
            IsNewCharacter = isNewCharacter;
            FragmentsGranted = fragmentsGranted;
            FailureReason = failureReason;
        }

        /// <summary>
        /// 신규 탐험가 지급 결과를 생성한다.
        /// </summary>
        public static ExplorerAcquireResult NewCharacter(
            string characterId,
            ExplorerGrade grade)
        {
            return new ExplorerAcquireResult(
                true,
                characterId,
                grade,
                true,
                0,
                null);
        }

        /// <summary>
        /// 중복 획득 조각 전환 결과를 생성한다.
        /// </summary>
        public static ExplorerAcquireResult DuplicateFragments(
            string characterId,
            ExplorerGrade grade,
            int fragmentsGranted)
        {
            return new ExplorerAcquireResult(
                true,
                characterId,
                grade,
                false,
                fragmentsGranted,
                null);
        }

        /// <summary>
        /// 실패 결과를 생성한다.
        /// </summary>
        public static ExplorerAcquireResult Failed(string characterId, string failureReason)
        {
            return new ExplorerAcquireResult(
                false,
                characterId,
                default,
                false,
                0,
                failureReason);
        }
    }
}
