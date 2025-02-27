using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.DTOS.School.Manager;

public class ManagerDTO
{
    public int? ManagerID { get; set; }
    public Name FullName { get; set; } = new Name();
    public string? UserID { get; set; }
    public int SchoolID { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string? Email { get; set; }
    public int? TenantID { get; set; }
    public string UserType { get; set; } = "MANAGER";
    public string? PhoneNumber { get; set; }

}
