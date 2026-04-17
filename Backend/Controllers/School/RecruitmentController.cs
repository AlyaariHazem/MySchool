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
[Authorize(Roles = "ADMIN,MANAGER")]
public class RecruitmentController : ControllerBase
{
    private readonly IRecruitmentService _recruitment;

    public RecruitmentController(IRecruitmentService recruitment)
    {
        _recruitment = recruitment;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    // --- Job postings ---

    [HttpGet("job-postings")]
    public async Task<ActionResult<APIResponse>> GetJobPostings([FromQuery] JobPostingFilterDto? filter, CancellationToken cancellationToken)
    {
        var response = new APIResponse();
        try
        {
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
    public async Task<ActionResult<APIResponse>> CreateJobPosting([FromBody] JobPostingCreateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.CreateJobPostingAsync(body, cancellationToken));

    [HttpPut("job-postings/{id:int}")]
    public async Task<ActionResult<APIResponse>> UpdateJobPosting(int id, [FromBody] JobPostingUpdateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.UpdateJobPostingAsync(id, body, cancellationToken));

    [HttpPost("job-postings/{id:int}/open")]
    public async Task<ActionResult<APIResponse>> OpenJobPosting(int id, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.OpenJobPostingAsync(id, cancellationToken));

    [HttpPost("job-postings/{id:int}/close")]
    public async Task<ActionResult<APIResponse>> CloseJobPosting(int id, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.CloseJobPostingAsync(id, cancellationToken));

    [HttpPost("job-postings/{id:int}/archive")]
    public async Task<ActionResult<APIResponse>> ArchiveJobPosting(int id, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.ArchiveJobPostingAsync(id, cancellationToken));

    // --- Job applications ---

    [HttpGet("job-applications")]
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
    public async Task<ActionResult<APIResponse>> GetJobApplicationFull(int id, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.GetJobApplicationFullAsync(id, cancellationToken));

    [HttpPost("job-applications")]
    public async Task<ActionResult<APIResponse>> CreateJobApplication([FromBody] JobApplicationCreateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.CreateJobApplicationAsync(body, cancellationToken));

    [HttpPut("job-applications/{id:int}")]
    public async Task<ActionResult<APIResponse>> UpdateJobApplication(int id, [FromBody] JobApplicationUpdateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.UpdateJobApplicationAsync(id, body, cancellationToken));

    [HttpPost("job-applications/{id:int}/status")]
    public async Task<ActionResult<APIResponse>> MoveJobApplicationStatus(int id, [FromBody] JobApplicationStatusMoveDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.MoveJobApplicationStatusAsync(id, body, cancellationToken));

    // --- Interviews ---

    [HttpGet("job-applications/{id:int}/interviews")]
    public async Task<ActionResult<APIResponse>> GetInterviewsForApplication(int id, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.GetInterviewsForApplicationAsync(id, cancellationToken));

    [HttpPost("job-applications/{id:int}/interviews")]
    public async Task<ActionResult<APIResponse>> ScheduleInterview(int id, [FromBody] InterviewCreateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.ScheduleInterviewAsync(id, body, cancellationToken));

    [HttpPut("interviews/{interviewId:int}")]
    public async Task<ActionResult<APIResponse>> UpdateInterview(int interviewId, [FromBody] InterviewUpdateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.UpdateInterviewAsync(interviewId, body, cancellationToken));

    [HttpPost("interviews/{interviewId:int}/complete")]
    public async Task<ActionResult<APIResponse>> CompleteInterview(int interviewId, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.CompleteInterviewAsync(interviewId, cancellationToken));

    [HttpPost("interviews/{interviewId:int}/cancel")]
    public async Task<ActionResult<APIResponse>> CancelInterview(int interviewId, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.CancelInterviewAsync(interviewId, cancellationToken));

    [HttpPost("interviews/{interviewId:int}/no-show")]
    public async Task<ActionResult<APIResponse>> NoShowInterview(int interviewId, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.NoShowInterviewAsync(interviewId, cancellationToken));

    // --- Evaluations ---

    [HttpGet("job-applications/{id:int}/evaluations")]
    public async Task<ActionResult<APIResponse>> GetEvaluationsForApplication(int id, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.GetEvaluationsForApplicationAsync(id, cancellationToken));

    [HttpPost("job-applications/{id:int}/evaluations")]
    public async Task<ActionResult<APIResponse>> AddEvaluation(int id, [FromBody] CandidateEvaluationCreateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.AddEvaluationAsync(id, body, cancellationToken));

    [HttpPut("evaluations/{evaluationId:int}")]
    public async Task<ActionResult<APIResponse>> UpdateEvaluation(int evaluationId, [FromBody] CandidateEvaluationUpdateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.UpdateEvaluationAsync(evaluationId, body, cancellationToken));

    // --- Hiring decisions ---

    [HttpGet("job-applications/{id:int}/decision")]
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
    public async Task<ActionResult<APIResponse>> CreateDecision(int id, [FromBody] HiringDecisionCreateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.CreateDecisionAsync(id, body, cancellationToken));

    [HttpPut("decisions/{decisionId:int}")]
    public async Task<ActionResult<APIResponse>> UpdateDecision(int decisionId, [FromBody] HiringDecisionUpdateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.UpdateDecisionAsync(decisionId, body, cancellationToken));

    [HttpPost("job-applications/{id:int}/accept")]
    public async Task<ActionResult<APIResponse>> AcceptApplication(int id, [FromBody] HiringDecisionCreateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.AcceptApplicationAsync(id, body, CurrentUserId, cancellationToken));

    [HttpPost("job-applications/{id:int}/reject")]
    public async Task<ActionResult<APIResponse>> RejectApplication(int id, [FromBody] HiringDecisionCreateDto body, CancellationToken cancellationToken) =>
        await RunAsync(() => _recruitment.RejectApplicationAsync(id, body, CurrentUserId, cancellationToken));

    [HttpPost("job-applications/{id:int}/convert-to-employee")]
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
