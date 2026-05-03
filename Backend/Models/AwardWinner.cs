using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>فائز معلن لدورة جائزة (مرتبة اختيارية مثل الأول).</summary>
public class AwardWinner
{
    public int AwardWinnerID { get; set; }

    [Required]
    public int AwardCycleID { get; set; }

    [JsonIgnore]
    public AwardCycle AwardCycle { get; set; } = null!;

    [Required]
    public int StudentID { get; set; }

    [JsonIgnore]
    public Student Student { get; set; } = null!;

    /// <summary>1 = المركز الأول، 2 = الثاني، إلخ.</summary>
    public int Rank { get; set; } = 1;

    public int? SelectedByEmployeeProfileID { get; set; }

    [JsonIgnore]
    public EmployeeProfile? SelectedByEmployeeProfile { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public DateTime SelectedAtUtc { get; set; } = DateTime.UtcNow;
}
