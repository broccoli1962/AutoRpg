using System;
using System.Collections.Generic;
using Backend.GameSystems.Offline;
using Backend.Meta.Currency;
using Backend.Meta.Mailbox;
using Backend.Meta.Retention;

namespace Backend.Meta.SeasonPass
{
    /// <summary>
    /// 시즌 포인트·트랙 보상·시즌 종료 이관을 담당한다.
    /// </summary>
    public sealed class SeasonPassService
    {
        private const string TIER_NOT_FOUND = "Season pass tier not found.";
        private const string TIER_NOT_REACHED = "Season pass tier is not reached.";
        private const string TIER_ALREADY_CLAIMED = "Season pass tier reward already claimed.";
        private const string PREMIUM_NOT_UNLOCKED = "Premium season pass track is not unlocked.";
        private const string PREMIUM_ALREADY_UNLOCKED = "Premium season pass track is already unlocked.";
        private const string NO_ACTIVE_SEASON = "No active season pass season.";
        private const string MAIL_TITLE = "Season Pass Unclaimed Rewards";
        private const string MAIL_BODY = "Unclaimed season pass rewards from the previous season.";
        private const int SEASON_END_REMINDER_DAYS = 3;

        private readonly Wallet _wallet;
        private readonly IServerTimeProvider _serverTimeProvider;
        private readonly Func<DateTimeOffset> _localUtcNow;
        private readonly ISeasonPassPremiumSync _premiumSync;
        private readonly ISeasonPassPushNotifier _pushNotifier;

        private int _seasonNumber;
        private int _seasonPoints;
        private int _dailyPointsEarned;
        private int _dailyPeriodKey;
        private bool _isPremiumUnlocked;
        private int _lastRewardedFloor;
        private int _scheduledPushSeasonNumber;
        private readonly HashSet<int> _claimedFreeTiers = new();
        private readonly HashSet<int> _claimedPremiumTiers = new();

        public SeasonPassService(
            Wallet wallet,
            IServerTimeProvider serverTimeProvider = null,
            Func<DateTimeOffset> localUtcNow = null,
            ISeasonPassPremiumSync premiumSync = null,
            ISeasonPassPushNotifier pushNotifier = null)
        {
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _serverTimeProvider = serverTimeProvider;
            _localUtcNow = localUtcNow;
            _premiumSync = premiumSync ?? new NullSeasonPassPremiumSync();
            _pushNotifier = pushNotifier ?? new NullSeasonPassPushNotifier();
        }

        /// <summary>
        /// 현재 시즌 번호를 반환한다.
        /// </summary>
        public int SeasonNumber => _seasonNumber;

        /// <summary>
        /// 현재 시즌 포인트를 반환한다.
        /// </summary>
        public int SeasonPoints => _seasonPoints;

        /// <summary>
        /// 오늘 획득한 시즌 포인트를 반환한다.
        /// </summary>
        public int DailyPointsEarned => _dailyPointsEarned;

        /// <summary>
        /// 프리미엄 트랙 해금 여부를 반환한다.
        /// </summary>
        public bool IsPremiumUnlocked => _isPremiumUnlocked;

        /// <summary>
        /// 무료 트랙 보상 수령 여부를 반환한다.
        /// </summary>
        public bool IsFreeTierClaimed(int tierIndex)
        {
            return tierIndex > 0 && _claimedFreeTiers.Contains(tierIndex);
        }

        /// <summary>
        /// 프리미엄 트랙 보상 수령 여부를 반환한다.
        /// </summary>
        public bool IsPremiumTierClaimed(int tierIndex)
        {
            return tierIndex > 0 && _claimedPremiumTiers.Contains(tierIndex);
        }

        /// <summary>
        /// 단계 달성 여부를 반환한다.
        /// </summary>
        public bool IsTierReached(int tierIndex, SeasonPassTable table)
        {
            var tier = table?.FindTier(tierIndex);
            return tier != null && _seasonPoints >= tier.RequiredPoints;
        }

        /// <summary>
        /// 서버 시간·시즌 경계·일일 상한을 갱신한다.
        /// </summary>
        public void RefreshSeason(SeasonPassTable table, MailboxService mailbox = null)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            RefreshDailyCapInternal();

            var now = ResolveNowUtc();
            var activeSeason = table.ResolveActiveSeason(now);

            if (activeSeason == null)
                return;

            if (_seasonNumber != 0 && activeSeason.SeasonNumber != _seasonNumber)
            {
                if (mailbox != null)
                    MigrateUnclaimedToMailbox(table, mailbox, _seasonNumber);

                ResetForSeason(activeSeason, table);
            }
            else if (_seasonNumber == 0)
            {
                ResetForSeason(activeSeason, table);
            }

            UpdateSeasonEndPushSchedule(table, activeSeason);
        }

        /// <summary>
        /// 일일 퀘스트 완료 시 시즌 포인트를 지급한다.
        /// </summary>
        public int ReportDailyQuestComplete(SeasonPassTable table)
        {
            return ReportPoints(
                table,
                SeasonPointSource.DailyQuestComplete,
                table.GetPointsForSource(SeasonPointSource.DailyQuestComplete));
        }

        /// <summary>
        /// 주간 퀘스트 완료 시 시즌 포인트를 지급한다.
        /// </summary>
        public int ReportWeeklyQuestComplete(SeasonPassTable table)
        {
            return ReportPoints(
                table,
                SeasonPointSource.WeeklyQuestComplete,
                table.GetPointsForSource(SeasonPointSource.WeeklyQuestComplete));
        }

        /// <summary>
        /// 신규 최고 층 도달 시 시즌 포인트를 지급한다.
        /// </summary>
        public int ReportFloorReached(int floor, SeasonPassTable table)
        {
            if (floor <= _lastRewardedFloor)
                return 0;

            _lastRewardedFloor = floor;
            return ReportPoints(
                table,
                SeasonPointSource.FloorReached,
                table.GetPointsForSource(SeasonPointSource.FloorReached));
        }

        /// <summary>
        /// 보스 처치 시 시즌 포인트를 지급한다.
        /// </summary>
        public int ReportBossKill(SeasonPassTable table)
        {
            return ReportPoints(
                table,
                SeasonPointSource.BossKill,
                table.GetPointsForSource(SeasonPointSource.BossKill));
        }

        /// <summary>
        /// 프리미엄 트랙을 해금하고 달성 단계 보상을 소급 지급한다.
        /// </summary>
        public SeasonPassUnlockResult UnlockPremium(SeasonPassTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            RefreshSeason(table);

            if (_isPremiumUnlocked)
                return SeasonPassUnlockResult.Failed(PREMIUM_ALREADY_UNLOCKED);

            _isPremiumUnlocked = true;
            _premiumSync.PersistPremiumUnlocked(_seasonNumber, true);

            var retroactiveCount = GrantRetroactivePremiumRewards(table);
            return SeasonPassUnlockResult.Succeeded(retroactiveCount);
        }

        /// <summary>
        /// 무료 트랙 단계 보상을 수령한다.
        /// </summary>
        public SeasonPassClaimResult TryClaimFreeTier(int tierIndex, SeasonPassTable table)
        {
            return TryClaimTierInternal(tierIndex, table, isPremiumTrack: false);
        }

        /// <summary>
        /// 프리미엄 트랙 단계 보상을 수령한다.
        /// </summary>
        public SeasonPassClaimResult TryClaimPremiumTier(int tierIndex, SeasonPassTable table)
        {
            if (!_isPremiumUnlocked)
                return SeasonPassClaimResult.Failed(tierIndex, true, PREMIUM_NOT_UNLOCKED);

            return TryClaimTierInternal(tierIndex, table, isPremiumTrack: true);
        }

        /// <summary>
        /// 시즌 종료 시 미수령 보상을 우편함으로 이관한다.
        /// </summary>
        public SeasonPassEndMigrationResult MigrateUnclaimedToMailbox(
            SeasonPassTable table,
            MailboxService mailbox,
            int endedSeasonNumber)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            if (mailbox == null)
                throw new ArgumentNullException(nameof(mailbox));

            var rewards = new List<CurrencyRewardEntry>();
            var migratedTierCount = 0;

            foreach (var tier in table.Tiers)
            {
                if (tier == null)
                    continue;

                if (_seasonPoints < tier.RequiredPoints)
                    continue;

                var hasUnclaimed = false;

                if (!_claimedFreeTiers.Contains(tier.TierIndex))
                {
                    AppendRewards(rewards, tier.FreeRewards);
                    hasUnclaimed = true;
                }

                if (_isPremiumUnlocked && !_claimedPremiumTiers.Contains(tier.TierIndex))
                {
                    AppendRewards(rewards, tier.PremiumRewards);
                    hasUnclaimed = true;
                }

                if (hasUnclaimed)
                    migratedTierCount++;
            }

            if (rewards.Count == 0)
                return new SeasonPassEndMigrationResult(0, 0);

            var expiresAt = ResolveNowUtc().AddDays(30);
            mailbox.AddRewardMail(
                MAIL_TITLE,
                $"{MAIL_BODY} (Season {endedSeasonNumber})",
                rewards.ToArray(),
                expiresAt);

            return new SeasonPassEndMigrationResult(migratedTierCount, rewards.Count);
        }

        /// <summary>
        /// 세이브용 스냅샷을 생성한다.
        /// </summary>
        public SeasonPassSaveData ToSaveData()
        {
            var freeTiers = new int[_claimedFreeTiers.Count];
            _claimedFreeTiers.CopyTo(freeTiers);

            var premiumTiers = new int[_claimedPremiumTiers.Count];
            _claimedPremiumTiers.CopyTo(premiumTiers);

            return new SeasonPassSaveData
            {
                SeasonNumber = _seasonNumber,
                SeasonPoints = _seasonPoints,
                DailyPointsEarned = _dailyPointsEarned,
                DailyPeriodKey = _dailyPeriodKey,
                IsPremiumUnlocked = _isPremiumUnlocked,
                LastRewardedFloor = _lastRewardedFloor,
                ClaimedFreeTierIndices = freeTiers,
                ClaimedPremiumTierIndices = premiumTiers,
            };
        }

        /// <summary>
        /// 세이브 스냅샷에서 SeasonPassService 를 복원한다.
        /// </summary>
        public static SeasonPassService FromSaveData(
            SeasonPassSaveData saveData,
            Wallet wallet,
            IServerTimeProvider serverTimeProvider = null,
            Func<DateTimeOffset> localUtcNow = null,
            ISeasonPassPremiumSync premiumSync = null,
            ISeasonPassPushNotifier pushNotifier = null)
        {
            var service = new SeasonPassService(
                wallet,
                serverTimeProvider,
                localUtcNow,
                premiumSync,
                pushNotifier);

            if (saveData == null)
                return service;

            service._seasonNumber = saveData.SeasonNumber;
            service._seasonPoints = Math.Max(0, saveData.SeasonPoints);
            service._dailyPointsEarned = Math.Max(0, saveData.DailyPointsEarned);
            service._dailyPeriodKey = saveData.DailyPeriodKey;
            service._isPremiumUnlocked = saveData.IsPremiumUnlocked;
            service._lastRewardedFloor = Math.Max(0, saveData.LastRewardedFloor);

            if (saveData.ClaimedFreeTierIndices != null)
            {
                foreach (var tierIndex in saveData.ClaimedFreeTierIndices)
                {
                    if (tierIndex > 0)
                        service._claimedFreeTiers.Add(tierIndex);
                }
            }

            if (saveData.ClaimedPremiumTierIndices != null)
            {
                foreach (var tierIndex in saveData.ClaimedPremiumTierIndices)
                {
                    if (tierIndex > 0)
                        service._claimedPremiumTiers.Add(tierIndex);
                }
            }

            if (premiumSync != null
                && service._seasonNumber > 0
                && premiumSync.TryRestorePremiumUnlocked(service._seasonNumber, out var serverPremium))
            {
                service._isPremiumUnlocked = serverPremium;
            }

            service.RefreshDailyCapInternal();
            return service;
        }

        private int ReportPoints(SeasonPassTable table, SeasonPointSource source, int amount)
        {
            if (table == null || amount <= 0)
                return 0;

            RefreshSeason(table);
            RefreshDailyCapInternal();

            if (table.ResolveActiveSeason(ResolveNowUtc()) == null)
                return 0;

            var dailyCap = table.PointConfig?.DailyEarnCap ?? 0;
            if (dailyCap > 0 && _dailyPointsEarned >= dailyCap)
                return 0;

            var granted = amount;
            if (dailyCap > 0)
            {
                var remaining = dailyCap - _dailyPointsEarned;
                granted = Math.Min(granted, remaining);
            }

            if (granted <= 0)
                return 0;

            _seasonPoints += granted;
            _dailyPointsEarned += granted;
            return granted;
        }

        private SeasonPassClaimResult TryClaimTierInternal(
            int tierIndex,
            SeasonPassTable table,
            bool isPremiumTrack)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            RefreshSeason(table);

            var tier = table.FindTier(tierIndex);
            if (tier == null)
                return SeasonPassClaimResult.Failed(tierIndex, isPremiumTrack, TIER_NOT_FOUND);

            var claimedSet = isPremiumTrack ? _claimedPremiumTiers : _claimedFreeTiers;
            if (claimedSet.Contains(tierIndex))
                return SeasonPassClaimResult.Failed(tierIndex, isPremiumTrack, TIER_ALREADY_CLAIMED);

            if (_seasonPoints < tier.RequiredPoints)
                return SeasonPassClaimResult.Failed(tierIndex, isPremiumTrack, TIER_NOT_REACHED);

            var rewards = isPremiumTrack ? tier.PremiumRewards : tier.FreeRewards;
            var reasonCode = isPremiumTrack
                ? CurrencyReasonCodes.SeasonPassPremiumReward
                : CurrencyReasonCodes.SeasonPassFreeReward;

            if (!CreditRewards(rewards, reasonCode))
                return SeasonPassClaimResult.Failed(tierIndex, isPremiumTrack, "Failed to credit reward.");

            claimedSet.Add(tierIndex);
            return SeasonPassClaimResult.Succeeded(tierIndex, isPremiumTrack);
        }

        private int GrantRetroactivePremiumRewards(SeasonPassTable table)
        {
            var retroactiveCount = 0;

            foreach (var tier in table.Tiers)
            {
                if (tier == null)
                    continue;

                if (_seasonPoints < tier.RequiredPoints)
                    continue;

                if (_claimedPremiumTiers.Contains(tier.TierIndex))
                    continue;

                if (!CreditRewards(tier.PremiumRewards, CurrencyReasonCodes.SeasonPassPremiumRetroactive))
                    continue;

                _claimedPremiumTiers.Add(tier.TierIndex);
                retroactiveCount++;
            }

            return retroactiveCount;
        }

        private static void AppendRewards(
            List<CurrencyRewardEntry> destination,
            SeasonPassRewardEntry[] source)
        {
            if (source == null)
                return;

            foreach (var reward in source)
            {
                if (reward.Amount <= 0L)
                    continue;

                destination.Add(new CurrencyRewardEntry
                {
                    CurrencyType = reward.CurrencyType,
                    Amount = reward.Amount,
                });
            }
        }

        private bool CreditRewards(SeasonPassRewardEntry[] rewards, string reasonCode)
        {
            if (rewards == null)
                return true;

            foreach (var reward in rewards)
            {
                if (reward.Amount <= 0L)
                    continue;

                var result = _wallet.TryCredit(reward.CurrencyType, reward.Amount, reasonCode);
                if (!result.Success)
                    return false;
            }

            return true;
        }

        private void ResetForSeason(SeasonDefinition season, SeasonPassTable table)
        {
            if (_seasonNumber != 0 && _seasonNumber != season.SeasonNumber)
                _pushNotifier.CancelSeasonEndReminder(_seasonNumber);

            _seasonNumber = season.SeasonNumber;
            _seasonPoints = 0;
            _dailyPointsEarned = 0;
            _lastRewardedFloor = 0;
            _claimedFreeTiers.Clear();
            _claimedPremiumTiers.Clear();
            _scheduledPushSeasonNumber = 0;

            _isPremiumUnlocked = false;
            if (_premiumSync.TryRestorePremiumUnlocked(_seasonNumber, out var serverPremium))
                _isPremiumUnlocked = serverPremium;

            RefreshDailyCapInternal();
            UpdateSeasonEndPushSchedule(table, season);
        }

        private void UpdateSeasonEndPushSchedule(SeasonPassTable table, SeasonDefinition season)
        {
            if (season == null || _scheduledPushSeasonNumber == season.SeasonNumber)
                return;

            var endUtc = new DateTimeOffset(season.EndUtcTicks, TimeSpan.Zero);
            var reminderUtc = endUtc.AddDays(-SEASON_END_REMINDER_DAYS);
            var now = ResolveNowUtc();

            if (now < reminderUtc)
            {
                _pushNotifier.ScheduleSeasonEndReminder(season.SeasonNumber, endUtc);
                _scheduledPushSeasonNumber = season.SeasonNumber;
            }
        }

        private void RefreshDailyCapInternal()
        {
            var dayKey = DailyResetClock.GetDayKey(ResolveNowUtc());
            if (_dailyPeriodKey != 0 && dayKey != _dailyPeriodKey)
                _dailyPointsEarned = 0;

            _dailyPeriodKey = dayKey;
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
