using System;
using System.Text.Json.Serialization;

namespace Backend.Models;

public class Attendance
{
    public Guid AttendanceId { get; set; }

    public int StudentID { get; set; }

    [JsonIgnore]
    public Student Student { get; set; } = null!;

    public int ClassID { get; set; }

    [JsonIgnore]
    public Class Class { get; set; } = null!;

    public DateOnly AttendanceDate { get; set; }

    public AttendanceStatus Status { get; set; }

    public string? Remarks { get; set; }

    /// <summary>Application user id (admin DB) of the staff who recorded this row.</summary>
    public string RecordedBy { get; set; } = string.Empty;

    /// <summary>Set for tenant users; null when an ADMIN acts on the admin database.</summary>
    public int? TenantId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
