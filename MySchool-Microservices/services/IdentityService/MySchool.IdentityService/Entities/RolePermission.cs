using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MySchool.IdentityService.Entities;

public class RolePermission
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string RoleName { get; set; } = string.Empty;

    public int PermissionId { get; set; }
    public bool IsAllowed { get; set; }

    [ForeignKey(nameof(PermissionId))]
    public Permission? Permission { get; set; }
}
