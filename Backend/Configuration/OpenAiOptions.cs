namespace Backend.Configuration;

/// <summary>
/// OpenAI API settings (secrets only via User Secrets or environment — never commit keys; never expose to Angular).
/// </summary>
public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    /// <summary>API key (e.g. sk-...).</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Chat completion model id (e.g. gpt-4o-mini).</summary>
    public string Model { get; set; } = "gpt-4o-mini";

    /// <summary>Base URL for OpenAI-compatible APIs (default: official OpenAI).</summary>
    public string BaseUrl { get; set; } = "https://api.openai.com/v1/";

    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>Max assistant↔tool loop iterations (safety).</summary>
    public int MaxToolIterations { get; set; } = 8;
}
