using Cysharp.Threading.Tasks;

namespace Backend.Services.Push
{
    /// <summary>
    /// 푸시 no-op 구현.
    /// </summary>
    public sealed class NullPushNotificationService : IPushNotificationService
    {
        /// <summary>
        /// FCM 토큰.
        /// </summary>
        public string Token => string.Empty;

        /// <summary>
        /// 푸시 서비스를 초기화한다.
        /// </summary>
        public UniTask<bool> InitializeAsync()
        {
            return UniTask.FromResult(true);
        }

        /// <summary>
        /// 토픽을 구독한다.
        /// </summary>
        public UniTask SubscribeTopicAsync(string topic)
        {
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 토픽 구독을 해제한다.
        /// </summary>
        public UniTask UnsubscribeTopicAsync(string topic)
        {
            return UniTask.CompletedTask;
        }
    }
}
