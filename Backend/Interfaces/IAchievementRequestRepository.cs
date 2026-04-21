using Backend.DTOS.School.Achievement;

namespace Backend.Interfaces;

public interface IAchievementRequestRepository
{
    Task<IReadOnlyList<AchievementCatalogItemDto>> ListCatalogAsync(int schoolId, int? academicYearId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AchievementRequestListItemDto>> ListAsync(AchievementRequestFilterDto filter, CancellationToken cancellationToken = default);

    Task<AchievementRequestDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<int> CreateAsync(AchievementRequestWriteDto dto, CancellationToken cancellationToken = default);

    Task UpdateAsync(int id, AchievementRequestWriteDto dto, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<int?> GetSchoolIdForRequestAsync(int requestId, CancellationToken cancellationToken = default);
}
