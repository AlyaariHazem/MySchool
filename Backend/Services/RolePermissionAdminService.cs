using Backend.DTOS.Permissions;
using Backend.Interfaces;

namespace Backend.Services;

public class RolePermissionAdminService : IRolePermissionAdminService
{
    private const string Message =
        "Role permissions are managed by the Identity service after extraction.";

    public Task<RolePermissionMatrixDto> GetMatrixAsync(CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(Message);

    public Task SaveMatrixAsync(RolePermissionMatrixUpdateDto dto, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(Message);
}
