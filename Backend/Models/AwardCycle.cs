using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

/// <summary>فترة زمنية محددة لجائزة (أسبوع، شهر، فصل، إلخ).</summary>
public class AwardCycle
{
    public int AwardCycleID { get; set; }

    [Required]
    public int AwardID { get; set; }

    [JsonIgnore]
    public Award Award { get; set; } = null!;

    [Required]
    public int AcademicYearID { get; set; }

    [JsonIgnore]
    public Year AcademicYear { get; set; } = null!;

    public int? TermID { get; set; }

    [JsonIgnore]
    public Term? Term { get; set; }

    public DateTime PeriodStartUtc { get; set; }

    public DateTime PeriodEndUtc { get; set; }

    public AwardCycleStatus Status { get; set; } = AwardCycleStatus.Draft;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public ICollection<AwardNomination> Nominations { get; set; } = new List<AwardNomination>();

    [JsonIgnore]
    public ICollection<AwardWinner> Winners { get; set; } = new List<AwardWinner>();
}
