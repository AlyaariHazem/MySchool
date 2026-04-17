using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

public class EmployeeHistory
{
    public int EmployeeHistoryID { get; set; }

    [Required]
    public int EmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile EmployeeProfile { get; set; } = null!;

    [Required]
    public int AcademicYearID { get; set; }

    [JsonIgnore]
    public Year AcademicYear { get; set; } = null!;

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    public int? EmployeeJobTypeID { get; set; }

    [JsonIgnore]
    public EmployeeJobType? JobType { get; set; }

    [MaxLength(256)]
    public string? JobTitle { get; set; }

    [MaxLength(256)]
    public string? Department { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [MaxLength(64)]
    public string? Status { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
