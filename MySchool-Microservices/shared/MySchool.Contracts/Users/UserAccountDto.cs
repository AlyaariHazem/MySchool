namespace MySchool.Contracts.Users;

/// <summary>Serializable user account matching monolith ApplicationUser POCO fields.</summary>
public class UserAccountDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? UserName { get; set; }
    public string? NormalizedUserName { get; set; }
    public string? Email { get; set; }
    public string? NormalizedEmail { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? PasswordHash { get; set; }
    public string? PhoneNumber { get; set; }
    public string? SecurityStamp { get; set; }
    public string? ConcurrencyStamp { get; set; }
    public string? Address { get; set; }
    public string? Gender { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? PhoneNumberNormalized { get; set; }
    public DateTime HireDate { get; set; } = DateTime.Now;
    public string UserType { get; set; } = string.Empty;
}
