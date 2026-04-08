using System;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models.Master;

/// <summary>
/// Links an ASP.NET Identity user (main database) to a school tenant and their role within that school.
/// </summary>
[Index(nameof(UserId), nameof(TenantId), IsUnique = true)]
public class UserTenant
{
    public int UserTenantId { get; set; }

    /// <summary>FK to AspNetUsers.Id (nvarchar(450)).</summary>
    public string UserId { get; set; } = default!;

    public int TenantId { get; set; }

    /// <summary>Role within this tenant only.</summary>
    public TenantRole TenantRole { get; set; }

    /// <summary>When the user last picked this school in the UI (for default selection).</summary>
    public DateTime? LastAccessedUtc { get; set; }

    public bool IsActive { get; set; } = true;

    public global::Backend.Models.ApplicationUser User { get; set; } = default!;
    public global::Backend.Models.Tenant Tenant { get; set; } = default!;
}
