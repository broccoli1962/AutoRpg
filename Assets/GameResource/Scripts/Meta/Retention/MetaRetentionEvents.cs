using System;
using Backend.Meta.Quests;

namespace Backend.Meta.Retention
{
    /// <summary>
    /// 출석·퀘스트 등 리텐션 이벤트 채널.
    /// </summary>
    public static class MetaRetentionEvents
    {
        public static event Action<QuestPeriod> QuestRewardClaimed;

        /// <summary>
        /// 퀘스트 보상 수령을 발행한다.
        /// </summary>
        public static void ReportQuestRewardClaimed(QuestPeriod period)
        {
            QuestRewardClaimed?.Invoke(period);
        }

        /// <summary>
        /// 테스트·씬 전환용 구독자를 모두 해제한다.
        /// </summary>
        public static void ClearSubscribers()
        {
            QuestRewardClaimed = null;
        }
    }
}
