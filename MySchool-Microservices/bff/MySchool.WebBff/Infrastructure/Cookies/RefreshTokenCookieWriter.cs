namespace MySchool.WebBff.Infrastructure.Cookies;

public sealed class RefreshTokenCookieWriter : IRefreshTokenCookieWriter
{
    public const string CookieName = "refreshToken";

    public void Write(HttpResponse response, string rawToken, DateTime expiresUtc)
    {
        response.Cookies.Append(CookieName, rawToken, BuildCookieOptions(response, expiresUtc));
    }

    public void Clear(HttpResponse response)
    {
        response.Cookies.Delete(CookieName, BuildCookieOptions(response, DateTime.UnixEpoch));
    }

    public string? Read(HttpRequest request) => request.Cookies[CookieName];

    private static CookieOptions BuildCookieOptions(HttpResponse response, DateTime expires) => new()
    {
        HttpOnly = true,
        Secure = response.HttpContext.Request.IsHttps,
        SameSite = SameSiteMode.None,
        Expires = expires
    };
}
