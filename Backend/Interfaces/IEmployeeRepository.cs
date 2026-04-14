using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.Employee;

namespace Backend.Interfaces;

public interface IEmployeeRepository
{
    Task<EmployeeDTO> AddEmployeeAsync(EmployeeDTO employee);
    Task<EmployeeDTO?> UpdateEmployeeAsync(int id, EmployeeDTO employee);

    /// <summary>
    /// Archives the employee for the current (or active) academic year — does not delete <see cref="Models.Teacher"/> /
    /// <see cref="Models.Manager"/> rows or identity users.
    /// </summary>
    Task DeleteEmployeeAsync(int employeeId, string jobType);

    /// <param name="schoolYearId">When year assignments are used, filter active staff for this year; null uses active/latest year.</param>
    Task<List<EmployeeDTO>> GetAllEmployeesAsync(int? schoolYearId = null);

    /// <param name="jobTypeHint">When set (e.g. <c>Teacher</c>, <c>SystemAdmin</c>), resolves ambiguous IDs safely.</param>
    Task<EmployeeDTO?> GetEmployeeByIdAsync(int employeeId, string? jobTypeHint = null);
}
