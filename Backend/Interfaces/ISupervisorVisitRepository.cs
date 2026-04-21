using Backend.DTOS.School.SupervisorVisit;

namespace Backend.Interfaces;

public interface ISupervisorVisitRepository
{
    Task<IReadOnlyList<SupervisorVisitListItemDto>> ListAsync(SupervisorVisitFilterDto filter, CancellationToken cancellationToken = default);

    Task<SupervisorVisitDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<int> CreateAsync(SupervisorVisitWriteDto dto, CancellationToken cancellationToken = default);

    Task UpdateAsync(int id, SupervisorVisitWriteDto dto, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Returns visit school id if it exists.</summary>
    Task<int?> GetSchoolIdForVisitAsync(int visitId, CancellationToken cancellationToken = default);
}
