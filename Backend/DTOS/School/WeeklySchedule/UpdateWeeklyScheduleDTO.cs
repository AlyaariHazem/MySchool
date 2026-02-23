using System.ComponentModel.DataAnnotations;

namespace Backend.DTOS.School.WeeklySchedule
{
    public class UpdateWeeklyScheduleDTO
    {
        public int WeeklyScheduleID { get; set; }
        
        [Required]
        public int DayOfWeek { get; set; }
        
        [Required]
        [Range(1, 10)]
        public int PeriodNumber { get; set; }
        
        [Required]
        public string StartTime { get; set; } = string.Empty;
        
        [Required]
        public string EndTime { get; set; } = string.Empty;
        
        [Required]
        public int ClassID { get; set; }
        
        [Required]
        public int TermID { get; set; }
        
        public int? SubjectID { get; set; }
        
        public int? TeacherID { get; set; }
        
        [Required]
        public int YearID { get; set; }
        
        public int? DivisionID { get; set; }
    }
}
