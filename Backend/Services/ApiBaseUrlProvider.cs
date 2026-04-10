using Backend.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Backend.Services;

public sealed class ApiBaseUrlProvider : IApiBaseUrlProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public ApiBaseUrlProvider(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public string GetOrigin()
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request != null)
            return $"{request.Scheme}://{request.Host.Value}".TrimEnd('/');

        var configured = _configuration["PublicApi:BaseUrl"]?.Trim().TrimEnd('/');
        return configured ?? string.Empty;
    }

    public string UploadsFile(string relativePathUnderUploads)
    {
        if (string.IsNullOrWhiteSpace(relativePathUnderUploads))
            return string.Empty;

        var path = relativePathUnderUploads.Trim().TrimStart('/');
        var origin = GetOrigin();
        var relativeUrl = $"/uploads/{path}";
        if (string.IsNullOrEmpty(origin))
            return relativeUrl;
        return $"{origin}{relativeUrl}";
    }

    public string UploadsFolder(string folderName)
    {
        if (string.IsNullOrWhiteSpace(folderName))
            return string.Empty;

        var f = folderName.Trim().TrimStart('/');
        var origin = GetOrigin();
        var relativeUrl = $"/uploads/{f}";
        if (string.IsNullOrEmpty(origin))
            return relativeUrl;
        return $"{origin}{relativeUrl}";
    }
}
