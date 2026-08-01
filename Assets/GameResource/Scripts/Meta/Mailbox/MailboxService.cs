using System;
using System.Collections.Generic;
using Backend.GameSystems.Offline;
using Backend.Meta.Currency;

namespace Backend.Meta.Mailbox
{
    /// <summary>
    /// 우편함 보상 지급·공지·만료·일괄 수령을 담당한다.
    /// </summary>
    public sealed class MailboxService
    {
        private const string MAIL_NOT_FOUND = "Mail not found.";
        private const string MAIL_EXPIRED = "Mail has expired.";
        private const string MAIL_ALREADY_CLAIMED = "Mail reward already claimed.";
        private const string MAIL_HAS_NO_REWARD = "Mail has no claimable reward.";

        private readonly Wallet _wallet;
        private readonly IServerTimeProvider _serverTimeProvider;
        private readonly Func<DateTimeOffset> _localUtcNow;
        private readonly List<MailEntry> _mails = new();
        private long _nextMailSequence = 1;

        public MailboxService(
            Wallet wallet,
            IServerTimeProvider serverTimeProvider = null,
            Func<DateTimeOffset> localUtcNow = null)
        {
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _serverTimeProvider = serverTimeProvider;
            _localUtcNow = localUtcNow;
        }

        /// <summary>
        /// 읽기 전용 우편 목록.
        /// </summary>
        public IReadOnlyList<MailEntry> Mails => _mails;

        /// <summary>
        /// 보상 우편을 추가한다.
        /// </summary>
        public MailEntry AddRewardMail(
            string title,
            string body,
            CurrencyRewardEntry[] rewards,
            DateTimeOffset expiresAtUtc)
        {
            return AddMailInternal(MailType.Reward, title, body, rewards, expiresAtUtc);
        }

        /// <summary>
        /// 공지 우편을 추가한다.
        /// </summary>
        public MailEntry AddNoticeMail(
            string title,
            string body,
            DateTimeOffset expiresAtUtc)
        {
            return AddMailInternal(MailType.Notice, title, body, Array.Empty<CurrencyRewardEntry>(), expiresAtUtc);
        }

        /// <summary>
        /// 만료된 우편을 제거한다.
        /// </summary>
        public int PurgeExpired()
        {
            var nowTicks = ResolveNowUtc().UtcTicks;
            var removed = 0;

            for (var i = _mails.Count - 1; i >= 0; i--)
            {
                var mail = _mails[i];
                if (mail.ExpiresAtUtcTicks > 0L && mail.ExpiresAtUtcTicks <= nowTicks)
                {
                    _mails.RemoveAt(i);
                    removed++;
                }
            }

            return removed;
        }

        /// <summary>
        /// 단일 우편 보상을 수령한다.
        /// </summary>
        public MailClaimResult TryClaim(string mailId)
        {
            var mail = FindMail(mailId);
            if (mail == null)
                return MailClaimResult.Failed(mailId, MAIL_NOT_FOUND);

            if (IsExpired(mail))
                return MailClaimResult.Failed(mailId, MAIL_EXPIRED);

            if (mail.Type == MailType.Notice)
                return MailClaimResult.Failed(mailId, MAIL_HAS_NO_REWARD);

            if (mail.IsClaimed)
                return MailClaimResult.Failed(mailId, MAIL_ALREADY_CLAIMED);

            if (!CreditRewards(mail.Rewards))
                return MailClaimResult.Failed(mailId, "Failed to credit reward.");

            mail.IsClaimed = true;
            return MailClaimResult.Succeeded(mailId);
        }

        /// <summary>
        /// 수령 가능한 모든 보상 우편을 일괄 수령한다.
        /// </summary>
        public MailBulkClaimResult TryClaimAll()
        {
            PurgeExpired();

            var claimedIds = new List<string>();
            var skipped = 0;

            foreach (var mail in _mails)
            {
                if (mail.Type != MailType.Reward)
                    continue;

                if (mail.IsClaimed || IsExpired(mail))
                {
                    skipped++;
                    continue;
                }

                var result = TryClaim(mail.MailId);
                if (result.Success)
                    claimedIds.Add(mail.MailId);
                else
                    skipped++;
            }

            return new MailBulkClaimResult
            {
                ClaimedCount = claimedIds.Count,
                SkippedCount = skipped,
                ClaimedMailIds = claimedIds.ToArray(),
            };
        }

        /// <summary>
        /// 세이브용 스냅샷을 생성한다.
        /// </summary>
        public MailboxSaveData ToSaveData()
        {
            return new MailboxSaveData
            {
                Mails = _mails.ToArray(),
                NextMailSequence = _nextMailSequence,
            };
        }

        /// <summary>
        /// 세이브 스냅샷에서 MailboxService 를 복원한다.
        /// </summary>
        public static MailboxService FromSaveData(
            MailboxSaveData saveData,
            Wallet wallet,
            IServerTimeProvider serverTimeProvider = null,
            Func<DateTimeOffset> localUtcNow = null)
        {
            var service = new MailboxService(wallet, serverTimeProvider, localUtcNow);

            if (saveData == null)
                return service;

            service._nextMailSequence = saveData.NextMailSequence > 0L
                ? saveData.NextMailSequence
                : 1L;

            if (saveData.Mails != null)
            {
                foreach (var mail in saveData.Mails)
                {
                    if (mail != null)
                        service._mails.Add(mail);
                }
            }

            return service;
        }

        private MailEntry AddMailInternal(
            MailType type,
            string title,
            string body,
            CurrencyRewardEntry[] rewards,
            DateTimeOffset expiresAtUtc)
        {
            var now = ResolveNowUtc();
            var mail = new MailEntry
            {
                MailId = $"mail_{_nextMailSequence++}",
                Type = type,
                Title = title ?? string.Empty,
                Body = body ?? string.Empty,
                Rewards = rewards ?? Array.Empty<CurrencyRewardEntry>(),
                ExpiresAtUtcTicks = expiresAtUtc.UtcTicks,
                IsClaimed = false,
                CreatedAtUtcTicks = now.UtcTicks,
            };

            _mails.Add(mail);
            return mail;
        }

        private MailEntry FindMail(string mailId)
        {
            if (string.IsNullOrEmpty(mailId))
                return null;

            foreach (var mail in _mails)
            {
                if (mail.MailId == mailId)
                    return mail;
            }

            return null;
        }

        private bool IsExpired(MailEntry mail)
        {
            if (mail.ExpiresAtUtcTicks <= 0L)
                return false;

            return mail.ExpiresAtUtcTicks <= ResolveNowUtc().UtcTicks;
        }

        private bool CreditRewards(CurrencyRewardEntry[] rewards)
        {
            if (rewards == null)
                return true;

            foreach (var reward in rewards)
            {
                if (reward.Amount <= 0L)
                    continue;

                var result = _wallet.TryCredit(
                    reward.CurrencyType,
                    reward.Amount,
                    CurrencyReasonCodes.MailboxReward);

                if (!result.Success)
                    return false;
            }

            return true;
        }

        private DateTimeOffset ResolveNowUtc()
        {
            if (_serverTimeProvider != null
                && _serverTimeProvider.TryGetServerTimeUtc(out var serverTimeUtc))
            {
                return serverTimeUtc;
            }

            return _localUtcNow != null ? _localUtcNow() : DateTimeOffset.UtcNow;
        }
    }
}
