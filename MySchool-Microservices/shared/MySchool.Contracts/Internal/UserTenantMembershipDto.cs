namespace MySchool.Contracts.Internal;

public sealed class UserTenantMembershipDto
{
    public int UserTenantId { get; set; }
    public string UserId { get; set; } = default!;
    public int TenantId { get; set; }
    public Auth.TenantRole TenantRole { get; set; }
    public DateTime? LastAccessedUtc { get; set; }
    public bool IsActive { get; set; }
}
