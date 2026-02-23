using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class WeeklySchedule
    {
        public int WeeklyScheduleID { get; set; }
        
        // Day of week: 0 = Saturday, 1 = Sunday, 2 = Monday, 3 = Tuesday, 4 = Wednesday
        public int DayOfWeek { get; set; }
        
        // Period number: 1, 2, 3, 4, 5
        public int PeriodNumber { get; set; }
        
        // Start time for the period (e.g., "08:00")
        public string StartTime { get; set; } = string.Empty;
        
        // End time for the period (e.g., "08:45")
        public string EndTime { get; set; } = string.Empty;
        
        // Class reference
        public int ClassID { get; set; }
        [JsonIgnore]
        public Class Class { get; set; }
        
        // Term reference
        public int TermID { get; set; }
        [JsonIgnore]
        public Term Term { get; set; }
        
        // Subject reference (nullable - can be empty)
        public int? SubjectID { get; set; }
        [JsonIgnore]
        public Subject? Subject { get; set; }
        
        // Teacher reference (nullable - can be empty)
        public int? TeacherID { get; set; }
        [JsonIgnore]
        public Teacher? Teacher { get; set; }
        
        // Year reference
        public int YearID { get; set; }
        [JsonIgnore]
        public Year Year { get; set; }
        
        // Division reference (optional)
        public int? DivisionID { get; set; }
        [JsonIgnore]
        public Division? Division { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
    }
}
