using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Lookup for HR job classification (separate from legacy <see cref="EmployeeYearAssignment.EmployeeRole"/> strings).</summary>
public class EmployeeJobType
{
    public int EmployeeJobTypeID { get; set; }

    [Required]
    [MaxLength(64)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? NameAr { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    [JsonIgnore]
    public ICollection<EmployeeProfile> EmployeeProfiles { get; set; } = new List<EmployeeProfile>();

    [JsonIgnore]
    public ICollection<EmployeeHistory> EmployeeHistories { get; set; } = new List<EmployeeHistory>();

    [JsonIgnore]
    public ICollection<EmployeePerformanceSummary> PerformanceSummaries { get; set; } = new List<EmployeePerformanceSummary>();
}
