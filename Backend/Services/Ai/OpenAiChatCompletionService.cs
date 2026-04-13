using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Backend.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Backend.Services.Ai;

/// <summary>
/// HTTP client for OpenAI Chat Completions — API key stays server-side (Bearer token).
/// </summary>
public sealed class OpenAiChatCompletionService
{
    private readonly HttpClient _http;
    private readonly OpenAiOptions _options;
    private readonly ILogger<OpenAiChatCompletionService> _logger;

    public OpenAiChatCompletionService(
        HttpClient http,
        IOptions<OpenAiOptions> options,
        ILogger<OpenAiChatCompletionService> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> SendChatCompletionRawAsync(string jsonBody, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("OpenAI API key is not configured (OpenAI:ApiKey).");

        var baseUrl = string.IsNullOrWhiteSpace(_options.BaseUrl) ? "https://api.openai.com/v1/" : _options.BaseUrl;
        var url = baseUrl.TrimEnd('/') + "/chat/completions";

        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);
        req.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        _logger.LogInformation("OpenAI chat/completions POST {Url} (model {Model})", url, _options.Model);

        var resp = await _http.SendAsync(req, cancellationToken);
        var text = await resp.Content.ReadAsStringAsync(cancellationToken);

        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("OpenAI HTTP {Status}: {Body}", (int)resp.StatusCode, text);
            throw new InvalidOperationException($"OpenAI request failed ({(int)resp.StatusCode}).");
        }

        return text;
    }
}
