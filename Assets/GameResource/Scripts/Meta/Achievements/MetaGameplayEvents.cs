using System;

namespace Backend.Meta.Achievements
{
    /// <summary>
    /// 게임플레이 이벤트를 업적 진행도에 연결하는 정적 채널.
    /// </summary>
    public static class MetaGameplayEvents
    {
        public static event Action<int> EnemyKillsReported;
        public static event Action<int> FloorReached;
        public static event Action<int> EquipmentUpgraded;
        public static event Action<int> SummonPerformed;
        public static event Action<int, int> CollectionProgressReported;
        public static event Action PrestigePerformed;
        public static event Action<int> CompendiumEntryAdded;
        public static event Action<int> BossKilled;

        /// <summary>
        /// 적 처치 수를 발행한다.
        /// </summary>
        public static void ReportEnemyKills(int count)
        {
            if (count > 0)
                EnemyKillsReported?.Invoke(count);
        }

        /// <summary>
        /// 도달 층을 발행한다.
        /// </summary>
        public static void ReportFloorReached(int floor)
        {
            if (floor > 0)
                FloorReached?.Invoke(floor);
        }

        /// <summary>
        /// 장비 강화 횟수를 발행한다.
        /// </summary>
        public static void ReportEquipmentUpgrade(int count = 1)
        {
            if (count > 0)
                EquipmentUpgraded?.Invoke(count);
        }

        /// <summary>
        /// 소환 횟수를 발행한다.
        /// </summary>
        public static void ReportSummon(int count = 1)
        {
            if (count > 0)
                SummonPerformed?.Invoke(count);
        }

        /// <summary>
        /// 수집 완성도(보유/전체)를 발행한다.
        /// </summary>
        public static void ReportCollectionProgress(int ownedUniqueCount, int totalAvailableCount)
        {
            if (totalAvailableCount > 0 && ownedUniqueCount >= 0)
                CollectionProgressReported?.Invoke(ownedUniqueCount, totalAvailableCount);
        }

        /// <summary>
        /// 프레스티지 수행을 발행한다.
        /// </summary>
        public static void ReportPrestige()
        {
            PrestigePerformed?.Invoke();
        }

        /// <summary>
        /// 도감 등재를 발행한다.
        /// </summary>
        public static void ReportCompendiumEntry(int count = 1)
        {
            if (count > 0)
                CompendiumEntryAdded?.Invoke(count);
        }

        /// <summary>
        /// 보스 처치 수를 발행한다.
        /// </summary>
        public static void ReportBossKill(int count = 1)
        {
            if (count > 0)
                BossKilled?.Invoke(count);
        }

        /// <summary>
        /// 테스트·씬 전환용 구독자를 모두 해제한다.
        /// </summary>
        public static void ClearSubscribers()
        {
            EnemyKillsReported = null;
            FloorReached = null;
            EquipmentUpgraded = null;
            SummonPerformed = null;
            CollectionProgressReported = null;
            PrestigePerformed = null;
            CompendiumEntryAdded = null;
            BossKilled = null;
        }
    }
}
