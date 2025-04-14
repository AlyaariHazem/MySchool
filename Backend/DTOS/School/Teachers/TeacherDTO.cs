using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Teachers;

public class TeacherDTO
{
    public int? TeacherID { get; set; }
    public string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string? UserID { get; set; }
    public string? UserName { get; set; }
    public string? Gender { get; set; }="Male";
    public string? Address { get; set; }
    public string Email { get; set; }
    public int ManagerID { get; set; }
}
