using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Materialized KPI value snapshot for a given time window (dashboards & trends).</summary>
public class KpiSnapshot
{
    public int KpiSnapshotID { get; set; }

    [Required]
    public int KpiDefinitionID { get; set; }

    [JsonIgnore]
    public KpiDefinition KpiDefinition { get; set; } = null!;

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    public int? AcademicYearID { get; set; }

    [JsonIgnore]
    public Year? AcademicYear { get; set; }

    public int? TermID { get; set; }

    [JsonIgnore]
    public Term? Term { get; set; }

    public int? EmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile? EmployeeProfile { get; set; }

    [MaxLength(256)]
    public string? DepartmentName { get; set; }

    public AnalyticsPeriodKind PeriodKind { get; set; } = AnalyticsPeriodKind.Monthly;

    public DateTime PeriodStartUtc { get; set; }

    public DateTime PeriodEndUtc { get; set; }

    /// <summary>Business snapshot timestamp (defaults to <see cref="RecordedAtUtc"/>).</summary>
    public DateTime SnapshotDateUtc { get; set; }

    public decimal Value { get; set; }

    public decimal? TargetValue { get; set; }

    public decimal? PreviousValue { get; set; }

    public decimal? ChangePercent { get; set; }

    public KpiSnapshotStatus Status { get; set; } = KpiSnapshotStatus.Ok;

    public DateTime RecordedAtUtc { get; set; } = DateTime.UtcNow;

    [MaxLength(4000)]
    public string? Notes { get; set; }
}
