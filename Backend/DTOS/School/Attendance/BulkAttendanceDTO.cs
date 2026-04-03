using System.ComponentModel.DataAnnotations;
using Backend.Models;

namespace Backend.DTOS.School.Attendance;

public class BulkAttendanceEntryDTO
{
    [Required]
    public int StudentID { get; set; }

    [Required]
    public AttendanceStatus Status { get; set; }

    public string? Remarks { get; set; }
}

public class BulkAttendanceDTO
{
    [Required]
    public int ClassID { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    [Required]
    [MinLength(1)]
    public List<BulkAttendanceEntryDTO> Entries { get; set; } = new();
}
