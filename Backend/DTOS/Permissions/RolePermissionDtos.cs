namespace Backend.DTOS.Permissions;

public class RolePermissionMatrixDto
{
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
    public IReadOnlyList<PermissionItemDto> Permissions { get; set; } = Array.Empty<PermissionItemDto>();
    /// <summary>Flattened cells: key = RoleName + \"|\" + PermissionName, value = allowed.</summary>
    public Dictionary<string, bool> Matrix { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class PermissionItemDto
{
    public string Name { get; set; } = string.Empty;
    public string Page { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
}

public class RolePermissionMatrixUpdateDto
{
    /// <summary>
    /// When set, every cell applies only to this school role (partial matrix update).
    /// Cells may omit <see cref="RolePermissionCellDto.RoleName"/>; if provided it must match.
    /// </summary>
    public string? ScopeToRoleName { get; set; }

    public List<RolePermissionCellDto> Cells { get; set; } = new();
}

public class RolePermissionCellDto
{
    /// <summary>Optional when <see cref="RolePermissionMatrixUpdateDto.ScopeToRoleName"/> is set.</summary>
    public string? RoleName { get; set; }
    public string PermissionName { get; set; } = string.Empty;
    public bool IsAllowed { get; set; }
}
