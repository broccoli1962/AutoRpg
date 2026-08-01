using System;

namespace Backend.Meta.Quests
{
    /// <summary>
    /// 퀘스트 보상 수령 결과.
    /// </summary>
    public sealed class QuestClaimResult
    {
        private QuestClaimResult(
            bool success,
            string questId,
            QuestPeriod period,
            bool isCompletionChest,
            string failureReason)
        {
            Success = success;
            QuestId = questId;
            Period = period;
            IsCompletionChest = isCompletionChest;
            FailureReason = failureReason;
        }

        public bool Success { get; }
        public string QuestId { get; }
        public QuestPeriod Period { get; }
        public bool IsCompletionChest { get; }
        public string FailureReason { get; }

        /// <summary>
        /// 성공 결과를 생성한다.
        /// </summary>
        public static QuestClaimResult Succeeded(
            string questId,
            QuestPeriod period,
            bool isCompletionChest = false)
        {
            return new QuestClaimResult(true, questId, period, isCompletionChest, null);
        }

        /// <summary>
        /// 실패 결과를 생성한다.
        /// </summary>
        public static QuestClaimResult Failed(
            string questId,
            QuestPeriod period,
            string reason,
            bool isCompletionChest = false)
        {
            return new QuestClaimResult(false, questId, period, isCompletionChest, reason);
        }
    }
}
