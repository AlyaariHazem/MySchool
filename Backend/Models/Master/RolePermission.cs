using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Master;

/// <summary>Master DB: whether a <see cref="SchoolUserRoleKeys"/> role may use a <see cref="Permission"/>.</summary>
public class RolePermission
{
    public int Id { get; set; }

    /// <summary>Matches <see cref="Common.SchoolUserRoleKeys"/> string.</summary>
    [Required, MaxLength(64)]
    public string RoleName { get; set; } = string.Empty;

    public int PermissionId { get; set; }

    public bool IsAllowed { get; set; }

    [ForeignKey(nameof(PermissionId))]
    public Permission? Permission { get; set; }
}
