using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

public class EmployeeSpecialization
{
    public int EmployeeSpecializationID { get; set; }

    [Required]
    public int EmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile EmployeeProfile { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? Category { get; set; }

    [MaxLength(64)]
    public string? Level { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
