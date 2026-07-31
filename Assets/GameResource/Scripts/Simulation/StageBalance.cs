namespace Backend.Simulation
{
    /// <summary>
    /// 스테이지·전투 진행 시 BalanceTable 기반 수치를 조회한다.
    /// </summary>
    public static class StageBalance
    {
        /// <summary>
        /// 통산 층의 몬스터 스탯 스냅샷을 반환한다.
        /// </summary>
        public static MonsterStatSnapshot GetMonsterStats(int floor, BalanceTable table = null)
        {
            table ??= BalanceTableProvider.Get();
            return new MonsterStatSnapshot(
                floor,
                BalanceFormulas.GetMonsterHp(table, floor),
                BalanceFormulas.GetMonsterAtk(table, floor),
                BalanceFormulas.GetMonsterDef(table, floor),
                BalanceFormulas.GetGoldDrop(table, floor));
        }

        /// <summary>
        /// 공격자가 몬스터에게 가하는 1회 피해를 계산한다.
        /// </summary>
        public static float CalculateHitDamage(
            float attackerAtk,
            MonsterStatSnapshot monster,
            float critRate,
            float critDamage,
            float elementMultiplier,
            float varianceMultiplier,
            BalanceTable table = null)
        {
            table ??= BalanceTableProvider.Get();
            var input = new DamageInput(
                attackerAtk,
                (float)monster.Defense,
                critRate,
                critDamage,
                elementMultiplier,
                varianceMultiplier);
            return DamageCalculator.CalculateDamage(input);
        }

        /// <summary>
        /// 현재 최고 층 기준 허용 강화 레벨을 반환한다.
        /// </summary>
        public static int GetAllowedUpgradeLevel(int highestFloorReached, BalanceTable table = null)
        {
            table ??= BalanceTableProvider.Get();
            return BalanceFormulas.GetMaxUpgradeLevel(table, highestFloorReached);
        }

        /// <summary>
        /// 강화 시도가 레벨 상한 규칙을 만족하는지 검사한다.
        /// </summary>
        public static bool CanUpgrade(int targetLevel, int highestFloorReached, BalanceTable table = null)
        {
            table ??= BalanceTableProvider.Get();
            var lmax = BalanceFormulas.GetMaxUpgradeLevel(table, highestFloorReached);
            return targetLevel >= 0 && targetLevel <= lmax;
        }

        /// <summary>
        /// 강화 1회 비용을 반환한다.
        /// </summary>
        public static double GetUpgradeCost(int currentLevel, BalanceTable table = null)
        {
            table ??= BalanceTableProvider.Get();
            return BalanceFormulas.GetUpgradeCost(table, currentLevel);
        }
    }

    /// <summary>
    /// 층 기준 몬스터 스탯·드롭 스냅샷.
    /// </summary>
    public readonly struct MonsterStatSnapshot
    {
        public MonsterStatSnapshot(int floor, double hp, double atk, double def, double goldDrop)
        {
            Floor = floor;
            Hp = hp;
            Attack = atk;
            Defense = def;
            GoldDrop = goldDrop;
        }

        public int Floor { get; }
        public double Hp { get; }
        public double Attack { get; }
        public double Defense { get; }
        public double GoldDrop { get; }
    }
}
