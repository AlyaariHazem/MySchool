using Microsoft.AspNetCore.Identity;

namespace MySchool.IdentityService.Entities;

public class ApplicationUser : IdentityUser
{
    public string? Address { get; set; }
    public string? Gender { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? PhoneNumberNormalized { get; set; }
    public DateTime HireDate { get; set; } = DateTime.Now;
    public string UserType { get; set; } = string.Empty;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
