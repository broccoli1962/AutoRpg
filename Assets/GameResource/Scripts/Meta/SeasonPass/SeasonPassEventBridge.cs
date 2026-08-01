using System;
using Backend.Meta.Achievements;
using Backend.Meta.Quests;
using Backend.Meta.Retention;

namespace Backend.Meta.SeasonPass
{
    /// <summary>
    /// 게임플레이·퀘스트 이벤트를 SeasonPassService 에 구독 연결한다.
    /// </summary>
    public sealed class SeasonPassEventBridge : IDisposable
    {
        private readonly SeasonPassService _service;
        private readonly SeasonPassTable _table;

        public SeasonPassEventBridge(SeasonPassService service, SeasonPassTable table)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _table = table ?? throw new ArgumentNullException(nameof(table));

            MetaGameplayEvents.FloorReached += OnFloorReached;
            MetaGameplayEvents.BossKilled += OnBossKilled;
            MetaRetentionEvents.QuestRewardClaimed += OnQuestRewardClaimed;
        }

        /// <summary>
        /// 이벤트 구독을 해제한다.
        /// </summary>
        public void Dispose()
        {
            MetaGameplayEvents.FloorReached -= OnFloorReached;
            MetaGameplayEvents.BossKilled -= OnBossKilled;
            MetaRetentionEvents.QuestRewardClaimed -= OnQuestRewardClaimed;
        }

        private void OnFloorReached(int floor)
        {
            _service.ReportFloorReached(floor, _table);
        }

        private void OnBossKilled(int count)
        {
            for (var i = 0; i < count; i++)
                _service.ReportBossKill(_table);
        }

        private void OnQuestRewardClaimed(QuestPeriod period)
        {
            if (period == QuestPeriod.Daily)
                _service.ReportDailyQuestComplete(_table);
            else
                _service.ReportWeeklyQuestComplete(_table);
        }
    }
}
