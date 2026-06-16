using System.ComponentModel.DataAnnotations;

namespace MySchool.IdentityService.Entities;

public class Permission
{
    public int Id { get; set; }

    [Required, MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(64)]
    public string Page { get; set; } = string.Empty;

    [Required, MaxLength(32)]
    public string Action { get; set; } = string.Empty;
}
