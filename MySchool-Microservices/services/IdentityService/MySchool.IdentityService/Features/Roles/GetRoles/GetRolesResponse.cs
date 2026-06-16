namespace MySchool.IdentityService.Features.Roles.GetRoles;

public sealed class RoleItemDto
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
}

public sealed class GetRolesResponse
{
    public IReadOnlyList<RoleItemDto> Roles { get; init; } = Array.Empty<RoleItemDto>();
}
