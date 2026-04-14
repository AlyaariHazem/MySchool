using Backend.DTOS.Permissions;

namespace Backend.Interfaces;

public interface IRolePermissionAdminService
{
    Task<RolePermissionMatrixDto> GetMatrixAsync(CancellationToken cancellationToken = default);
    Task SaveMatrixAsync(RolePermissionMatrixUpdateDto dto, CancellationToken cancellationToken = default);
}
