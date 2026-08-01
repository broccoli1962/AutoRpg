using System;
using Backend.Meta.Currency;
using UnityEngine;

namespace Backend.Meta.Attendance
{
    /// <summary>
    /// 신규 7일·월간 28일 출석 보상 테이블.
    /// </summary>
    [CreateAssetMenu(fileName = "AttendanceTable", menuName = "Abyss Chronicle/Attendance Table")]
    public sealed class AttendanceTable : ScriptableObject
    {
        [SerializeField] private AttendanceRewardEntry[] _newPlayerRewards = Array.Empty<AttendanceRewardEntry>();
        [SerializeField] private AttendanceRewardEntry[] _monthlyRewards = Array.Empty<AttendanceRewardEntry>();

        public AttendanceRewardEntry[] NewPlayerRewards => _newPlayerRewards;
        public AttendanceRewardEntry[] MonthlyRewards => _monthlyRewards;

        /// <summary>
        /// 신규 7일 보상을 조회한다.
        /// </summary>
        public AttendanceRewardEntry? FindNewPlayerReward(int dayIndex)
        {
            return FindReward(_newPlayerRewards, dayIndex);
        }

        /// <summary>
        /// 월간 캘린더 보상을 조회한다.
        /// </summary>
        public AttendanceRewardEntry? FindMonthlyReward(int dayIndex)
        {
            return FindReward(_monthlyRewards, dayIndex);
        }

        /// <summary>
        /// spec 기본값으로 직렬화 필드를 채운다.
        /// </summary>
        public void ApplySpecDefaults()
        {
            _newPlayerRewards = new[]
            {
                Reward(1, CurrencyType.Gold, 1_000),
                Reward(2, CurrencyType.Gold, 2_000),
                Reward(3, CurrencyType.ManaShard, 50),
                Reward(4, CurrencyType.Gold, 5_000),
                Reward(5, CurrencyType.SummonTicket, 1),
                Reward(6, CurrencyType.AbyssStone, 50),
                Reward(7, CurrencyType.AbyssStone, 100),
            };

            _monthlyRewards = new AttendanceRewardEntry[28];
            for (var day = 1; day <= 28; day++)
            {
                var isMilestone = day % 7 == 0;
                _monthlyRewards[day - 1] = Reward(
                    day,
                    isMilestone ? CurrencyType.AbyssStone : CurrencyType.Gold,
                    isMilestone ? 20L : 300L * day);
            }
        }

        private static AttendanceRewardEntry? FindReward(AttendanceRewardEntry[] rewards, int dayIndex)
        {
            if (rewards == null)
                return null;

            foreach (var reward in rewards)
            {
                if (reward.DayIndex == dayIndex)
                    return reward;
            }

            return null;
        }

        private static AttendanceRewardEntry Reward(int dayIndex, CurrencyType type, long amount)
        {
            return new AttendanceRewardEntry
            {
                DayIndex = dayIndex,
                CurrencyType = type,
                Amount = amount,
            };
        }
    }
}
