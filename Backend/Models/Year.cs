using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class Year
    {
        public int YearID { get; set; }
        public DateTime YearDateStart { get; set; }
        public DateTime? YearDateEnd { get; set; }
        public DateTime HireDate { get; set; }
        public bool Active { get; set; }
        public int SchoolID { get; set; }
        [JsonIgnore]
        public School School { get; set; }
        public ICollection<YearTermMonth> YearTermMonths { get; set; }

        public virtual ICollection<Stage> Stages { get; set; }
        public ICollection<CoursePlan> CoursePlans { get; set; }
        public ICollection<MonthlyGrade> MonthlyGrades { get; set; }


    }
}