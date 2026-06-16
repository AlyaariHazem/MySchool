namespace MySchool.Contracts.Auth;

public class LoginDto
{
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string userType { get; set; } = default!;

    /// <summary>Optional when the user belongs to multiple schools; picks the tenant for the JWT.</summary>
    public int? TenantId { get; set; }
}
