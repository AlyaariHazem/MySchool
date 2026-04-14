using System.Collections.Generic;
using Backend.Common;
using Backend.Controllers;
using Backend.DTOS.School.Employee;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

/// <summary>
/// Employee CRUD via <see cref="GenericCrudController{TEntity,TKey}"/> (teachers and managers as <see cref="EmployeeDTO"/>).
/// DELETE / archive require <c>?jobType=Teacher</c> or <c>?jobType=Manager</c> (or <see cref="ArchiveEmployeeRequestDTO.JobType"/> in body).
/// Physical deletion of teachers/managers is disabled — DELETE performs a per-year archive (deactivate).
/// </summary>
[Route("api/[controller]")]
public class EmployeeController : GenericCrudController<EmployeeDTO, int>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeeYearAssignmentService _yearAssignments;

    public EmployeeController(
        IGenericCrudRepository<EmployeeDTO, int> repository,
        IEmployeeRepository employeeRepository,
        IEmployeeYearAssignmentService yearAssignments)
        : base(repository)
    {
        _employeeRepository = employeeRepository;
        _yearAssignments = yearAssignments;
    }

    /// <summary>GET /{id}?jobType=Teacher|Manager|Student|… — optional job type disambiguates overlapping numeric IDs.</summary>
    [HttpGet("{id}")]
    public override async Task<IActionResult> GetById(int id)
    {
        var jobType = GetJobTypeFromQuery();
        var entity = await _employeeRepository.GetEmployeeByIdAsync(id, jobType);
        if (entity == null)
            return NotFound(new { message = $"Entity with ID {id} was not found." });

        return Ok(entity);
    }

    /// <summary>
    /// Archive (deactivate) the employee for a school year with optional exit metadata.
    /// Does not remove <see cref="Teacher"/> / <see cref="Manager"/> or AspNetUsers rows.
    /// </summary>
    [HttpPost("{id}/archive")]
    public async Task<IActionResult> ArchiveEmployee(int id, [FromBody] ArchiveEmployeeRequestDTO? body)
    {
        var jobType = body?.JobType;
        if (string.IsNullOrWhiteSpace(jobType))
            jobType = GetJobTypeFromQuery();

        if (string.IsNullOrWhiteSpace(jobType))
        {
            return BadRequest(new
            {
                message = "jobType is required (Teacher, Manager, Student, Guardian, SystemAdmin, …), in the JSON body or query string."
            });
        }

        if (!await _employeeRepository.ExistsForJobTypeAsync(id, jobType))
            return NotFound(new { message = $"No employee {id} with job type {jobType}." });

        var role = EmployeeJopNameToYearRole.ToAssignmentRole(jobType);

        await _yearAssignments.ArchiveEmployeeForYearAsync(
            role,
            id,
            body?.YearId,
            body?.ExitDate ?? DateTime.UtcNow,
            body?.ExitReason,
            body?.Notes,
            null);

        return NoContent();
    }

    /// <summary>
    /// Copy selected teachers/managers into the target year as Active (continue staff). Skips duplicates.
    /// </summary>
    [HttpPost("rollover/continue")]
    public async Task<IActionResult> RolloverContinue([FromBody] EmployeeYearRolloverRequestDTO? body)
    {
        if (body == null)
            return BadRequest(new { message = "Request body is required." });

        try
        {
            await _yearAssignments.RolloverContinueAsync(
                body.SourceYearId,
                body.TargetYearId,
                body.TeacherIds ?? new List<int>(),
                body.ManagerIds ?? new List<int>(),
                body.SchoolStaffIds,
                null);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        return Ok(new { message = "Rollover completed." });
    }

    /// <summary>
    /// DELETE /{id}?jobType=Teacher|Manager — archives the employee for the current school year (same as deactivate).
    /// </summary>
    [HttpDelete("{id}")]
    public override async Task<IActionResult> Delete(int id)
    {
        var jobType = GetJobTypeFromQuery();
        if (string.IsNullOrWhiteSpace(jobType))
        {
            return BadRequest(new
            {
                message = "Query parameter jobType is required (Teacher, Manager, Student, Guardian, SystemAdmin, …)."
            });
        }

        var archived = await _employeeRepository.DeleteEmployeeAsync(id, jobType);
        if (!archived)
            return NotFound(new { message = $"No employee {id} with job type {jobType}." });

        return NoContent();
    }

    /// <summary>Reads <c>jobType</c> / <c>JobType</c> from the query string.</summary>
    private string? GetJobTypeFromQuery()
    {
        if (Request.Query.TryGetValue("jobType", out var a) && a.Count > 0)
        {
            var s = a.ToString().Trim();
            if (s.Length > 0) return s;
        }

        if (Request.Query.TryGetValue("JobType", out var b) && b.Count > 0)
        {
            var s = b.ToString().Trim();
            if (s.Length > 0) return s;
        }

        return null;
    }
}
