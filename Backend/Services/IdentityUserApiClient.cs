using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Backend.Models;
using Backend.Repository.School.Implements;
using Microsoft.Extensions.Configuration;

namespace Backend.Services;

public sealed class IdentityUserApiClient : IUserRepository
{
    public const string ApiKeyHeaderName = Middleware.InternalServiceApiKeyMiddleware.ApiKeyHeaderName;

    private readonly HttpClient _http;
    private readonly string _apiKey;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public IdentityUserApiClient(HttpClient http, IConfiguration configuration)
    {
        _http = http;
        var baseUrl = configuration["IdentityService:BaseUrl"]
            ?? throw new InvalidOperationException("IdentityService:BaseUrl is not configured.");
        _http.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        _apiKey = configuration["InternalService:ApiKey"]
            ?? throw new InvalidOperationException("InternalService:ApiKey is not configured.");
    }

    public async Task<ApplicationUser> CreateUserAsync(ApplicationUser user, string password, string role)
    {
        var created = await SendAsync<ApplicationUser>(HttpMethod.Post, "api/users", new CreateUserApiRequest
        {
            User = user,
            Password = password,
            Role = role
        });
        return created ?? throw new InvalidOperationException("Identity service returned an empty user.");
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
    {
        var response = await SendRequestAsync(HttpMethod.Get, $"api/users/{Uri.EscapeDataString(userId)}");
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ApplicationUser>(JsonOptions);
    }

    public async Task<ApplicationUser?> GetUserByIdOrNameAsync(string userIdOrName)
    {
        var response = await SendRequestAsync(
            HttpMethod.Get,
            $"api/users/by-id-or-name/{Uri.EscapeDataString(userIdOrName)}");
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ApplicationUser>(JsonOptions);
    }

    public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
    {
        var response = await SendRequestAsync(HttpMethod.Get, "api/users");
        response.EnsureSuccessStatusCode();
        var list = await response.Content.ReadFromJsonAsync<List<ApplicationUser>>(JsonOptions);
        return list ?? new List<ApplicationUser>();
    }

    public Task AddAsync(ApplicationUser user) =>
        SendAsync(HttpMethod.Post, "api/users/raw", user);

    public Task UpdateAsync(ApplicationUser user) =>
        SendAsync(HttpMethod.Put, $"api/users/{Uri.EscapeDataString(user.Id)}", user);

    public Task DeleteAsync(string userId) =>
        SendAsync(HttpMethod.Delete, $"api/users/{Uri.EscapeDataString(userId)}");

    public async Task<bool> ExistsByNormalizedUserNameOrEmailAsync(string normalizedUserName, string normalizedEmail)
    {
        var query =
            $"api/users/exists?normalizedUserName={Uri.EscapeDataString(normalizedUserName)}&normalizedEmail={Uri.EscapeDataString(normalizedEmail)}";
        var response = await SendRequestAsync(HttpMethod.Get, query);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ExistsResponse>(JsonOptions);
        return payload?.Exists == true;
    }

    public async Task<bool> ExistsByNormalizedPhoneAsync(string normalizedPhone)
    {
        var query = $"api/users/exists-phone?normalizedPhone={Uri.EscapeDataString(normalizedPhone)}";
        var response = await SendRequestAsync(HttpMethod.Get, query);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ExistsResponse>(JsonOptions);
        return payload?.Exists == true;
    }

    public async Task<ApplicationUser> CreateApprovedRegistrationUserAsync(
        ApplicationUser user,
        string passwordHash,
        string role)
    {
        var created = await SendAsync<ApplicationUser>(HttpMethod.Post, "api/users/approved-registration", new ApprovedRegistrationUserRequest
        {
            User = user,
            PasswordHash = passwordHash,
            Role = role
        });
        return created ?? throw new InvalidOperationException("Identity service returned an empty user.");
    }

    private async Task<T?> SendAsync<T>(HttpMethod method, string relativeUrl, object? body = null)
    {
        var response = await SendRequestAsync(method, relativeUrl, body);
        response.EnsureSuccessStatusCode();
        if (response.StatusCode == HttpStatusCode.NoContent)
            return default;
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    private async Task SendAsync(HttpMethod method, string relativeUrl, object? body = null)
    {
        var response = await SendRequestAsync(method, relativeUrl, body);
        response.EnsureSuccessStatusCode();
    }

    private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string relativeUrl, object? body = null)
    {
        using var request = new HttpRequestMessage(method, relativeUrl);
        request.Headers.TryAddWithoutValidation(ApiKeyHeaderName, _apiKey);
        if (body != null)
            request.Content = JsonContent.Create(body, options: JsonOptions);
        return await _http.SendAsync(request);
    }

    private sealed class CreateUserApiRequest
    {
        public ApplicationUser User { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string Role { get; set; } = default!;
    }

    private sealed class ApprovedRegistrationUserRequest
    {
        public ApplicationUser User { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public string Role { get; set; } = default!;
    }

    private sealed class ExistsResponse
    {
        public bool Exists { get; set; }
    }
}
