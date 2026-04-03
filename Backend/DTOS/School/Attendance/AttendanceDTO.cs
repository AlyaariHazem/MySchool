using Backend.Models;

namespace Backend.DTOS.School.Attendance;

public class AttendanceDTO
{
    public Guid AttendanceId { get; set; }
    public int StudentID { get; set; }
    public string? StudentName { get; set; }
    public int ClassID { get; set; }
    public string? ClassName { get; set; }
    public DateOnly Date { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Remarks { get; set; }
    public string RecordedBy { get; set; } = string.Empty;
    public int? TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
