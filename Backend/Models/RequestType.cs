using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>Lookup table that defines employee request types per school.</summary>
public class RequestType
{
    public int RequestTypeID { get; set; }

    [Required]
    public int SchoolID { get; set; }

    [JsonIgnore]
    public School School { get; set; } = null!;

    [Required]
    [MaxLength(64)]
    public string Code { get; set; } = string.Empty;

    [Required]
    public EmployeeRequestCategory Category { get; set; } = EmployeeRequestCategory.Support;

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? NameAr { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<EmployeeRequest> EmployeeRequests { get; set; } = new List<EmployeeRequest>();
}
