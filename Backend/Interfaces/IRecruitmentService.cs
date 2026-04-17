using Backend.DTOS.School.Recruitment;
using Backend.Models;

namespace Backend.Interfaces;

public interface IRecruitmentService
{
    // Job postings
    Task<JobPostingReadDto> CreateJobPostingAsync(JobPostingCreateDto dto, CancellationToken cancellationToken = default);
    Task<JobPostingReadDto> UpdateJobPostingAsync(int id, JobPostingUpdateDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JobPostingListDto>> GetJobPostingsAsync(JobPostingFilterDto? filter, CancellationToken cancellationToken = default);
    Task<JobPostingReadDto?> GetJobPostingByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<JobPostingReadDto> OpenJobPostingAsync(int id, CancellationToken cancellationToken = default);
    Task<JobPostingReadDto> CloseJobPostingAsync(int id, CancellationToken cancellationToken = default);
    Task<JobPostingReadDto> ArchiveJobPostingAsync(int id, CancellationToken cancellationToken = default);

    // Applications
    Task<JobApplicationReadDto> CreateJobApplicationAsync(JobApplicationCreateDto dto, CancellationToken cancellationToken = default);
    Task<JobApplicationReadDto> UpdateJobApplicationAsync(int id, JobApplicationUpdateDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JobApplicationListDto>> GetJobApplicationsAsync(JobApplicationFilterDto? filter, CancellationToken cancellationToken = default);
    Task<JobApplicationReadDto?> GetJobApplicationByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<JobApplicationFullDto> GetJobApplicationFullAsync(int id, CancellationToken cancellationToken = default);
    Task<JobApplicationReadDto> MoveJobApplicationStatusAsync(int id, JobApplicationStatusMoveDto dto, CancellationToken cancellationToken = default);

    // Interviews
    Task<InterviewReadDto> ScheduleInterviewAsync(int jobApplicationId, InterviewCreateDto dto, CancellationToken cancellationToken = default);
    Task<InterviewReadDto> UpdateInterviewAsync(int interviewId, InterviewUpdateDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InterviewReadDto>> GetInterviewsForApplicationAsync(int jobApplicationId, CancellationToken cancellationToken = default);
    Task<InterviewReadDto> CompleteInterviewAsync(int interviewId, CancellationToken cancellationToken = default);
    Task<InterviewReadDto> CancelInterviewAsync(int interviewId, CancellationToken cancellationToken = default);
    Task<InterviewReadDto> NoShowInterviewAsync(int interviewId, CancellationToken cancellationToken = default);

    // Evaluations
    Task<CandidateEvaluationReadDto> AddEvaluationAsync(int jobApplicationId, CandidateEvaluationCreateDto dto, CancellationToken cancellationToken = default);
    Task<CandidateEvaluationReadDto> UpdateEvaluationAsync(int evaluationId, CandidateEvaluationUpdateDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CandidateEvaluationReadDto>> GetEvaluationsForApplicationAsync(int jobApplicationId, CancellationToken cancellationToken = default);

    // Hiring decisions
    Task<HiringDecisionReadDto?> GetDecisionForApplicationAsync(int jobApplicationId, CancellationToken cancellationToken = default);
    Task<HiringDecisionReadDto> CreateDecisionAsync(int jobApplicationId, HiringDecisionCreateDto dto, CancellationToken cancellationToken = default);
    Task<HiringDecisionReadDto> UpdateDecisionAsync(int hiringDecisionId, HiringDecisionUpdateDto dto, CancellationToken cancellationToken = default);
    Task<HiringDecisionReadDto> AcceptApplicationAsync(int jobApplicationId, HiringDecisionCreateDto dto, string? decidedByUserId, CancellationToken cancellationToken = default);
    Task<HiringDecisionReadDto> RejectApplicationAsync(int jobApplicationId, HiringDecisionCreateDto dto, string? decidedByUserId, CancellationToken cancellationToken = default);
    Task<ConvertApplicantToEmployeeResultDto> ConvertAcceptedApplicantToEmployeeAsync(int jobApplicationId, ConvertApplicantToEmployeeDto dto, CancellationToken cancellationToken = default);
}
