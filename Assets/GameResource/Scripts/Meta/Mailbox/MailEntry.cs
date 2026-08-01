using System;
using Backend.Meta.Currency;

namespace Backend.Meta.Mailbox
{
    /// <summary>
    /// 우편함 항목.
    /// </summary>
    [Serializable]
    public sealed class MailEntry
    {
        public string MailId;
        public MailType Type;
        public string Title;
        public string Body;
        public CurrencyRewardEntry[] Rewards = Array.Empty<CurrencyRewardEntry>();
        public long ExpiresAtUtcTicks;
        public bool IsClaimed;
        public long CreatedAtUtcTicks;
    }

    /// <summary>
    /// 우편 첨부 재화 보상.
    /// </summary>
    [Serializable]
    public struct CurrencyRewardEntry
    {
        public CurrencyType CurrencyType;
        public long Amount;
    }
}
