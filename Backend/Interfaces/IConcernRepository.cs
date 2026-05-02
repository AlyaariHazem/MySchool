using Backend.DTOS.School.Concern;

namespace Backend.Interfaces;

public interface IConcernRepository
{
    Task<IReadOnlyList<ConcernCategoryListItemDto>> ListCategoriesAsync(ConcernCategoriesFilterDto filter, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ComplaintListItemDto>> ListComplaintsAsync(ConcernFilterDto filter, CancellationToken cancellationToken = default);

    Task<ComplaintDetailDto?> GetComplaintByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<int> CreateComplaintAsync(ComplaintWriteDto dto, int? actorEmployeeProfileId, CancellationToken cancellationToken = default);

    Task UpdateComplaintAsync(int id, ComplaintWriteDto dto, int? actorEmployeeProfileId, CancellationToken cancellationToken = default);

    Task<int?> GetSchoolIdForComplaintAsync(int complaintId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SuggestionListItemDto>> ListSuggestionsAsync(ConcernFilterDto filter, CancellationToken cancellationToken = default);

    Task<SuggestionDetailDto?> GetSuggestionByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<int> CreateSuggestionAsync(SuggestionWriteDto dto, int? actorEmployeeProfileId, CancellationToken cancellationToken = default);

    Task UpdateSuggestionAsync(int id, SuggestionWriteDto dto, int? actorEmployeeProfileId, CancellationToken cancellationToken = default);

    Task<int?> GetSchoolIdForSuggestionAsync(int suggestionId, CancellationToken cancellationToken = default);

    Task<int?> GetEmployeeProfileIdForUserInSchoolAsync(string? userId, int schoolId, CancellationToken cancellationToken = default);
}
