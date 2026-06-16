using System;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models.Master;

/// <summary>
/// Links an Identity user (Identity service) to a school tenant and their role within that school.
/// </summary>
[Index(nameof(UserId), nameof(TenantId), IsUnique = true)]
public class UserTenant
{
    public int UserTenantId { get; set; }

    /// <summary>FK to Identity user id (nvarchar(450)).</summary>
    public string UserId { get; set; } = default!;

    public int TenantId { get; set; }

    /// <summary>Role within this tenant only.</summary>
    public TenantRole TenantRole { get; set; }

    /// <summary>When the user last picked this school in the UI (for default selection).</summary>
    public DateTime? LastAccessedUtc { get; set; }

    public bool IsActive { get; set; } = true;

    public global::Backend.Models.Tenant Tenant { get; set; } = default!;
}
