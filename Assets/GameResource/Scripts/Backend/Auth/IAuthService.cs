using Cysharp.Threading.Tasks;

namespace Backend.Services.Auth
{
    /// <summary>
    /// Firebase Auth 추상화. 오프라인·SDK 미설치 시에도 게임이 동작해야 한다.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// 인증 초기화 완료 여부.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 현재 로그인 사용자. 미로그인 시 null.
        /// </summary>
        AuthUserInfo CurrentUser { get; }

        /// <summary>
        /// 인증 SDK를 초기화하고 익명 로그인을 시도한다.
        /// </summary>
        UniTask<bool> InitializeAndSignInAnonymouslyAsync();

        /// <summary>
        /// Google 계정으로 익명 계정을 승격·연동한다.
        /// </summary>
        UniTask<bool> LinkGoogleAsync(string idToken);

        /// <summary>
        /// Apple 계정으로 익명 계정을 승격·연동한다.
        /// </summary>
        UniTask<bool> LinkAppleAsync(string idToken, string rawNonce);

        /// <summary>
        /// 로그아웃한다.
        /// </summary>
        UniTask SignOutAsync();
    }
}
