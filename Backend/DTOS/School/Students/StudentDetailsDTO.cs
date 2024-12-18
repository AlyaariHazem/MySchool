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
    public string? PlaceBirth { get; set; }
    public string? StudentAddress { get; set; }
    public string? StudentPhone { get; set; }
    public DateTime StudnetDOB { get; set; }
    public int DivisionID { get; set; }
    public string UserID { get; set; }
    public ApplicationUserDTO ApplicationUser { get; set; }
    public GuardianDto Guardians { get; set; }
}
