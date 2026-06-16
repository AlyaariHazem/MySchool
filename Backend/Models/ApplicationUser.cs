namespace Backend.Models;

public class ApplicationUser
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

    /// <summary>Date of birth when captured from registration or school records.</summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>Digits-only phone key for uniqueness (public registration).</summary>
    public string? PhoneNumberNormalized { get; set; }

    public DateTime HireDate { get; set; } = DateTime.Now;

    /// <summary>Legacy coarse category; per-school access is tracked in master UserTenants.</summary>
    public string UserType { get; set; } = string.Empty;
}
