using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Students;

public class GuardianDto
{
    public string FullName { get; set; }
    public string? Type { get; set; }
    public string UserID { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}
