using System;

namespace Backend.Meta.Mailbox
{
    /// <summary>
    /// 우편 수령 결과.
    /// </summary>
    public sealed class MailClaimResult
    {
        private MailClaimResult(bool success, string mailId, string failureReason)
        {
            Success = success;
            MailId = mailId;
            FailureReason = failureReason;
        }

        public bool Success { get; }
        public string MailId { get; }
        public string FailureReason { get; }

        /// <summary>
        /// 성공 결과를 생성한다.
        /// </summary>
        public static MailClaimResult Succeeded(string mailId)
        {
            return new MailClaimResult(true, mailId, null);
        }

        /// <summary>
        /// 실패 결과를 생성한다.
        /// </summary>
        public static MailClaimResult Failed(string mailId, string reason)
        {
            return new MailClaimResult(false, mailId, reason);
        }
    }

    /// <summary>
    /// 일괄 수령 결과.
    /// </summary>
    public sealed class MailBulkClaimResult
    {
        public int ClaimedCount { get; set; }
        public int SkippedCount { get; set; }
        public string[] ClaimedMailIds { get; set; } = Array.Empty<string>();
    }
}
