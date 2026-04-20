using System.ComponentModel.DataAnnotations;

namespace Backend.DTOS.School.WeeklySchedule;

public class GenerateWeeklyScheduleRequestDTO
{
    [Required]
    public int ClassID { get; set; }

    [Required]
    public int TermID { get; set; }

    public int? DivisionID { get; set; }

    [Range(1, 7)]
    public int DaysPerWeek { get; set; } = 5;

    [Range(1, 14)]
    public int PeriodsPerDay { get; set; } = 6;

    /// <summary>Do not assign the same subject more than this many times on one calendar day (0 = no limit).</summary>
    [Range(0, 10)]
    public int MaxSameSubjectPerDay { get; set; } = 2;

    /// <summary>Optional seed for shuffling placement attempts.</summary>
    public int? RandomSeed { get; set; }
}
