using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Employee;

public class EmployeeDTO
{
    public int? EmployeeID { get; set; }
    public string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string LastName { get; set; }
    public string JopName { get; set; }
    public int? age { get; set; }
    public string? Address { get; set; }
    public string? Mobile { get; set; }
    public string? Gender { get; set; }
    public DateTime? HireDate { get; set; }
    public string? Email { get; set; }
    public string? ImageURL { get; set; }
    public int? ManagerID { get; set; }


}
