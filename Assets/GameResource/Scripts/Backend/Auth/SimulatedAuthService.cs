using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Backend.Services.Auth
{
    /// <summary>
    /// EditMode·오프라인용 인증 스텁. 로컬 GUID 기반 익명 사용자를 생성한다.
    /// </summary>
    public sealed class SimulatedAuthService : IAuthService
    {
        private const string PREF_USER_ID = "abyss_sim_auth_user_id";
        private const string PREF_PROVIDER = "abyss_sim_auth_provider";
        private const string PREF_IS_ANONYMOUS = "abyss_sim_auth_is_anonymous";

        /// <summary>
        /// 인증 초기화 완료 여부.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// 현재 로그인 사용자.
        /// </summary>
        public AuthUserInfo CurrentUser { get; private set; }

        /// <summary>
        /// 인증 SDK를 초기화하고 익명 로그인을 시도한다.
        /// </summary>
        public UniTask<bool> InitializeAndSignInAnonymouslyAsync()
        {
            IsInitialized = true;

            var userId = PlayerPrefs.GetString(PREF_USER_ID, string.Empty);
            if (string.IsNullOrEmpty(userId))
            {
                userId = Guid.NewGuid().ToString("N");
                PlayerPrefs.SetString(PREF_USER_ID, userId);
                PlayerPrefs.SetInt(PREF_PROVIDER, (int)AuthLinkProvider.Anonymous);
                PlayerPrefs.SetInt(PREF_IS_ANONYMOUS, 1);
                PlayerPrefs.Save();
            }

            var provider = (AuthLinkProvider)PlayerPrefs.GetInt(PREF_PROVIDER, (int)AuthLinkProvider.Anonymous);
            var isAnonymous = PlayerPrefs.GetInt(PREF_IS_ANONYMOUS, 1) == 1;
            CurrentUser = new AuthUserInfo(userId, provider, isAnonymous, DateTimeOffset.UtcNow);
            return UniTask.FromResult(true);
        }

        /// <summary>
        /// Google 계정으로 익명 계정을 승격·연동한다.
        /// </summary>
        public UniTask<bool> LinkGoogleAsync(string idToken)
        {
            if (!IsInitialized || CurrentUser == null || string.IsNullOrEmpty(idToken))
                return UniTask.FromResult(false);

            PromoteLinkedAccount(AuthLinkProvider.Google);
            return UniTask.FromResult(true);
        }

        /// <summary>
        /// Apple 계정으로 익명 계정을 승격·연동한다.
        /// </summary>
        public UniTask<bool> LinkAppleAsync(string idToken, string rawNonce)
        {
            if (!IsInitialized || CurrentUser == null || string.IsNullOrEmpty(idToken))
                return UniTask.FromResult(false);

            PromoteLinkedAccount(AuthLinkProvider.Apple);
            return UniTask.FromResult(true);
        }

        /// <summary>
        /// 로그아웃한다.
        /// </summary>
        public UniTask SignOutAsync()
        {
            CurrentUser = null;
            PlayerPrefs.DeleteKey(PREF_USER_ID);
            PlayerPrefs.DeleteKey(PREF_PROVIDER);
            PlayerPrefs.DeleteKey(PREF_IS_ANONYMOUS);
            PlayerPrefs.Save();
            return UniTask.CompletedTask;
        }

        private void PromoteLinkedAccount(AuthLinkProvider provider)
        {
            PlayerPrefs.SetInt(PREF_PROVIDER, (int)provider);
            PlayerPrefs.SetInt(PREF_IS_ANONYMOUS, 0);
            PlayerPrefs.Save();

            CurrentUser = new AuthUserInfo(
                CurrentUser.UserId,
                provider,
                false,
                DateTimeOffset.UtcNow);
        }
    }
}
