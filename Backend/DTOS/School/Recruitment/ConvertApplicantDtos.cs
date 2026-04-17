using System.ComponentModel.DataAnnotations;
using Backend.DTOS.School.Employees;
using Backend.Models;

namespace Backend.DTOS.School.Recruitment;

public class ConvertApplicantToEmployeeDto
{
    /// <summary>Optional override; default is generated (e.g. HIRE-{applicationId}).</summary>
    [MaxLength(64)]
    public string? EmployeeCode { get; set; }

    [MaxLength(450)]
    public string? UserId { get; set; }

    /// <summary>Defaults to offer job type from hiring decision or posting.</summary>
    public int? EmployeeJobTypeID { get; set; }

    public DateTime? HireDate { get; set; }

    public EmploymentStatus EmploymentStatus { get; set; } = EmploymentStatus.Active;

    [MaxLength(4000)]
    public string? Notes { get; set; }

    /// <summary>When true, map HighestQualification / Specialization into HR child rows.</summary>
    public bool MapQualificationAndSpecialization { get; set; } = true;
}

public class ConvertApplicantToEmployeeResultDto
{
    public int EmployeeProfileID { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public int JobApplicationID { get; set; }
}
