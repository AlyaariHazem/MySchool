using MySchool.Identity.Grpc;

namespace MySchool.WebBff.Common.DTOs.Identity;

public static class RoleListItemMapper
{
    public static IEnumerable<RoleListItemDto> FromGrpc(GetRolesResponse result) =>
        result.Roles.Select(r => new RoleListItemDto
        {
            Id = r.Id,
            Name = r.Name
        });
}

public sealed class RoleListItemDto
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
}
