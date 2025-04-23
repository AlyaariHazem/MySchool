using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.Employee;

namespace Backend.Interfaces;

public interface IEmployeeRepository
{
    Task<string> AddEmployeeAsync(EmployeeDTO employee);
    Task UpdateEmployeeAsync(EmployeeDTO employee);
    Task DeleteEmployeeAsync(int employeeId);
    Task<List<EmployeeDTO>> GetAllEmployeesAsync();
    Task<EmployeeDTO> GetEmployeeByIdAsync(int employeeId);
}
