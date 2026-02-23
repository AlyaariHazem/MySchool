using System.Collections.Generic;

namespace Backend.DTOS.School.WeeklySchedule
{
    /// <summary>
    /// DTO for returning the schedule in a grid format (days x periods)
    /// </summary>
    public class WeeklyScheduleGridDTO
    {
        public int ClassID { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int TermID { get; set; }
        public string TermName { get; set; } = string.Empty;
        public int YearID { get; set; }
        
        // Grid data: List of schedule items
        public List<WeeklyScheduleDTO> ScheduleItems { get; set; } = new List<WeeklyScheduleDTO>();
        
        // Period definitions
        public List<PeriodDTO> Periods { get; set; } = new List<PeriodDTO>();
    }
    
    public class PeriodDTO
    {
        public int PeriodNumber { get; set; }
        public string PeriodName { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
    }
}
