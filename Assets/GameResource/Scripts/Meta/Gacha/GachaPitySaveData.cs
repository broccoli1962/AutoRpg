using System;

namespace Backend.Meta.Gacha
{
    /// <summary>
    /// 천장 카운터 세이브 스냅샷.
    /// </summary>
    [Serializable]
    public sealed class GachaPitySaveData
    {
        public int SsrCounter;
        public int UrCounter;
    }
}
