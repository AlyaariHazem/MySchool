using Backend.DTOS.School.Activity;

namespace Backend.Interfaces;

public interface IActivityRepository
{
    Task<IReadOnlyList<ActivityListItemDto>> ListAsync(ActivityFilterDto filter, CancellationToken cancellationToken = default);

    Task<ActivityDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<int> CreateAsync(ActivityRequestWriteDto dto, CancellationToken cancellationToken = default);

    Task UpdateAsync(int id, ActivityRequestWriteDto dto, CancellationToken cancellationToken = default);

    Task<int?> GetSchoolIdForRequestAsync(int activityRequestId, CancellationToken cancellationToken = default);

    Task<int?> GetEmployeeProfileIdForUserInSchoolAsync(string? userId, int schoolId, CancellationToken cancellationToken = default);
}
