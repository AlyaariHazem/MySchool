using Backend.DTOS.School.Homework;

namespace Backend.Interfaces;

public interface IHomeworkRepository
{
    Task<int?> GetStudentIdByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<int?> GetGuardianIdByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    Task<HomeworkTaskDetailDto> CreateTaskAsync(int teacherId, CreateHomeworkTaskDto dto, bool skipCoursePlanCheck, CancellationToken cancellationToken = default);
    Task<HomeworkTaskDetailDto?> UpdateTaskAsync(int homeworkTaskId, int teacherId, UpdateHomeworkTaskDto dto, bool skipCoursePlanCheck, bool privileged, CancellationToken cancellationToken = default);
    Task<bool> DeleteTaskAsync(int homeworkTaskId, int teacherId, bool privileged, CancellationToken cancellationToken = default);

    Task<HomeworkTaskDetailDto?> GetTaskByIdAsync(int homeworkTaskId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HomeworkTaskListDto>> ListTasksForTeacherAsync(int teacherId, HomeworkFilterQuery filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HomeworkTaskListDto>> ListTasksPrivilegedAsync(HomeworkFilterQuery filter, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HomeworkSubmissionRowDto>> ListSubmissionsForTaskAsync(int homeworkTaskId, CancellationToken cancellationToken = default);

    Task<HomeworkSubmissionRowDto?> ReviewSubmissionAsync(int homeworkSubmissionId, int teacherId, ReviewHomeworkSubmissionDto dto, bool privileged, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentHomeworkListItemDto>> ListStudentTasksAsync(int studentId, string? filter, CancellationToken cancellationToken = default);
    Task<StudentHomeworkDetailDto?> GetStudentTaskDetailAsync(int studentId, int homeworkTaskId, CancellationToken cancellationToken = default);
    Task<StudentHomeworkDetailDto?> SubmitStudentTaskAsync(int studentId, int homeworkTaskId, StudentSubmitHomeworkDto dto, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentHomeworkListItemDto>> ListGuardianStudentTasksAsync(int guardianId, int studentId, string? filter, CancellationToken cancellationToken = default);

    /// <summary>All homework submissions for students linked to this guardian (one row per submission).</summary>
    Task<IReadOnlyList<GuardianStudentHomeworkRowDto>> ListAllGuardianStudentTasksAsync(int guardianId, string? filter, CancellationToken cancellationToken = default);

    Task<StudentHomeworkDetailDto?> GetGuardianStudentTaskDetailAsync(int guardianId, int studentId, int homeworkTaskId, CancellationToken cancellationToken = default);

    Task<HomeworkActivitySummaryDto> GetActivitySummaryAsync(int yearId, int termId, int? classId, int? teacherId, CancellationToken cancellationToken = default);
}
