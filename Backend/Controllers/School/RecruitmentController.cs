using System.Net;
using System.Security.Claims;
using Backend.DTOS.School.Recruitment;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

/// <summary>Recruitment and hiring workflow (job postings through conversion to <c>EmployeeProfile</c>).</summary>
[ApiController]
[Route("api/recruitment")]
[Authorize]
public class RecruitmentController : ControllerBase
{
    private readonly IRecruitmentService _recruitment;

    public RecruitmentController(IRecruitmentService recruitment)
    {
        _recruitment = recruitment;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private static bool IsSchoolHrUser(ClaimsPrincipal user) =>
        user.IsInRole("ADMIN") || user.IsInRole("MANAGER");

    /// <summary>Non-HR users only see active, open postings (public job board).</summary>
    private static JobPostingFilterDto ApplyPublicJobBoardFilter(JobPostingFilterDto? filter, ClaimsPrincipal? user)
    {
        if (user != null && IsSchoolHrUser(user))
            return filter ?? new JobPostingFilterDto();

        var f = filter ?? new JobPostingFilterDto();
        f.Status = JobPostingStatus.Open;
        f.IsActive = true;
        return f;
    }

    // --- Job postings ---

    [HttpGet("job-postings")]
    [AllowAnonymous]
    public async Task<ActionResult<APIResponse>> GetJobPostings([FromQuery] JobPostingFilterDto? filter, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            filter = ApplyPublicJobBoardFilter(filter, User.Identity?.IsAuthenticated == true ? User : null);
            response.Result = await _recruitment.GetJobPostingsAsync(filter, cancellationToken);
            response.statusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.InternalServerError;
            response.ErrorMasseges.Add(ex.Message);
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }

    [HttpGet("job-postings/{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<APIResponse>> GetJobPostingById(int id, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            var row = await _recruitment.GetJobPostingByIdAsync(id, cancellationToken);
            if (row == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add($"Job posting {id} was not found.");
                return NotFound(response);
            }

            if (!IsSchoolHrUser(User)
                && (row.Status != JobPostingStatus.Open || !row.IsActive))
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add($"Job posting {id} was not found.");
                return NotFound(response);
            }

            response.Result = row;
            response.statusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.InternalServerError;
            response.ErrorMasseges.Add(ex.Message);
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }

    [HttpPost("job-postings")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> CreateJobPosting([FromBody] JobPostingCreateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.CreateJobPostingAsync(body, cancellationToken));

    [HttpPut("job-postings/{id:int}")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> UpdateJobPosting(int id, [FromBody] JobPostingUpdateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.UpdateJobPostingAsync(id, body, cancellationToken));

    [HttpPost("job-postings/{id:int}/open")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> OpenJobPosting(int id, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.OpenJobPostingAsync(id, cancellationToken));

    [HttpPost("job-postings/{id:int}/close")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> CloseJobPosting(int id, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.CloseJobPostingAsync(id, cancellationToken));

    [HttpPost("job-postings/{id:int}/archive")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> ArchiveJobPosting(int id, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.ArchiveJobPostingAsync(id, cancellationToken));

    // --- Job applications ---

    [HttpGet("job-applications")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> GetJobApplications([FromQuery] JobApplicationFilterDto? filter, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            response.Result = await _recruitment.GetJobApplicationsAsync(filter, cancellationToken);
            response.statusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.InternalServerError;
            response.ErrorMasseges.Add(ex.Message);
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }

    [HttpGet("job-applications/{id:int}")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> GetJobApplicationById(int id, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            var row = await _recruitment.GetJobApplicationByIdAsync(id, cancellationToken);
            if (row == null)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.NotFound;
                response.ErrorMasseges.Add($"Job application {id} was not found.");
                return NotFound(response);
            }
            response.Result = row;
            response.statusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.InternalServerError;
            response.ErrorMasseges.Add(ex.Message);
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }

    [HttpGet("job-applications/{id:int}/full")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> GetJobApplicationFull(int id, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.GetJobApplicationFullAsync(id, cancellationToken));

    [HttpPost("job-applications")]
    [AllowAnonymous]
    public async Task<ActionResult<APIResponse>> CreateJobApplication([FromBody] JobApplicationCreateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.CreateJobApplicationAsync(body, cancellationToken));

    [HttpPut("job-applications/{id:int}")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> UpdateJobApplication(int id, [FromBody] JobApplicationUpdateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.UpdateJobApplicationAsync(id, body, cancellationToken));

    [HttpPost("job-applications/{id:int}/status")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> MoveJobApplicationStatus(int id, [FromBody] JobApplicationStatusMoveDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.MoveJobApplicationStatusAsync(id, body, cancellationToken));

    // --- Interviews ---

    [HttpGet("job-applications/{id:int}/interviews")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> GetInterviewsForApplication(int id, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.GetInterviewsForApplicationAsync(id, cancellationToken));

    [HttpPost("job-applications/{id:int}/interviews")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> ScheduleInterview(int id, [FromBody] InterviewCreateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.ScheduleInterviewAsync(id, body, cancellationToken));

    [HttpPut("interviews/{interviewId:int}")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> UpdateInterview(int interviewId, [FromBody] InterviewUpdateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.UpdateInterviewAsync(interviewId, body, cancellationToken));

    [HttpPost("interviews/{interviewId:int}/complete")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> CompleteInterview(int interviewId, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.CompleteInterviewAsync(interviewId, cancellationToken));

    [HttpPost("interviews/{interviewId:int}/cancel")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> CancelInterview(int interviewId, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.CancelInterviewAsync(interviewId, cancellationToken));

    [HttpPost("interviews/{interviewId:int}/no-show")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> NoShowInterview(int interviewId, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.NoShowInterviewAsync(interviewId, cancellationToken));

    // --- Evaluations ---

    [HttpGet("job-applications/{id:int}/evaluations")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> GetEvaluationsForApplication(int id, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.GetEvaluationsForApplicationAsync(id, cancellationToken));

    [HttpPost("job-applications/{id:int}/evaluations")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> AddEvaluation(int id, [FromBody] CandidateEvaluationCreateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.AddEvaluationAsync(id, body, cancellationToken));

    [HttpPut("evaluations/{evaluationId:int}")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> UpdateEvaluation(int evaluationId, [FromBody] CandidateEvaluationUpdateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.UpdateEvaluationAsync(evaluationId, body, cancellationToken));

    // --- Hiring decisions ---

    [HttpGet("job-applications/{id:int}/decision")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> GetDecisionForApplication(int id, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
            var row = await _recruitment.GetDecisionForApplicationAsync(id, cancellationToken);
            response.Result = row;
            response.statusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.InternalServerError;
            response.ErrorMasseges.Add(ex.Message);
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }

    [HttpPost("job-applications/{id:int}/decision")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> CreateDecision(int id, [FromBody] HiringDecisionCreateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.CreateDecisionAsync(id, body, cancellationToken));

    [HttpPut("decisions/{decisionId:int}")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> UpdateDecision(int decisionId, [FromBody] HiringDecisionUpdateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.UpdateDecisionAsync(decisionId, body, cancellationToken));

    [HttpPost("job-applications/{id:int}/accept")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> AcceptApplication(int id, [FromBody] HiringDecisionCreateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.AcceptApplicationAsync(id, body, CurrentUserId, cancellationToken));

    [HttpPost("job-applications/{id:int}/reject")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> RejectApplication(int id, [FromBody] HiringDecisionCreateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.RejectApplicationAsync(id, body, CurrentUserId, cancellationToken));

    [HttpPost("job-applications/{id:int}/convert-to-employee")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<ActionResult<APIResponse>> ConvertToEmployee(int id, [FromBody] ConvertApplicantToEmployeeDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.ConvertAcceptedApplicantToEmployeeAsync(id, body, cancellationToken));

    private async Task<ActionResult<APIResponse>> RunAsync<T>(Func<Task<T>> action)
    {
        var response = new APIResponse();
        try
        {
            response.Result = await action();
            response.statusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.NotFound;
            response.ErrorMasseges.Add(ex.Message);
            return NotFound(response);
        }
        catch (ArgumentException ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.BadRequest;
            response.ErrorMasseges.Add(ex.Message);
            return BadRequest(response);
        }
        catch (InvalidOperationException ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.Conflict;
            response.ErrorMasseges.Add(ex.Message);
            return Conflict(response);
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.statusCode = HttpStatusCode.InternalServerError;
            response.ErrorMasseges.Add(ex.Message);
            return StatusCode((int)HttpStatusCode.InternalServerError, response);
        }
    }
}
