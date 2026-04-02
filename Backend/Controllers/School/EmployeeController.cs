using Backend.Controllers;
using Backend.DTOS.School.Employee;
using Backend.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School;

/// <summary>
/// Employee CRUD via <see cref="GenericCrudController{TEntity,TKey}"/> (teachers and managers as <see cref="EmployeeDTO"/>).
/// DELETE requires <c>?jobType=Teacher</c> or <c>?jobType=Manager</c>.
/// </summary>
[Route("api/[controller]")]
public class EmployeeController : GenericCrudController<EmployeeDTO, int>
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeController(
        IGenericCrudRepository<EmployeeDTO, int> repository,
        IEmployeeRepository employeeRepository)
        : base(repository)
    {
        _employeeRepository = employeeRepository;
    }

    /// <summary>DELETE /{id}?jobType=Teacher|Manager — disambiguates teacher vs manager identity.</summary>
    [HttpDelete("{id}")]
    public override async Task<IActionResult> Delete(int id)
    {
        var jobType = Request.Query["jobType"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(jobType))
        {
            return BadRequest(new
            {
                message = "Query parameter jobType is required (Teacher or Manager)."
            });
        }

        var existing = await _employeeRepository.GetEmployeeByIdAsync(id);
        if (existing == null)
            return NotFound(new { message = $"Employee with ID {id} not found." });

        if (!string.Equals(existing.JopName, jobType, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = $"Employee {id} is recorded as {existing.JopName}; jobType must match."
            });
        }

        await _employeeRepository.DeleteEmployeeAsync(id, jobType);
        return NoContent();
    }
}
