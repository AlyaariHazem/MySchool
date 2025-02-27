using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS;

public class LoginDto
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public string userType { get; set; }
}
public class ManagerLoginDto
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public int SchoolId { get; set; } = 1; // Or string SchoolName
}
