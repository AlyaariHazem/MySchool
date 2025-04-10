using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class Subject
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; }
        public string? SubjectReplacement { get; set; }
        public string? Note { get; set; }
        public DateTime HireDate { get; set; }= DateTime.Now;
        public virtual ICollection<SubjectStudent> SubjectStudents { get; set; }
        public virtual ICollection<TeacherSubjectStudent> TeacherSubjectStudents { get; set; }
        public ICollection<MonthlyGrade> MonthlyGrades { get; set; }
        public ICollection<Curriculum> Curriculums { get; set; }
        public ICollection<CoursePlan> CoursePlans { get; set; }
        public ICollection<TermlyGrade> TermlyGrades { get; set; }

    }
    }