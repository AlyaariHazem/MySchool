using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using MySchool.Contracts.Auth;
using MySchool.Contracts.Internal;
using MySchool.IdentityService.Interfaces;
using MySchool.IdentityService.Middleware;

namespace MySchool.IdentityService.Services;

public sealed class MonolithIntegrationClient : IMonolithIntegrationClient
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public MonolithIntegrationClient(HttpClient http, IConfiguration configuration)
    {
        _http = http;
        var baseUrl = configuration["MonolithService:BaseUrl"]
            ?? throw new InvalidOperationException("MonolithService:BaseUrl is not configured.");
        _http.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        _apiKey = configuration["InternalService:ApiKey"]
            ?? throw new InvalidOperationException("InternalService:ApiKey is not configured.");
    }

    public async Task<LoginEnrichmentResponseDto> GetLoginEnrichmentAsync(
        string userId,
        string userType,
        int? requestedTenantId = null,
        CancellationToken cancellationToken = default)
    {
        var response = await SendJsonAsync(
            HttpMethod.Post,
            "api/internal/auth/login-enrichment",
            new LoginEnrichmentRequestDto
            {
                UserId = userId,
                UserType = userType,
                RequestedTenantId = requestedTenantId
            },
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LoginEnrichmentResponseDto>(JsonOptions, cancellationToken)
            ?? new LoginEnrichmentResponseDto();
    }

    public async Task<IReadOnlyList<UserTenantSummaryDto>> GetTenantSummariesAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var response = await SendJsonAsync(
            HttpMethod.Get,
            $"api/internal/auth/tenant-summaries/{Uri.EscapeDataString(userId)}",
            null,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<UserTenantSummaryDto>>(JsonOptions, cancellationToken)
            ?? new List<UserTenantSummaryDto>();
    }

    public async Task<UserTenantMembershipDto?> GetMembershipAsync(
        string userId,
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        var response = await SendJsonAsync(
            HttpMethod.Get,
            $"api/internal/auth/membership/{Uri.EscapeDataString(userId)}/{tenantId}",
            null,
            cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserTenantMembershipDto>(JsonOptions, cancellationToken);
    }

    public async Task TouchTenantAccessAsync(string userId, int tenantId, CancellationToken cancellationToken = default)
    {
        var response = await SendJsonAsync(
            HttpMethod.Post,
            "api/internal/auth/touch-tenant-access",
            new TouchTenantAccessRequestDto { UserId = userId, TenantId = tenantId },
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task EnsureUserTenantAsync(
        string userId,
        int tenantId,
        TenantRole tenantRole,
        CancellationToken cancellationToken = default)
    {
        var response = await SendJsonAsync(
            HttpMethod.Post,
            "api/internal/auth/ensure-user-tenant",
            new EnsureUserTenantRequestDto { UserId = userId, TenantId = tenantId, TenantRole = tenantRole },
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<string?> ResolveSchoolRoleKeyAsync(
        string userId,
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        var response = await SendJsonAsync(
            HttpMethod.Get,
            $"api/internal/auth/school-role/{Uri.EscapeDataString(userId)}/{tenantId}",
            null,
            cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<SchoolRoleResponseDto>(JsonOptions, cancellationToken);
        return payload?.SchoolRole;
    }

    public Task<HttpResponseMessage> ProxyRegistrationGetAsync(
        string relativePath,
        string? authorizationHeader,
        CancellationToken cancellationToken = default) =>
        SendProxyAsync(HttpMethod.Get, relativePath, null, authorizationHeader, cancellationToken);

    public Task<HttpResponseMessage> ProxyRegistrationPostAsync(
        string relativePath,
        HttpContent? content,
        string? authorizationHeader,
        CancellationToken cancellationToken = default) =>
        SendProxyAsync(HttpMethod.Post, relativePath, content, authorizationHeader, cancellationToken);

    private Task<HttpResponseMessage> SendJsonAsync(
        HttpMethod method,
        string relativeUrl,
        object? body,
        CancellationToken cancellationToken)
    {
        var request = BuildRequest(method, relativeUrl, apiKeyOnly: true, authorizationHeader: null);
        if (body != null)
            request.Content = JsonContent.Create(body, options: JsonOptions);
        return _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }

    private Task<HttpResponseMessage> SendProxyAsync(
        HttpMethod method,
        string relativeUrl,
        HttpContent? content,
        string? authorizationHeader,
        CancellationToken cancellationToken)
    {
        var request = BuildRequest(method, relativeUrl, apiKeyOnly: true, authorizationHeader);
        request.Content = content;
        return _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }

    private HttpRequestMessage BuildRequest(
        HttpMethod method,
        string relativeUrl,
        bool apiKeyOnly,
        string? authorizationHeader)
    {
        var request = new HttpRequestMessage(method, relativeUrl);
        request.Headers.TryAddWithoutValidation(InternalServiceApiKeyMiddleware.ApiKeyHeaderName, _apiKey);
        if (!string.IsNullOrWhiteSpace(authorizationHeader))
            request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
        return request;
    }
}
