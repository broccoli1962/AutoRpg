using System;
using Backend.Meta.Achievements;
using Backend.Meta.Ads;
using Backend.Meta.Attendance;
using Backend.Meta.Characters;
using Backend.Meta.Currency;
using Backend.Meta.Gacha;
using Backend.Meta.IAP;
using Backend.Meta.Mailbox;
using Backend.Meta.Quests;
using Backend.Meta.SeasonPass;
using Backend.Meta.Shop;
using Backend.Meta.Tutorial;
using Backend.GameSystems.Offline;

namespace Backend.Services.Save
{
    /// <summary>
    /// 런타임 서비스에서 GameSaveSnapshot 을 수집·복원하는 집계기.
    /// </summary>
    public interface IGameSaveAggregator
    {
        /// <summary>
        /// 현재 런타임 상태를 스냅샷으로 내보낸다.
        /// </summary>
        GameSaveSnapshot ExportSnapshot();

        /// <summary>
        /// 스냅샷을 런타임 상태에 적용한다.
        /// </summary>
        void ImportSnapshot(GameSaveSnapshot snapshot);
    }

    /// <summary>
    /// 주입 가능한 게임 세이브 집계기.
    /// </summary>
    public sealed class GameSaveAggregator : IGameSaveAggregator
    {
        private readonly Func<WalletSaveData> _exportWallet;
        private readonly Action<WalletSaveData> _importWallet;
        private readonly Func<ExplorerCatalogSaveData> _exportCatalog;
        private readonly Action<ExplorerCatalogSaveData> _importCatalog;
        private readonly Func<GachaSaveData> _exportGacha;
        private readonly Action<GachaSaveData> _importGacha;
        private readonly Func<AdSaveData> _exportAds;
        private readonly Action<AdSaveData> _importAds;
        private readonly Func<ShopSaveData> _exportShop;
        private readonly Action<ShopSaveData> _importShop;
        private readonly Func<IapSaveData> _exportIap;
        private readonly Action<IapSaveData> _importIap;
        private readonly Func<QuestSaveData> _exportQuests;
        private readonly Action<QuestSaveData> _importQuests;
        private readonly Func<AttendanceSaveData> _exportAttendance;
        private readonly Action<AttendanceSaveData> _importAttendance;
        private readonly Func<MailboxSaveData> _exportMailbox;
        private readonly Action<MailboxSaveData> _importMailbox;
        private readonly Func<AchievementSaveData> _exportAchievements;
        private readonly Action<AchievementSaveData> _importAchievements;
        private readonly Func<SeasonPassSaveData> _exportSeasonPass;
        private readonly Action<SeasonPassSaveData> _importSeasonPass;
        private readonly Func<OfflineProgressSaveData> _exportOffline;
        private readonly Action<OfflineProgressSaveData> _importOffline;
        private readonly Func<TutorialSaveData> _exportTutorial;
        private readonly Action<TutorialSaveData> _importTutorial;

        public GameSaveAggregator(
            Func<WalletSaveData> exportWallet = null,
            Action<WalletSaveData> importWallet = null,
            Func<ExplorerCatalogSaveData> exportCatalog = null,
            Action<ExplorerCatalogSaveData> importCatalog = null,
            Func<GachaSaveData> exportGacha = null,
            Action<GachaSaveData> importGacha = null,
            Func<AdSaveData> exportAds = null,
            Action<AdSaveData> importAds = null,
            Func<ShopSaveData> exportShop = null,
            Action<ShopSaveData> importShop = null,
            Func<IapSaveData> exportIap = null,
            Action<IapSaveData> importIap = null,
            Func<QuestSaveData> exportQuests = null,
            Action<QuestSaveData> importQuests = null,
            Func<AttendanceSaveData> exportAttendance = null,
            Action<AttendanceSaveData> importAttendance = null,
            Func<MailboxSaveData> exportMailbox = null,
            Action<MailboxSaveData> importMailbox = null,
            Func<AchievementSaveData> exportAchievements = null,
            Action<AchievementSaveData> importAchievements = null,
            Func<SeasonPassSaveData> exportSeasonPass = null,
            Action<SeasonPassSaveData> importSeasonPass = null,
            Func<OfflineProgressSaveData> exportOffline = null,
            Action<OfflineProgressSaveData> importOffline = null,
            Func<TutorialSaveData> exportTutorial = null,
            Action<TutorialSaveData> importTutorial = null)
        {
            _exportWallet = exportWallet;
            _importWallet = importWallet;
            _exportCatalog = exportCatalog;
            _importCatalog = importCatalog;
            _exportGacha = exportGacha;
            _importGacha = importGacha;
            _exportAds = exportAds;
            _importAds = importAds;
            _exportShop = exportShop;
            _importShop = importShop;
            _exportIap = exportIap;
            _importIap = importIap;
            _exportQuests = exportQuests;
            _importQuests = importQuests;
            _exportAttendance = exportAttendance;
            _importAttendance = importAttendance;
            _exportMailbox = exportMailbox;
            _importMailbox = importMailbox;
            _exportAchievements = exportAchievements;
            _importAchievements = importAchievements;
            _exportSeasonPass = exportSeasonPass;
            _importSeasonPass = importSeasonPass;
            _exportOffline = exportOffline;
            _importOffline = importOffline;
            _exportTutorial = exportTutorial;
            _importTutorial = importTutorial;
        }

        /// <summary>
        /// 현재 런타임 상태를 스냅샷으로 내보낸다.
        /// </summary>
        public GameSaveSnapshot ExportSnapshot()
        {
            return new GameSaveSnapshot
            {
                SavedAtUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Wallet = _exportWallet?.Invoke() ?? new WalletSaveData(),
                Catalog = _exportCatalog?.Invoke() ?? new ExplorerCatalogSaveData(),
                Gacha = _exportGacha?.Invoke() ?? new GachaSaveData(),
                Ads = _exportAds?.Invoke() ?? new AdSaveData(),
                Shop = _exportShop?.Invoke() ?? new ShopSaveData(),
                Iap = _exportIap?.Invoke() ?? new IapSaveData(),
                Quests = _exportQuests?.Invoke() ?? new QuestSaveData(),
                Attendance = _exportAttendance?.Invoke() ?? new AttendanceSaveData(),
                Mailbox = _exportMailbox?.Invoke() ?? new MailboxSaveData(),
                Achievements = _exportAchievements?.Invoke() ?? new AchievementSaveData(),
                SeasonPass = _exportSeasonPass?.Invoke() ?? new SeasonPassSaveData(),
                Offline = _exportOffline?.Invoke() ?? new OfflineProgressSaveData(),
                Tutorial = _exportTutorial?.Invoke() ?? new TutorialSaveData(),
            };
        }

        /// <summary>
        /// 스냅샷을 런타임 상태에 적용한다.
        /// </summary>
        public void ImportSnapshot(GameSaveSnapshot snapshot)
        {
            if (snapshot == null)
                return;

            if (snapshot.Wallet != null)
                _importWallet?.Invoke(snapshot.Wallet);
            if (snapshot.Catalog != null)
                _importCatalog?.Invoke(snapshot.Catalog);
            if (snapshot.Gacha != null)
                _importGacha?.Invoke(snapshot.Gacha);
            if (snapshot.Ads != null)
                _importAds?.Invoke(snapshot.Ads);
            if (snapshot.Shop != null)
                _importShop?.Invoke(snapshot.Shop);
            if (snapshot.Iap != null)
                _importIap?.Invoke(snapshot.Iap);
            if (snapshot.Quests != null)
                _importQuests?.Invoke(snapshot.Quests);
            if (snapshot.Attendance != null)
                _importAttendance?.Invoke(snapshot.Attendance);
            if (snapshot.Mailbox != null)
                _importMailbox?.Invoke(snapshot.Mailbox);
            if (snapshot.Achievements != null)
                _importAchievements?.Invoke(snapshot.Achievements);
            if (snapshot.SeasonPass != null)
                _importSeasonPass?.Invoke(snapshot.SeasonPass);
            if (snapshot.Offline != null)
                _importOffline?.Invoke(snapshot.Offline);
            if (snapshot.Tutorial != null)
                _importTutorial?.Invoke(snapshot.Tutorial);
        }
    }
}
