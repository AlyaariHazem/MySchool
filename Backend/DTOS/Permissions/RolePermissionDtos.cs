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
    public List<RolePermissionCellDto> Cells { get; set; } = new();
}

public class RolePermissionCellDto
{
    public string RoleName { get; set; } = string.Empty;
    public string PermissionName { get; set; } = string.Empty;
    public bool IsAllowed { get; set; }
}
