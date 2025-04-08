using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class Student
    {
        public int StudentID { get; set; }
        [Required]
        public Name FullName { get; set; }
        public NameAlis? FullNameAlis { get; set; }
        public string? ImageURL { get; set; }
        public string? PlaceBirth { get; set; }
        public DateTime? StudentDOB { get; set; }=DateTime.Now;
        [Required]
        public int DivisionID { get; set; }
        public int GuardianID { get; set; } // Foreign Key
        public Guardian Guardian { get; set; } // Navigation Property
        [JsonIgnore]
        public Division Division { get; set; }
        public string UserID { get; set; }
        public ICollection<MonthlyGrade> MonthlyGrades { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual ICollection<Attachments> Attachments { get; set; }
        public virtual ICollection<TeacherStudent> TeacherStudents { get; set; }
        public virtual ICollection<SubjectStudent> SubjectStudents { get; set; }
        public ICollection<TermlyGrade> TermlyGrades { get; set; }
        public virtual ICollection<TeacherSubjectStudent> TeacherSubjectStudents { get; set; }
        public virtual ICollection<AccountStudentGuardian> AccountStudentGuardians { get; set; }
        public virtual ICollection<StudentClassFees> StudentClassFees { get; set; }
       
    }
}