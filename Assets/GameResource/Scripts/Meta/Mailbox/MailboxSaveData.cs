using System;

namespace Backend.Meta.Mailbox
{
    /// <summary>
    /// 우편함 세이브 스냅샷.
    /// </summary>
    [Serializable]
    public sealed class MailboxSaveData
    {
        public MailEntry[] Mails = Array.Empty<MailEntry>();
        public long NextMailSequence = 1;
    }
}
