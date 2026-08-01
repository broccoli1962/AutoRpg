using System;
using Cysharp.Threading.Tasks;

namespace Backend.Services.Push
{
    /// <summary>
    /// Cloud Messaging(푸시) 추상화.
    /// </summary>
    public interface IPushNotificationService
    {
        /// <summary>
        /// FCM 토큰.
        /// </summary>
        string Token { get; }

        /// <summary>
        /// 푸시 서비스를 초기화한다.
        /// </summary>
        UniTask<bool> InitializeAsync();

        /// <summary>
        /// 토픽을 구독한다.
        /// </summary>
        UniTask SubscribeTopicAsync(string topic);

        /// <summary>
        /// 토픽 구독을 해제한다.
        /// </summary>
        UniTask UnsubscribeTopicAsync(string topic);
    }
}
