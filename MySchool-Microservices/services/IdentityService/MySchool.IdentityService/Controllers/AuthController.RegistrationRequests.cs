using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MySchool.IdentityService.Controllers;

public partial class AuthController
{
    private string? GetAuthorizationHeader() =>
        Request.Headers.Authorization.FirstOrDefault();

    [HttpGet("PublicSchools")]
    [AllowAnonymous]
    public Task<IActionResult> PublicSchools(CancellationToken cancellationToken) =>
        ProxyRegistrationAsync(
            () => _monolith.ProxyRegistrationGetAsync(
                "api/internal/registration/public-schools",
                authorizationHeader: null,
                cancellationToken),
            cancellationToken);

    [HttpPost("RequestRegistration")]
    [AllowAnonymous]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(80 * 1024 * 1024)]
    public async Task<IActionResult> RequestRegistration(CancellationToken cancellationToken)
    {
        using var content = new MultipartFormDataContent();
        foreach (var field in Request.Form)
        {
            foreach (var value in field.Value)
                content.Add(new StringContent(value ?? string.Empty), field.Key);
        }

        foreach (var file in Request.Form.Files)
        {
            var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            if (!string.IsNullOrWhiteSpace(file.ContentType))
                fileContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(file.ContentType);
            content.Add(fileContent, file.Name, file.FileName);
        }

        return await ProxyRegistrationAsync(
            () => _monolith.ProxyRegistrationPostAsync(
                "api/internal/registration/request",
                content,
                authorizationHeader: null,
                cancellationToken),
            cancellationToken);
    }

    [HttpPost("PendingRequests")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public Task<IActionResult> PendingRequests(CancellationToken cancellationToken)
    {
        var body = new StreamContent(Request.Body);
        if (!string.IsNullOrWhiteSpace(Request.ContentType))
            body.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(Request.ContentType);

        return ProxyRegistrationAsync(
            () => _monolith.ProxyRegistrationPostAsync(
                "api/internal/registration/pending-requests",
                body,
                GetAuthorizationHeader(),
                cancellationToken),
            cancellationToken);
    }

    [HttpPost("ApproveRequest/{id:int}")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public Task<IActionResult> ApproveRequest(int id, CancellationToken cancellationToken)
    {
        var body = new StreamContent(Request.Body);
        if (!string.IsNullOrWhiteSpace(Request.ContentType))
            body.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(Request.ContentType);

        return ProxyRegistrationAsync(
            () => _monolith.ProxyRegistrationPostAsync(
                $"api/internal/registration/approve/{id}",
                body,
                GetAuthorizationHeader(),
                cancellationToken),
            cancellationToken);
    }

    [HttpPost("RejectRequest/{id:int}")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public Task<IActionResult> RejectRequest(int id, CancellationToken cancellationToken)
    {
        var body = new StreamContent(Request.Body);
        if (!string.IsNullOrWhiteSpace(Request.ContentType))
            body.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(Request.ContentType);

        return ProxyRegistrationAsync(
            () => _monolith.ProxyRegistrationPostAsync(
                $"api/internal/registration/reject/{id}",
                body,
                GetAuthorizationHeader(),
                cancellationToken),
            cancellationToken);
    }

    private static async Task<IActionResult> ProxyRegistrationAsync(
        Func<Task<HttpResponseMessage>> send,
        CancellationToken cancellationToken)
    {
        using var response = await send();
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";
        return new ContentResult
        {
            StatusCode = (int)response.StatusCode,
            Content = content,
            ContentType = contentType
        };
    }
}
