using System.ComponentModel.DataAnnotations;
using Backend.Models;

namespace Backend.DTOS.School.Attendance;

public class UpdateAttendanceDTO
{
    [Required]
    public AttendanceStatus Status { get; set; }

    public string? Remarks { get; set; }

    /// <summary>Optional; use when correcting the calendar date of a record.</summary>
    public DateOnly? Date { get; set; }
}
