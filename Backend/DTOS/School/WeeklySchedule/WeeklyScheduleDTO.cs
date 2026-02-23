using System;

namespace Backend.DTOS.School.WeeklySchedule
{
    public class WeeklyScheduleDTO
    {
        public int WeeklyScheduleID { get; set; }
        public int DayOfWeek { get; set; }
        public string DayName { get; set; } = string.Empty; // Arabic day name
        public int PeriodNumber { get; set; }
        public string PeriodName { get; set; } = string.Empty; // Arabic period name
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public int ClassID { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int TermID { get; set; }
        public string TermName { get; set; } = string.Empty;
        public int? SubjectID { get; set; }
        public string? SubjectName { get; set; }
        public int? TeacherID { get; set; }
        public string? TeacherName { get; set; }
        public int YearID { get; set; }
        public int? DivisionID { get; set; }
        public string? DivisionName { get; set; }
    }
}
