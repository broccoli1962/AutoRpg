namespace Backend.Meta.SeasonPass
{
    /// <summary>
    /// 프리미엄 트랙 해금 상태를 서버에 영속화한다.
    /// </summary>
    public interface ISeasonPassPremiumSync
    {
        /// <summary>
        /// 프리미엄 해금 상태를 서버에 저장한다.
        /// </summary>
        void PersistPremiumUnlocked(int seasonNumber, bool isPremiumUnlocked);

        /// <summary>
        /// 서버에서 프리미엄 해금 상태를 복원한다.
        /// </summary>
        bool TryRestorePremiumUnlocked(int seasonNumber, out bool isPremiumUnlocked);
    }

    /// <summary>
    /// 개발용 no-op 프리미엄 동기화 구현.
    /// </summary>
    public sealed class NullSeasonPassPremiumSync : ISeasonPassPremiumSync
    {
        /// <summary>
        /// 저장 요청을 무시한다.
        /// </summary>
        public void PersistPremiumUnlocked(int seasonNumber, bool isPremiumUnlocked)
        {
        }

        /// <summary>
        /// 서버 복원을 사용하지 않는다.
        /// </summary>
        public bool TryRestorePremiumUnlocked(int seasonNumber, out bool isPremiumUnlocked)
        {
            isPremiumUnlocked = false;
            return false;
        }
    }
}
