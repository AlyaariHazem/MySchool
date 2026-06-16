namespace MySchool.WebBff.Common.Extensions;

public sealed class OpenApiProxyOptions
{
    public const string SectionName = "OpenApi";

    public Dictionary<string, string> Services { get; set; } = new();
}
