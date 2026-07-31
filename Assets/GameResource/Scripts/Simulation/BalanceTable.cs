using UnityEngine;

namespace Backend.Simulation
{
    /// <summary>
    /// spec.md 3.2 성장 곡선·구역 배율 계수를 담는 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "BalanceTable", menuName = "Abyss Chronicle/Balance Table")]
    public sealed class BalanceTable : ScriptableObject
    {
        [Header("Monster HP — base × growth^(n-1) × ZoneMul")]
        [SerializeField] private float _monsterHpBase = 100f;
        [SerializeField] private float _monsterHpGrowth = 1.135f;

        [Header("Monster ATK — base × growth^(n-1) × ZoneMul")]
        [SerializeField] private float _monsterAtkBase = 10f;
        [SerializeField] private float _monsterAtkGrowth = 1.130f;

        [Header("Monster DEF — base × growth^(n-1) × ZoneMul")]
        [SerializeField] private float _monsterDefBase = 5f;
        [SerializeField] private float _monsterDefGrowth = 1.130f;

        [Header("Gold Drop — base × growth^(n-1)")]
        [SerializeField] private float _goldDropBase = 10f;
        [SerializeField] private float _goldDropGrowth = 1.120f;

        [Header("Equipment Upgrade — base × growth^L")]
        [SerializeField] private float _upgradeCostBase = 50f;
        [SerializeField] private float _upgradeCostGrowth = 1.080f;
        [SerializeField] private float _upgradeLevelCapMultiplier = 1.5f;

        [Header("Character EXP — base × growth^(L-1)")]
        [SerializeField] private float _expRequiredBase = 100f;
        [SerializeField] private float _expRequiredGrowth = 1.095f;

        [Header("Zone — floors per zone & multipliers (Zone 1~8)")]
        [SerializeField] private int _floorsPerZone = 25;
        [SerializeField] private float[] _zoneMultipliers =
        {
            1.00f, 1.10f, 1.25f, 1.45f, 1.70f, 2.00f, 2.35f, 2.75f
        };

        [Header("Zone 9+ infinite scaling")]
        [SerializeField] private float _infiniteZoneBaseMultiplier = 2.75f;
        [SerializeField] private float _infiniteZoneGrowth = 1.15f;
        [SerializeField] private int _infiniteZoneStart = 9;

        [Header("Damage variance")]
        [SerializeField] private float _damageVarianceMin = 0.9f;
        [SerializeField] private float _damageVarianceMax = 1.1f;

        public float MonsterHpBase => _monsterHpBase;
        public float MonsterHpGrowth => _monsterHpGrowth;
        public float MonsterAtkBase => _monsterAtkBase;
        public float MonsterAtkGrowth => _monsterAtkGrowth;
        public float MonsterDefBase => _monsterDefBase;
        public float MonsterDefGrowth => _monsterDefGrowth;
        public float GoldDropBase => _goldDropBase;
        public float GoldDropGrowth => _goldDropGrowth;
        public float UpgradeCostBase => _upgradeCostBase;
        public float UpgradeCostGrowth => _upgradeCostGrowth;
        public float UpgradeLevelCapMultiplier => _upgradeLevelCapMultiplier;
        public float ExpRequiredBase => _expRequiredBase;
        public float ExpRequiredGrowth => _expRequiredGrowth;
        public int FloorsPerZone => _floorsPerZone;
        public float InfiniteZoneBaseMultiplier => _infiniteZoneBaseMultiplier;
        public float InfiniteZoneGrowth => _infiniteZoneGrowth;
        public int InfiniteZoneStart => _infiniteZoneStart;
        public float DamageVarianceMin => _damageVarianceMin;
        public float DamageVarianceMax => _damageVarianceMax;

        /// <summary>
        /// spec.md 3.2 기본값으로 직렬화 필드를 채운다.
        /// </summary>
        public void ApplySpecDefaults()
        {
            _monsterHpBase = 100f;
            _monsterHpGrowth = 1.135f;
            _monsterAtkBase = 10f;
            _monsterAtkGrowth = 1.130f;
            _monsterDefBase = 5f;
            _monsterDefGrowth = 1.130f;
            _goldDropBase = 10f;
            _goldDropGrowth = 1.120f;
            _upgradeCostBase = 50f;
            _upgradeCostGrowth = 1.080f;
            _upgradeLevelCapMultiplier = 1.5f;
            _expRequiredBase = 100f;
            _expRequiredGrowth = 1.095f;
            _floorsPerZone = 25;
            _zoneMultipliers = new[]
            {
                1.00f, 1.10f, 1.25f, 1.45f, 1.70f, 2.00f, 2.35f, 2.75f
            };
            _infiniteZoneBaseMultiplier = 2.75f;
            _infiniteZoneGrowth = 1.15f;
            _infiniteZoneStart = 9;
            _damageVarianceMin = 0.9f;
            _damageVarianceMax = 1.1f;
        }

        /// <summary>
        /// 구역 번호(1-based)에 해당하는 ZoneMul 배율을 반환한다.
        /// </summary>
        public float GetZoneMultiplier(int zone)
        {
            if (zone < 1)
                return _zoneMultipliers.Length > 0 ? _zoneMultipliers[0] : 1f;

            if (zone < _infiniteZoneStart)
            {
                var index = zone - 1;
                if (index >= 0 && index < _zoneMultipliers.Length)
                    return _zoneMultipliers[index];

                return _zoneMultipliers.Length > 0
                    ? _zoneMultipliers[_zoneMultipliers.Length - 1]
                    : 1f;
            }

            var exponent = zone - 8;
            return _infiniteZoneBaseMultiplier * Mathf.Pow(_infiniteZoneGrowth, exponent);
        }
    }
}
