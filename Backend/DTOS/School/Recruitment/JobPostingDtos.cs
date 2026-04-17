using System.ComponentModel.DataAnnotations;
using Backend.Models;

namespace Backend.DTOS.School.Recruitment;

public class JobPostingCreateDto
{
    [Required]
    public int SchoolID { get; set; }

    public int? AcademicYearID { get; set; }

    [Required]
    public int EmployeeJobTypeID { get; set; }

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? Department { get; set; }

    [MaxLength(8000)]
    public string? Description { get; set; }

    [MaxLength(8000)]
    public string? Requirements { get; set; }

    [MaxLength(8000)]
    public string? Responsibilities { get; set; }

    [MaxLength(64)]
    public string? EmploymentType { get; set; }

    [Range(1, 1000)]
    public int NumberOfOpenings { get; set; } = 1;

    public DateTime PostingDate { get; set; } = DateTime.UtcNow;

    public DateTime? ClosingDate { get; set; }

    public JobPostingStatus Status { get; set; } = JobPostingStatus.Draft;

    [MaxLength(4000)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;
}

public class JobPostingUpdateDto : JobPostingCreateDto
{
}

public class JobPostingReadDto
{
    public int JobPostingID { get; set; }
    public int SchoolID { get; set; }
    public int? AcademicYearID { get; set; }
    public int EmployeeJobTypeID { get; set; }
    public string? JobTypeCode { get; set; }
    public string? JobTypeName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public string? Responsibilities { get; set; }
    public string? EmploymentType { get; set; }
    public int NumberOfOpenings { get; set; }
    public DateTime PostingDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public JobPostingStatus Status { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public class JobPostingListDto
{
    public int JobPostingID { get; set; }
    public int SchoolID { get; set; }
    public int? AcademicYearID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Department { get; set; }
    public JobPostingStatus Status { get; set; }
    public DateTime PostingDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public int NumberOfOpenings { get; set; }
    public string? JobTypeName { get; set; }
}
