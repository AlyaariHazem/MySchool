namespace Backend.DTOS.School.WeeklySchedule;

public class GenerateWeeklyScheduleResultDTO
{
    public bool Success { get; set; }
    public int PlacedPeriods { get; set; }
    public int RequiredPeriods { get; set; }
    public int GridSlots { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> UnplacedLines { get; set; } = new();
}
