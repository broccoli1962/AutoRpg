using System;
using Backend.GameSystems.Offline;
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

namespace Backend.Services.Save
{
    /// <summary>
    /// 전체 게임 세이브 스냅샷. 로컬·클라우드 백업 단위.
    /// </summary>
    [Serializable]
    public sealed class GameSaveSnapshot
    {
        public int SchemaVersion = 1;
        public long SavedAtUnixSeconds;
        public WalletSaveData Wallet = new();
        public ExplorerCatalogSaveData Catalog = new();
        public GachaSaveData Gacha = new();
        public AdSaveData Ads = new();
        public ShopSaveData Shop = new();
        public IapSaveData Iap = new();
        public QuestSaveData Quests = new();
        public AttendanceSaveData Attendance = new();
        public MailboxSaveData Mailbox = new();
        public AchievementSaveData Achievements = new();
        public SeasonPassSaveData SeasonPass = new();
        public OfflineProgressSaveData Offline = new();
    }
}
