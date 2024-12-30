using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS;

public class RegisterDTO
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string UserType { get; set; } = "Admin"; // Example: "Teacher", "Student", etc.
}