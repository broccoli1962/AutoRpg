using System;

namespace Backend.GameSystems.Offline
{
    /// <summary>
    /// 서버 UTC 시각 제공자. BaaS 연동 전에는 스텁·로컬 폴백을 사용한다.
    /// </summary>
    public interface IServerTimeProvider
    {
        /// <summary>
        /// 서버 UTC 시각을 반환한다. 실패 시 false.
        /// </summary>
        bool TryGetServerTimeUtc(out DateTimeOffset serverTimeUtc);
    }
}
