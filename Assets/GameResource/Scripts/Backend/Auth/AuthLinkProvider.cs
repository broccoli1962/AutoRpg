namespace Backend.Services.Auth
{
    /// <summary>
    /// 계정 연동 제공자 종류.
    /// </summary>
    public enum AuthLinkProvider
    {
        Anonymous = 0,
        Google = 1,
        Apple = 2,
    }
}
