using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.DTOS.School.Students;

public class GuardianDto
{
    public string guardianFullName { get; set; }
    public string? guardianEmail { get; set; }="";
    public string? guardianPhone { get; set; } = "";
    public string guardianType { get; set; } = "";
    public DateTime? guardianDOB { get; set; }
    public string guardianAddress { get; set; } = "";
}