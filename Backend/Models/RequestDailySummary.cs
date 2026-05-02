using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Daily summary note for request progress.</summary>
public class RequestDailySummary
{
    public int RequestDailySummaryID { get; set; }

    [Required]
    public int EmployeeRequestID { get; set; }

    [JsonIgnore]
    public EmployeeRequest EmployeeRequest { get; set; } = null!;

    public DateTime SummaryDate { get; set; } = DateTime.UtcNow.Date;

    [Required]
    [MaxLength(4000)]
    public string Summary { get; set; } = string.Empty;

    public int? ProgressPercent { get; set; }

    public bool IsFinalForDay { get; set; }

    public int? CreatedByEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile? CreatedByEmployeeProfile { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
