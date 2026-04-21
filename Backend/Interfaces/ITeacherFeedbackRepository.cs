using Backend.DTOS.School.TeacherFeedback;

namespace Backend.Interfaces;

public interface ITeacherFeedbackRepository
{
    Task<int?> GetSchoolIdForCycleAsync(int cycleId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TeacherFeedbackCycleListItemDto>> ListCyclesAsync(
        TeacherFeedbackCycleFilterDto filter,
        CancellationToken cancellationToken = default);

    Task<TeacherFeedbackCycleDetailDto?> GetCycleByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<int> CreateCycleAsync(TeacherFeedbackCycleWriteDto dto, CancellationToken cancellationToken = default);

    Task UpdateCycleAsync(int id, TeacherFeedbackCycleWriteDto dto, CancellationToken cancellationToken = default);

    Task<bool> DeleteCycleAsync(int id, CancellationToken cancellationToken = default);

    Task RecomputeSummariesAsync(int cycleId, CancellationToken cancellationToken = default);

    Task UpsertStudentFeedbackAsync(int studentId, StudentFeedbackSubmitDto dto, CancellationToken cancellationToken = default);

    Task UpsertParentFeedbackAsync(int guardianId, ParentFeedbackSubmitDto dto, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TeacherFeedbackOpenCycleDto>> ListOpenCyclesForStudentAsync(
        int studentId,
        CancellationToken cancellationToken = default);

    Task<TeacherFeedbackParticipantFormDto?> GetStudentCycleFormAsync(
        int studentId,
        int cycleId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TeacherFeedbackOpenCycleDto>> ListOpenCyclesForGuardianAsync(
        int guardianId,
        CancellationToken cancellationToken = default);

    Task<TeacherFeedbackParticipantFormDto?> GetParentCycleFormAsync(
        int guardianId,
        int cycleId,
        int studentId,
        CancellationToken cancellationToken = default);
}
