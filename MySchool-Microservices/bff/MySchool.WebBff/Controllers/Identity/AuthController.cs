using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySchool.Contracts.Auth;
using MySchool.WebBff.Common.DTOs.Identity;
using MySchool.WebBff.Common.Results;
using MySchool.WebBff.GrpcJsonConverters;
using MySchool.WebBff.GrpcServices;
using MySchool.WebBff.Infrastructure.Auth;
using MySchool.WebBff.Infrastructure.Cookies;

namespace MySchool.WebBff.Controllers.Identity;

[ApiController]
[Route("bff/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IIdentityGrpcGateway _identity;
    private readonly IRefreshTokenCookieWriter _cookieWriter;

    public AuthController(IIdentityGrpcGateway identity, IRefreshTokenCookieWriter cookieWriter)
    {
        _identity = identity;
        _cookieWriter = cookieWriter;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto request, CancellationToken cancellationToken)
    {
        var result = await _identity.LoginAsync(request, cancellationToken);
        if (!result.Success)
        {
            return BffResults.BadRequestMessage(
                string.IsNullOrEmpty(result.ErrorMessage) ? "Login failed." : result.ErrorMessage);
        }

        if (!string.IsNullOrEmpty(result.RefreshToken) && result.RefreshTokenExpires is not null)
            _cookieWriter.Write(Response, result.RefreshToken, result.RefreshTokenExpires.ToUtcDateTime());

        return Ok(LoginResponseMapper.ToJson(result));
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto request, CancellationToken cancellationToken)
    {
        var result = await _identity.RegisterAsync(request, cancellationToken);
        if (!result.Success)
            return BffResults.BadRequestMessage(result.ErrorMessage);

        return Ok(new RegisterResponseDto { Message = result.Message });
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken)
    {
        var rawToken = _cookieWriter.Read(Request);
        if (string.IsNullOrWhiteSpace(rawToken))
            return Unauthorized();

        var result = await _identity.RefreshTokenAsync(rawToken, cancellationToken);
        if (!result.Success)
            return Unauthorized();

        if (!string.IsNullOrEmpty(result.RefreshToken) && result.RefreshTokenExpires is not null)
            _cookieWriter.Write(Response, result.RefreshToken, result.RefreshTokenExpires.ToUtcDateTime());

        return Ok(new RefreshTokenResponseDto
        {
            Token = result.Token,
            Expiration = result.Expiration
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _identity.GetCurrentUserAsync(userId, cancellationToken);
        if (!result.Success || result.User is null)
            return BffResults.NotFoundMessage(result.ErrorMessage ?? "User not found.");

        return Ok(MeResponseMapper.ToJson(result));
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        _cookieWriter.Clear(Response);
        return Ok(new { message = "Logged out" });
    }
}
