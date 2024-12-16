using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.DTOS.School.Students;

public class StudentDetailsDTO
{
    public int StudentID { get; set; }
    public NameDTO FullName { get; set; }
    public NameAlisDTO? FullNameAlis { get; set; }
    public int DivisionID { get; set; }
    public string? DivisionName { get; set; }
    public string? ClassName { get; set; }
    public string? StageName { get; set; }
    public string? PhotoUrl { get; set; }
    public int? Age { get; set; }
    public decimal? Fee { get; set; }
    public string? Gender { get; set; }
    public string? PlaceBirth { get; set; }
    public string? StudentPhone { get; set; }
    public DateTime? HireDate { get; set; }
    public string? StudentAddress { get; set; }
    public string? UserID { get; set; }
    public ApplicationUserDTO ApplicationUser { get; set; }
    public GuardianDto Guardians { get; set; }
}
