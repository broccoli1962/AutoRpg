using System;
using UnityEngine;

namespace Backend.Simulation
{
    /// <summary>
    /// 전투 피해 입력 파라미터.
    /// </summary>
    public readonly struct DamageInput
    {
        public DamageInput(
            float attack,
            float defense,
            float critRate,
            float critDamage,
            float elementMultiplier,
            float varianceMultiplier)
        {
            Attack = attack;
            Defense = defense;
            CritRate = critRate;
            CritDamage = critDamage;
            ElementMultiplier = elementMultiplier;
            VarianceMultiplier = varianceMultiplier;
        }

        public float Attack { get; }
        public float Defense { get; }
        public float CritRate { get; }
        public float CritDamage { get; }
        public float ElementMultiplier { get; }
        public float VarianceMultiplier { get; }
    }

    /// <summary>
    /// spec.md 3.1 감쇠식 데미지 공식 계산기.
    /// </summary>
    public static class DamageCalculator
    {
        private const float MIN_DENOMINATOR = 1e-6f;
        private const float MIN_POSITIVE_DAMAGE = 1e-6f;

        /// <summary>
        /// 감쇠식 피해 = ATK × ATK / (ATK + DEF) × 크리 × 속성 × 편차.
        /// </summary>
        public static float CalculateDamage(in DamageInput input)
        {
            var attack = Math.Max(0f, input.Attack);
            var defense = Math.Max(0f, input.Defense);
            var denominator = Math.Max(attack + defense, MIN_DENOMINATOR);
            var baseDamage = attack * attack / denominator;

            var critMultiplier = 1f + Mathf.Clamp01(input.CritRate) * (Mathf.Max(1f, input.CritDamage) - 1f);
            var elementMultiplier = Mathf.Max(0f, input.ElementMultiplier);
            var varianceMultiplier = Mathf.Max(0f, input.VarianceMultiplier);

            var damage = baseDamage * critMultiplier * elementMultiplier * varianceMultiplier;
            if (attack <= 0f)
                return MIN_POSITIVE_DAMAGE;

            return Math.Max(damage, MIN_POSITIVE_DAMAGE);
        }

        /// <summary>
        /// BalanceTable 편차 범위에서 결정론적 편차 배율을 계산한다.
        /// </summary>
        public static float ResolveVarianceMultiplier(BalanceTable table, double normalizedRoll)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var t = Math.Clamp(normalizedRoll, 0d, 1d);
            var min = table.DamageVarianceMin;
            var max = table.DamageVarianceMax;
            return (float)(min + (max - min) * t);
        }

        /// <summary>
        /// 공격 속도를 적용한 DPS를 계산한다.
        /// </summary>
        public static float CalculateDps(in DamageInput input, float attackSpeed)
        {
            var damage = CalculateDamage(input);
            return damage * Math.Max(0f, attackSpeed);
        }
    }
}
