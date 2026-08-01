namespace Backend.Meta.Tutorial
{
    /// <summary>
    /// FTUE 튜토리얼 6단계 식별자.
    /// </summary>
    public enum TutorialStepId
    {
        None = 0,
        FirstDispatch = 1,
        FirstKillObservation = 2,
        FirstEnhancement = 3,
        FirstEquip = 4,
        FirstSummon = 5,
        FirstOfflineReward = 6,
    }

    /// <summary>
    /// 단계 완료를 판정하는 게임플레이 트리거.
    /// </summary>
    public enum TutorialCompletionTrigger
    {
        DispatchStarted = 0,
        EnemyKilled = 1,
        EquipmentUpgraded = 2,
        EquipmentEquipped = 3,
        SummonPerformed = 4,
        OfflineRewardClaimed = 5,
    }
}
