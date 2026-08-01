using System;
using Cysharp.Threading.Tasks;

namespace Backend.Services.Push
{
    /// <summary>
    /// Firebase Cloud Messaging 구현체. ABYSS_HAS_FIREBASE 정의 시 실 SDK를 사용한다.
    /// </summary>
    public sealed class FirebaseCloudMessagingService : IPushNotificationService
    {
        /// <summary>
        /// FCM 토큰.
        /// </summary>
        public string Token { get; private set; } = string.Empty;

        /// <summary>
        /// 푸시 서비스를 초기화한다.
        /// </summary>
        public async UniTask<bool> InitializeAsync()
        {
#if ABYSS_HAS_FIREBASE
            try
            {
                Token = await Firebase.Messaging.FirebaseMessaging.GetTokenAsync();
                Firebase.Messaging.FirebaseMessaging.TokenReceived += (_, e) => Token = e.Token;
                return !string.IsNullOrEmpty(Token);
            }
            catch (Exception)
            {
                return false;
            }
#else
            await UniTask.CompletedTask;
            return false;
#endif
        }

        /// <summary>
        /// 토픽을 구독한다.
        /// </summary>
        public async UniTask SubscribeTopicAsync(string topic)
        {
#if ABYSS_HAS_FIREBASE
            if (string.IsNullOrEmpty(topic))
                return;

            try
            {
                await Firebase.Messaging.FirebaseMessaging.SubscribeAsync(topic);
            }
            catch (Exception)
            {
            }
#else
            await UniTask.CompletedTask;
#endif
        }

        /// <summary>
        /// 토픽 구독을 해제한다.
        /// </summary>
        public async UniTask UnsubscribeTopicAsync(string topic)
        {
#if ABYSS_HAS_FIREBASE
            if (string.IsNullOrEmpty(topic))
                return;

            try
            {
                await Firebase.Messaging.FirebaseMessaging.UnsubscribeAsync(topic);
            }
            catch (Exception)
            {
            }
#else
            await UniTask.CompletedTask;
#endif
        }
    }
}
