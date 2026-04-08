namespace Backend.DTOS;

public sealed class SelectTenantDto
{
    /// <summary>Tenant to embed in the next JWT (must exist in UserTenants for this user).</summary>
    public int TenantId { get; set; }
}
