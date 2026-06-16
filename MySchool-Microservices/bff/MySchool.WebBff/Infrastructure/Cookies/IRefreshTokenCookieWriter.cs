namespace MySchool.WebBff.Infrastructure.Cookies;

public interface IRefreshTokenCookieWriter
{
    void Write(HttpResponse response, string rawToken, DateTime expiresUtc);
    void Clear(HttpResponse response);
    string? Read(HttpRequest request);
}
