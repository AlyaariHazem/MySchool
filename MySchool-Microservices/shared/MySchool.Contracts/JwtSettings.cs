namespace MySchool.Contracts;

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string AudienceIP { get; set; } = string.Empty;
    public string IssuerIP { get; set; } = string.Empty;
}
