using System.Collections.Generic;
using Backend.Services.RemoteConfig;
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

        [Header("Explorer grade — base stat multiplier (R/SR/SSR/UR)")]
        [SerializeField] private float[] _gradeStatMultipliers = { 1.00f, 1.15f, 1.35f, 1.60f };

        [Header("Explorer grade — duplicate acquisition fragment yield")]
        [SerializeField] private int[] _duplicateFragmentYields = { 10, 20, 50, 100 };

        [Header("Limit break — max stage & per-stage stat bonus (additive)")]
        [SerializeField] private int _maxLimitBreakStage = 5;
        [SerializeField] private float _limitBreakStatBonusPerStage = 0.04f;

        [Header("Limit break — skill level cap (base + bonus per stage)")]
        [SerializeField] private int _baseSkillLevelCap = 10;
        [SerializeField] private int _limitBreakSkillCapBonusPerStage = 2;

        [Header("Limit break — fragment cost per target stage (5 stages × 4 grades, row-major)")]
        [SerializeField] private int[] _limitBreakFragmentCosts =
        {
            20, 30, 40, 50, 60,
            30, 45, 60, 75, 90,
            50, 75, 100, 125, 150,
            80, 120, 160, 200, 240,
        };

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
        public int MaxLimitBreakStage => _maxLimitBreakStage;
        public float LimitBreakStatBonusPerStage => _limitBreakStatBonusPerStage;
        public int BaseSkillLevelCap => _baseSkillLevelCap;
        public int LimitBreakSkillCapBonusPerStage => _limitBreakSkillCapBonusPerStage;

        /// <summary>
        /// 등급별 기본 스탯 배율을 반환한다.
        /// </summary>
        public float GetGradeStatMultiplier(int gradeIndex)
        {
            return GetIndexedValue(_gradeStatMultipliers, gradeIndex, 1f);
        }

        /// <summary>
        /// 중복 획득 시 등급별 조각 전환량을 반환한다.
        /// </summary>
        public int GetDuplicateFragmentYield(int gradeIndex)
        {
            return GetIndexedValue(_duplicateFragmentYields, gradeIndex, 0);
        }

        /// <summary>
        /// 한계돌파 목표 단계(1-based)에 필요한 조각 수를 반환한다.
        /// </summary>
        public int GetLimitBreakFragmentCost(int gradeIndex, int targetStage)
        {
            if (targetStage < 1 || targetStage > _maxLimitBreakStage)
                return 0;

            var index = gradeIndex * _maxLimitBreakStage + (targetStage - 1);
            if (index < 0 || index >= _limitBreakFragmentCosts.Length)
                return 0;

            return _limitBreakFragmentCosts[index];
        }

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
            _gradeStatMultipliers = new[] { 1.00f, 1.15f, 1.35f, 1.60f };
            _duplicateFragmentYields = new[] { 10, 20, 50, 100 };
            _maxLimitBreakStage = 5;
            _limitBreakStatBonusPerStage = 0.04f;
            _baseSkillLevelCap = 10;
            _limitBreakSkillCapBonusPerStage = 2;
            _limitBreakFragmentCosts = new[]
            {
                20, 30, 40, 50, 60,
                30, 45, 60, 75, 90,
                50, 75, 100, 125, 150,
                80, 120, 160, 200, 240,
            };
        }

        private static T GetIndexedValue<T>(T[] values, int index, T fallback)
        {
            if (values == null || index < 0 || index >= values.Length)
                return fallback;

            return values[index];
        }

        /// <summary>
        /// Remote Config 값으로 성장 곡선 계수를 덮어쓴다.
        /// </summary>
        public void ApplyRemoteOverrides(IReadOnlyDictionary<string, string> remoteValues)
        {
            if (remoteValues == null)
                return;

            if (TryParseFloat(remoteValues, RemoteConfigKeys.MonsterHpGrowth, out var hpGrowth))
                _monsterHpGrowth = hpGrowth;
            if (TryParseFloat(remoteValues, RemoteConfigKeys.MonsterAtkGrowth, out var atkGrowth))
                _monsterAtkGrowth = atkGrowth;
            if (TryParseFloat(remoteValues, RemoteConfigKeys.MonsterDefGrowth, out var defGrowth))
                _monsterDefGrowth = defGrowth;
            if (TryParseFloat(remoteValues, RemoteConfigKeys.GoldDropGrowth, out var goldGrowth))
                _goldDropGrowth = goldGrowth;
            if (TryParseFloat(remoteValues, RemoteConfigKeys.UpgradeCostGrowth, out var upgradeGrowth))
                _upgradeCostGrowth = upgradeGrowth;
        }

        private static bool TryParseFloat(
            IReadOnlyDictionary<string, string> remoteValues,
            string key,
            out float value)
        {
            value = 0f;
            if (!remoteValues.TryGetValue(key, out var raw))
                return false;

            return float.TryParse(raw, out value) && value > 0f;
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
