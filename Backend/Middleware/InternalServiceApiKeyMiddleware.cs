using Microsoft.Extensions.Configuration;

namespace Backend.Middleware;

/// <summary>
/// Protects <c>/api/internal/*</c> routes with a shared API key header.
/// </summary>
public sealed class InternalServiceApiKeyMiddleware
{
    public const string ApiKeyHeaderName = "X-Internal-Service-ApiKey";

    private readonly RequestDelegate _next;
    private readonly string? _expectedApiKey;

    public InternalServiceApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _expectedApiKey = configuration["InternalService:ApiKey"];
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api/internal", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        if (string.IsNullOrWhiteSpace(_expectedApiKey))
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(new { message = "Internal service API key is not configured." });
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var provided)
            || !string.Equals(provided.ToString(), _expectedApiKey, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "Invalid or missing internal service API key." });
            return;
        }

        await _next(context);
    }
}
