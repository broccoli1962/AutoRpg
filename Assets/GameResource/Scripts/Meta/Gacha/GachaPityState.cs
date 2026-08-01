using System;

namespace Backend.Meta.Gacha
{
    /// <summary>
    /// SSR·UR 천장 카운터. 배너와 무관하게 이월되며 세이브에 영속화된다.
    /// </summary>
    [Serializable]
    public sealed class GachaPityState
    {
        public int SsrCounter;
        public int UrCounter;

        /// <summary>
        /// 현재 SSR 천장 카운터를 반환한다.
        /// </summary>
        public int GetSsrCounter() => SsrCounter;

        /// <summary>
        /// 현재 UR 천장 카운터를 반환한다.
        /// </summary>
        public int GetUrCounter() => UrCounter;

        /// <summary>
        /// 소환 1회분 카운터를 증가시킨다.
        /// </summary>
        public void IncrementCounters()
        {
            SsrCounter++;
            UrCounter++;
        }

        /// <summary>
        /// SSR 이상 획득 시 SSR 카운터를 리셋한다.
        /// </summary>
        public void ResetSsrCounter()
        {
            SsrCounter = 0;
        }

        /// <summary>
        /// UR 획득 시 UR 카운터를 리셋한다.
        /// </summary>
        public void ResetUrCounter()
        {
            UrCounter = 0;
        }

        /// <summary>
        /// 세이브용 스냅샷을 생성한다.
        /// </summary>
        public GachaPitySaveData ToSaveData()
        {
            return new GachaPitySaveData
            {
                SsrCounter = SsrCounter,
                UrCounter = UrCounter,
            };
        }

        /// <summary>
        /// 세이브 스냅샷에서 천장 상태를 복원한다.
        /// </summary>
        public static GachaPityState FromSaveData(GachaPitySaveData saveData)
        {
            if (saveData == null)
                return new GachaPityState();

            return new GachaPityState
            {
                SsrCounter = Math.Max(0, saveData.SsrCounter),
                UrCounter = Math.Max(0, saveData.UrCounter),
            };
        }
    }
}
