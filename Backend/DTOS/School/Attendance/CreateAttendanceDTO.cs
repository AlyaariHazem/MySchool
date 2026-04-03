using System.ComponentModel.DataAnnotations;
using Backend.Models;

namespace Backend.DTOS.School.Attendance;

public class CreateAttendanceDTO
{
    [Required]
    public int StudentID { get; set; }

    [Required]
    public int ClassID { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    [Required]
    public AttendanceStatus Status { get; set; }

    public string? Remarks { get; set; }
}
