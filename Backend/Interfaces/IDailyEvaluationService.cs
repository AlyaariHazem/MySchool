using Backend.Common;
using Backend.DTOS.School.DailyEvaluation;
using Backend.Models;

namespace Backend.Interfaces;

public interface IDailyEvaluationService
{
    // Templates
    Task<DailyEvaluationTemplateReadDto> CreateTemplateAsync(DailyEvaluationTemplateCreateDto dto, CancellationToken cancellationToken = default);
    Task<DailyEvaluationTemplateReadDto> UpdateTemplateAsync(int id, DailyEvaluationTemplateUpdateDto dto, CancellationToken cancellationToken = default);
    Task<DailyEvaluationTemplateReadDto?> GetTemplateByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<DailyEvaluationTemplateListDto>> GetTemplatesPageAsync(DailyEvaluationTemplatesPageRequestDto request, CancellationToken cancellationToken = default);
    Task<DailyEvaluationTemplateReadDto> ActivateTemplateAsync(int id, CancellationToken cancellationToken = default);
    Task<DailyEvaluationTemplateReadDto> DeactivateTemplateAsync(int id, CancellationToken cancellationToken = default);
    Task<DailyEvaluationTemplateReadDto> ArchiveTemplateAsync(int id, CancellationToken cancellationToken = default);

    // Criteria
    Task<DailyEvaluationCriteriaReadDto> AddCriteriaAsync(int templateId, DailyEvaluationCriteriaCreateDto dto, CancellationToken cancellationToken = default);
    Task<DailyEvaluationCriteriaReadDto> UpdateCriteriaAsync(int criteriaId, DailyEvaluationCriteriaUpdateDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DailyEvaluationCriteriaReadDto>> GetCriteriaForTemplateAsync(int templateId, CancellationToken cancellationToken = default);

    // Evaluations
    Task<DailyEvaluationReadDto> CreateEvaluationAsync(DailyEvaluationCreateDto dto, CancellationToken cancellationToken = default);
    Task<DailyEvaluationReadDto?> GetEvaluationByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<DailyEvaluationFullDto> GetEvaluationFullAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<DailyEvaluationListDto>> GetEvaluationsPageAsync(DailyEvaluationsPageRequestDto request, CancellationToken cancellationToken = default);
    /// <summary>Tenant <see cref="Models.Manager.SchoolID"/> for the given user, when the user is a school manager.</summary>
    Task<int?> GetSchoolIdForManagerUserAsync(string? userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TeacherEvaluationOptionDto>> GetTeachersForStudentEvaluationAsync(
        int schoolId,
        string? studentUserId,
        CancellationToken cancellationToken = default);
    Task<string?> ValidateStudentEvaluationCreateAsync(DailyEvaluationCreateDto body, string studentUserId, CancellationToken cancellationToken = default);
    Task<int?> GetEvaluationIdForItemAsync(int dailyEvaluationItemId, CancellationToken cancellationToken = default);
    Task<DailyEvaluationReadDto> UpdateEvaluationAsync(int id, DailyEvaluationUpdateDto dto, string? currentUserId, CancellationToken cancellationToken = default);
    Task<DailyEvaluationReadDto> SubmitEvaluationAsync(int id, CancellationToken cancellationToken = default);

    Task<DailyEvaluationItemReadDto> UpsertItemAsync(int evaluationId, DailyEvaluationItemCreateDto dto, CancellationToken cancellationToken = default);
    Task<DailyEvaluationItemReadDto> UpdateItemAsync(int itemId, DailyEvaluationItemUpdateDto dto, CancellationToken cancellationToken = default);

    // Locks
    Task<EvaluationLockReadDto> LockDayAsync(EvaluationLockCreateDto dto, string lockedByUserId, CancellationToken cancellationToken = default);
    Task<EvaluationLockReadDto?> GetLockByDateAsync(int schoolId, int academicYearId, DateOnly date, int? templateId, CancellationToken cancellationToken = default);
    Task<EvaluationLockReadDto> ReopenLockAsync(int lockId, EvaluationReopenDto dto, string reopenedByUserId, CancellationToken cancellationToken = default);

    // Overrides
    Task<DailyEvaluationReadDto> OverrideUpdateAfterLockAsync(int evaluationId, EvaluationOverrideRequestDto dto, string performedByUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EvaluationOverrideLogReadDto>> GetOverrideLogsForEvaluationAsync(int evaluationId, CancellationToken cancellationToken = default);
}
