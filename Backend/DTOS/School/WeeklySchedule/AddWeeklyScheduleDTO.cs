using System.ComponentModel.DataAnnotations;

namespace Backend.DTOS.School.WeeklySchedule
{
    public class AddWeeklyScheduleDTO
    {
        [Required]
        public int DayOfWeek { get; set; } // 0 = Saturday, 1 = Sunday, etc.
        
        [Required]
        [Range(1, 10)]
        public int PeriodNumber { get; set; }
        
        [Required]
        public string StartTime { get; set; } = string.Empty; // Format: "HH:mm"
        
        [Required]
        public string EndTime { get; set; } = string.Empty; // Format: "HH:mm"
        
        [Required]
        public int ClassID { get; set; }
        
        [Required]
        public int TermID { get; set; }
        
        public int? SubjectID { get; set; } // Nullable - can be empty
        
        public int? TeacherID { get; set; } // Nullable - can be empty
        
        [Required]
        public int YearID { get; set; }
        
        public int? DivisionID { get; set; } // Optional
    }
}
