using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.DTOS.School.Manager;

public class GetManagerDTO
{
    public int? ManagerID { get; set; }
    public Name FullName { get; set; } = new Name();
    public DateTime HireDate { get; set; } = DateTime.Now;
    public string SchoolName { get; set; }
    public int? TenantID { get; set; }
    public string? TenantName { get; set; }
    public string UserName { get; set; }
    public string? Email { get; set; }
    public string UserType { get; set; } = "MANAGER";
    public string? PhoneNumber { get; set; }
}
