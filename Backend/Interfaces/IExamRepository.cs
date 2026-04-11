using Backend.DTOS.School.Exams;

namespace Backend.Interfaces;

public interface IExamRepository
{
    Task<IReadOnlyList<ExamTypeDto>> GetExamTypesAsync(bool includeInactive, CancellationToken cancellationToken = default);
    Task<ExamTypeDto?> UpdateExamTypeAsync(int examTypeId, string name, int sortOrder, bool isActive, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExamSessionDto>> GetExamSessionsAsync(int? yearId, int? termId, CancellationToken cancellationToken = default);
    Task<ExamSessionDto> CreateExamSessionAsync(CreateExamSessionDto dto, CancellationToken cancellationToken = default);
    Task<ExamSessionDto?> UpdateExamSessionAsync(UpdateExamSessionDto dto, CancellationToken cancellationToken = default);
    Task DeleteExamSessionAsync(int examSessionId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ScheduledExamListDto>> GetScheduledExamsAsync(ExamFilterQuery filter, CancellationToken cancellationToken = default);
    Task<ScheduledExamListDto?> GetScheduledExamByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ScheduledExamListDto> CreateScheduledExamAsync(CreateScheduledExamDto dto, CancellationToken cancellationToken = default);
    Task<ScheduledExamListDto?> UpdateScheduledExamAsync(UpdateScheduledExamDto dto, CancellationToken cancellationToken = default);
    Task DeleteScheduledExamAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ScheduledExamListDto>> GetTeacherScheduledExamsAsync(int teacherId, ExamFilterQuery filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamResultRowDto>> GetExamResultsAsync(int scheduledExamId, CancellationToken cancellationToken = default);
    Task SaveExamResultsAsync(int scheduledExamId, BulkExamResultsDto dto, CancellationToken cancellationToken = default);
    Task PublishResultsAsync(int scheduledExamId, bool publish, CancellationToken cancellationToken = default);
    Task PublishScheduleAsync(int scheduledExamId, bool publish, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentExamCardDto>> GetStudentExamsAsync(int studentId, bool upcomingOnly, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudentExamCardDto>> GetGuardianStudentExamsAsync(int guardianId, int studentId, bool upcomingOnly, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GuardianStudentExamCardDto>> GetGuardianAllStudentsExamsAsync(int guardianId, bool upcomingOnly, CancellationToken cancellationToken = default);

    Task<ClassExamSheetReportDto?> GetClassExamSheetAsync(int scheduledExamId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubjectPerformanceReportDto>> GetSubjectPerformanceAsync(int yearId, int termId, int? classId, int? divisionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TopWeakStudentDto>> GetTopStudentsAsync(int scheduledExamId, int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TopWeakStudentDto>> GetWeakStudentsAsync(int scheduledExamId, int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamResultRowDto>> GetAbsentStudentsAsync(int scheduledExamId, CancellationToken cancellationToken = default);

    Task<int?> GetStudentIdByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<int?> GetGuardianIdByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
