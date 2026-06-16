namespace MySchool.Gateway.OpenApi;

public sealed class OpenApiProxyOptions
{
    public const string SectionName = "OpenApi";

    /// <summary>Service name (e.g. identity, monolith) → base URL of that service.</summary>
    public Dictionary<string, string> Services { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
