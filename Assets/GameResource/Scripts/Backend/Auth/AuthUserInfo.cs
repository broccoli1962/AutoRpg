using System;

namespace Backend.Services.Auth
{
    /// <summary>
    /// 인증된 사용자 스냅샷.
    /// </summary>
    public sealed class AuthUserInfo
    {
        public string UserId { get; }
        public AuthLinkProvider Provider { get; }
        public bool IsAnonymous { get; }
        public DateTimeOffset SignedInAtUtc { get; }

        public AuthUserInfo(string userId, AuthLinkProvider provider, bool isAnonymous, DateTimeOffset signedInAtUtc)
        {
            UserId = userId ?? string.Empty;
            Provider = provider;
            IsAnonymous = isAnonymous;
            SignedInAtUtc = signedInAtUtc;
        }
    }
}
