using Backend.Meta.Achievements;

namespace Backend.Meta.Tutorial
{
    /// <summary>
    /// MetaGameplayEvents 를 TutorialService 에 구독 연결한다.
    /// </summary>
    public sealed class TutorialEventBridge
    {
        private readonly TutorialService _service;
        private readonly TutorialTable _table;

        public TutorialEventBridge(TutorialService service, TutorialTable table)
        {
            _service = service;
            _table = table;
        }

        /// <summary>
        /// 이벤트 구독을 시작한다.
        /// </summary>
        public void Subscribe()
        {
            MetaGameplayEvents.DispatchStarted += OnDispatchStarted;
            MetaGameplayEvents.EnemyKillsReported += OnEnemyKilled;
            MetaGameplayEvents.EquipmentUpgraded += OnEquipmentUpgraded;
            MetaGameplayEvents.EquipmentEquipped += OnEquipmentEquipped;
            MetaGameplayEvents.SummonPerformed += OnSummonPerformed;
            MetaGameplayEvents.OfflineRewardClaimed += OnOfflineRewardClaimed;
        }

        /// <summary>
        /// 이벤트 구독을 해제한다.
        /// </summary>
        public void Unsubscribe()
        {
            MetaGameplayEvents.DispatchStarted -= OnDispatchStarted;
            MetaGameplayEvents.EnemyKillsReported -= OnEnemyKilled;
            MetaGameplayEvents.EquipmentUpgraded -= OnEquipmentUpgraded;
            MetaGameplayEvents.EquipmentEquipped -= OnEquipmentEquipped;
            MetaGameplayEvents.SummonPerformed -= OnSummonPerformed;
            MetaGameplayEvents.OfflineRewardClaimed -= OnOfflineRewardClaimed;
        }

        private void OnDispatchStarted()
        {
            _service.TryHandleTrigger(TutorialCompletionTrigger.DispatchStarted, _table);
        }

        private void OnEnemyKilled(int count)
        {
            if (count > 0)
                _service.TryHandleTrigger(TutorialCompletionTrigger.EnemyKilled, _table);
        }

        private void OnEquipmentUpgraded(int count)
        {
            if (count > 0)
                _service.TryHandleTrigger(TutorialCompletionTrigger.EquipmentUpgraded, _table);
        }

        private void OnEquipmentEquipped(int count)
        {
            if (count > 0)
                _service.TryHandleTrigger(TutorialCompletionTrigger.EquipmentEquipped, _table);
        }

        private void OnSummonPerformed(int count)
        {
            if (count > 0)
                _service.TryHandleTrigger(TutorialCompletionTrigger.SummonPerformed, _table);
        }

        private void OnOfflineRewardClaimed()
        {
            _service.TryHandleTrigger(TutorialCompletionTrigger.OfflineRewardClaimed, _table);
        }
    }
}
