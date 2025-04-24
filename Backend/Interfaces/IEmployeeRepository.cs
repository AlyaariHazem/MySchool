using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.Employee;

namespace Backend.Interfaces;

public interface IEmployeeRepository
{
    Task<string> AddEmployeeAsync(EmployeeDTO employee);
    Task<string> UpdateEmployeeAsync(int id,EmployeeDTO employee);
    Task DeleteEmployeeAsync(int employeeId,string jobType);
    Task<List<EmployeeDTO>> GetAllEmployeesAsync();
    Task<EmployeeDTO> GetEmployeeByIdAsync(int employeeId);
}
