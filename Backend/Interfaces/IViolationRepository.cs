using Backend.DTOS.School.Violation;

namespace Backend.Interfaces;

public interface IViolationRepository
{
    Task<IReadOnlyList<ViolationTypeListItemDto>> ListTypesAsync(int schoolId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ViolationListItemDto>> ListAsync(ViolationFilterDto filter, CancellationToken cancellationToken = default);

    Task<ViolationDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<int> CreateAsync(ViolationWriteDto dto, CancellationToken cancellationToken = default);

    Task UpdateAsync(int id, ViolationWriteDto dto, CancellationToken cancellationToken = default);

    Task<int> AddResponseAsync(int violationId, ViolationResponseWriteDto dto, CancellationToken cancellationToken = default);

    Task<int> AddActionAsync(int violationId, ViolationActionWriteDto dto, CancellationToken cancellationToken = default);

    Task EscalateAsync(int violationId, ViolationEscalateDto dto, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<int?> GetSchoolIdForViolationAsync(int violationId, CancellationToken cancellationToken = default);
}
