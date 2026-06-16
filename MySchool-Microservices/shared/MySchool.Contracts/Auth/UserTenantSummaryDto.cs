namespace MySchool.Contracts.Auth;

public sealed class UserTenantSummaryDto
{
    public int TenantId { get; set; }
    public string SchoolName { get; set; } = default!;
    public TenantRole TenantRole { get; set; }
    public bool IsDefaultSuggestion { get; set; }
}
