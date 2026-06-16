namespace MySchool.IdentityService.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public string TokenHash { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public DateTime Expires { get; set; }
    public DateTime? Revoked { get; set; }
    public ApplicationUser User { get; set; } = default!;
}
