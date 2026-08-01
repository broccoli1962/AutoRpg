using System;

namespace Backend.Meta.Achievements
{
    /// <summary>
    /// MetaGameplayEvents 를 AchievementService 에 구독 연결한다.
    /// </summary>
    public sealed class AchievementEventBridge : IDisposable
    {
        private readonly AchievementService _service;
        private readonly AchievementTable _table;

        public AchievementEventBridge(AchievementService service, AchievementTable table)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _table = table ?? throw new ArgumentNullException(nameof(table));

            MetaGameplayEvents.EnemyKillsReported += OnEnemyKillsReported;
            MetaGameplayEvents.FloorReached += OnFloorReached;
            MetaGameplayEvents.EquipmentUpgraded += OnEquipmentUpgraded;
            MetaGameplayEvents.SummonPerformed += OnSummonPerformed;
            MetaGameplayEvents.CollectionProgressReported += OnCollectionProgressReported;
            MetaGameplayEvents.PrestigePerformed += OnPrestigePerformed;
            MetaGameplayEvents.CompendiumEntryAdded += OnCompendiumEntryAdded;
        }

        /// <summary>
        /// 이벤트 구독을 해제한다.
        /// </summary>
        public void Dispose()
        {
            MetaGameplayEvents.EnemyKillsReported -= OnEnemyKillsReported;
            MetaGameplayEvents.FloorReached -= OnFloorReached;
            MetaGameplayEvents.EquipmentUpgraded -= OnEquipmentUpgraded;
            MetaGameplayEvents.SummonPerformed -= OnSummonPerformed;
            MetaGameplayEvents.CollectionProgressReported -= OnCollectionProgressReported;
            MetaGameplayEvents.PrestigePerformed -= OnPrestigePerformed;
            MetaGameplayEvents.CompendiumEntryAdded -= OnCompendiumEntryAdded;
        }

        private void OnEnemyKillsReported(int count)
        {
            _service.ReportProgress(AchievementCategory.TotalKills, count, _table);
        }

        private void OnFloorReached(int floor)
        {
            _service.ReportHighestFloor(floor, _table);
        }

        private void OnEquipmentUpgraded(int count)
        {
            _service.ReportProgress(AchievementCategory.EquipmentUpgrades, count, _table);
        }

        private void OnSummonPerformed(int count)
        {
            _service.ReportProgress(AchievementCategory.SummonCount, count, _table);
        }

        private void OnCollectionProgressReported(int ownedUniqueCount, int totalAvailableCount)
        {
            _service.ReportCollectionCompletion(ownedUniqueCount, totalAvailableCount, _table);
        }

        private void OnPrestigePerformed()
        {
            _service.ReportProgress(AchievementCategory.PrestigeCount, 1L, _table);
        }

        private void OnCompendiumEntryAdded(int count)
        {
            _service.ReportProgress(AchievementCategory.CompendiumEntries, count, _table);
        }
    }
}
