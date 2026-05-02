using System.ComponentModel.DataAnnotations;

namespace Backend.DTOS.School.Meeting;

public class MeetingFilterDto
{
    public int? SchoolID { get; set; }
    public int? AcademicYearID { get; set; }
    public int? Status { get; set; }
}

public class MeetingListItemDto
{
    public int MeetingID { get; set; }
    public int SchoolID { get; set; }
    public int AcademicYearID { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateTime StartAtUtc { get; set; }
    public DateTime? EndAtUtc { get; set; }
    public int OrganizerEmployeeProfileID { get; set; }
    public string OrganizerName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public int AttendeeCount { get; set; }
}

public class MeetingAttendeeReadDto
{
    public int MeetingAttendeeID { get; set; }
    public int EmployeeProfileID { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Role { get; set; }
    public int Response { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public class MeetingMinutesReadDto
{
    public int MeetingMinutesID { get; set; }
    public int MeetingID { get; set; }
    public string Body { get; set; } = string.Empty;
    public int RecordedByEmployeeProfileID { get; set; }
    public string RecordedByName { get; set; } = string.Empty;
    public DateTime RecordedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public int? ApprovedByEmployeeProfileID { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
}

public class MeetingTaskFollowUpReadDto
{
    public int MeetingTaskFollowUpID { get; set; }
    public int MeetingTaskID { get; set; }
    public string Note { get; set; } = string.Empty;
    public int? ProgressPercent { get; set; }
    public int? AuthorEmployeeProfileID { get; set; }
    public string? AuthorName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class MeetingTaskReadDto
{
    public int MeetingTaskID { get; set; }
    public int MeetingID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Details { get; set; }
    public int? AssignedToEmployeeProfileID { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime? DueAtUtc { get; set; }
    public int Status { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public List<MeetingTaskFollowUpReadDto> FollowUps { get; set; } = new();
}

public class MeetingDetailDto : MeetingListItemDto
{
    public string? Description { get; set; }
    public string? Location { get; set; }
    public List<MeetingAttendeeReadDto> Attendees { get; set; } = new();
    public MeetingMinutesReadDto? Minutes { get; set; }
    public List<MeetingTaskReadDto> Tasks { get; set; } = new();
}

public class MeetingAttendeeWriteDto
{
    [Required]
    public int EmployeeProfileID { get; set; }

    public int Role { get; set; }

    public int Response { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}

public class MeetingWriteDto
{
    [Required]
    public int SchoolID { get; set; }

    public int? AcademicYearID { get; set; }

    [Required]
    public int OrganizerEmployeeProfileID { get; set; }

    [Required]
    [MaxLength(512)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; set; }

    [MaxLength(256)]
    public string? Location { get; set; }

    [Required]
    public DateTime StartAtUtc { get; set; }

    public DateTime? EndAtUtc { get; set; }

    [Required]
    public int Status { get; set; }

    public List<MeetingAttendeeWriteDto> Attendees { get; set; } = new();
}

public class MeetingMinutesWriteDto
{
    [Required]
    public string Body { get; set; } = string.Empty;

    [Required]
    public int RecordedByEmployeeProfileID { get; set; }

    public int? ApprovedByEmployeeProfileID { get; set; }

    public DateTime? ApprovedAtUtc { get; set; }
}

public class MeetingTaskFollowUpWriteDto
{
    [Required]
    [MaxLength(4000)]
    public string Note { get; set; } = string.Empty;

    public int? ProgressPercent { get; set; }

    public int? AuthorEmployeeProfileID { get; set; }
}

public class MeetingTaskWriteDto
{
    [Required]
    [MaxLength(512)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Details { get; set; }

    public int? AssignedToEmployeeProfileID { get; set; }

    public DateTime? DueAtUtc { get; set; }

    [Required]
    public int Status { get; set; }

    public int SortOrder { get; set; }

    public List<MeetingTaskFollowUpWriteDto> FollowUps { get; set; } = new();
}
