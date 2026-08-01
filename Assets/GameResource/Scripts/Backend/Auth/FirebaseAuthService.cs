using System;
using Cysharp.Threading.Tasks;

namespace Backend.Services.Auth
{
    /// <summary>
    /// Firebase Auth 연동 구현체. ABYSS_HAS_FIREBASE 정의 시 실 SDK를 사용한다.
    /// </summary>
    public sealed class FirebaseAuthService : IAuthService
    {
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
        public async UniTask<bool> InitializeAndSignInAnonymouslyAsync()
        {
#if ABYSS_HAS_FIREBASE
            try
            {
                var dependencyStatus = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
                if (dependencyStatus != Firebase.DependencyStatus.Available)
                {
                    IsInitialized = false;
                    return false;
                }

                var auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
                if (auth.CurrentUser == null)
                    await auth.SignInAnonymouslyAsync();

                RefreshCurrentUser(auth);
                IsInitialized = auth.CurrentUser != null;
                return IsInitialized;
            }
            catch (Exception)
            {
                IsInitialized = false;
                return false;
            }
#else
            IsInitialized = false;
            await UniTask.CompletedTask;
            return false;
#endif
        }

        /// <summary>
        /// Google 계정으로 익명 계정을 승격·연동한다.
        /// </summary>
        public async UniTask<bool> LinkGoogleAsync(string idToken)
        {
#if ABYSS_HAS_FIREBASE
            if (!IsInitialized || string.IsNullOrEmpty(idToken))
                return false;

            try
            {
                var auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
                var credential = Firebase.Auth.GoogleAuthProvider.GetCredential(idToken, null);
                if (auth.CurrentUser == null)
                    await auth.SignInWithCredentialAsync(credential);
                else
                    await auth.CurrentUser.LinkWithCredentialAsync(credential);

                RefreshCurrentUser(auth);
                return CurrentUser != null && !CurrentUser.IsAnonymous;
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
        /// Apple 계정으로 익명 계정을 승격·연동한다.
        /// </summary>
        public async UniTask<bool> LinkAppleAsync(string idToken, string rawNonce)
        {
#if ABYSS_HAS_FIREBASE
            if (!IsInitialized || string.IsNullOrEmpty(idToken))
                return false;

            try
            {
                var auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
                var credential = Firebase.Auth.OAuthProvider.GetCredential(
                    "apple.com",
                    idToken,
                    rawNonce,
                    null);

                if (auth.CurrentUser == null)
                    await auth.SignInWithCredentialAsync(credential);
                else
                    await auth.CurrentUser.LinkWithCredentialAsync(credential);

                RefreshCurrentUser(auth);
                return CurrentUser != null && !CurrentUser.IsAnonymous;
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
        /// 로그아웃한다.
        /// </summary>
        public UniTask SignOutAsync()
        {
#if ABYSS_HAS_FIREBASE
            Firebase.Auth.FirebaseAuth.DefaultInstance?.SignOut();
#endif
            CurrentUser = null;
            IsInitialized = false;
            return UniTask.CompletedTask;
        }

#if ABYSS_HAS_FIREBASE
        private void RefreshCurrentUser(Firebase.Auth.FirebaseAuth auth)
        {
            var user = auth?.CurrentUser;
            if (user == null)
            {
                CurrentUser = null;
                return;
            }

            var provider = AuthLinkProvider.Anonymous;
            foreach (var profile in user.ProviderData)
            {
                if (profile.ProviderId == "google.com")
                    provider = AuthLinkProvider.Google;
                else if (profile.ProviderId == "apple.com")
                    provider = AuthLinkProvider.Apple;
            }

            CurrentUser = new AuthUserInfo(
                user.UserId,
                provider,
                user.IsAnonymous,
                DateTimeOffset.UtcNow);
        }
#endif
    }
}
