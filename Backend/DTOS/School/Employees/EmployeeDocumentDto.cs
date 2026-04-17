using System.ComponentModel.DataAnnotations;

namespace Backend.DTOS.School.Employees;

public class EmployeeDocumentDto
{
    public int? EmployeeDocumentID { get; set; }

    [Required]
    [MaxLength(128)]
    public string DocumentType { get; set; } = string.Empty;

    [Required]
    [MaxLength(512)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(512)]
    public string? FileName { get; set; }

    [MaxLength(2048)]
    public string? FileUrl { get; set; }

    public DateTime? UploadedAtUtc { get; set; }

    public DateTime? ExpiryDate { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;
}
