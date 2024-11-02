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
        public DateOnly DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;

        public string? Phone { get; set; }
        [Required]
        public int GuardianID { get; set; }
        [JsonIgnore]
        public Guardian Guardian { get; set; }
        [Required]
        public int UserID { get; set; }
        [JsonIgnore]
        public User User { get; set; }
        [Required]
        public int DivisionID { get; set; }
        [JsonIgnore]
        public Division Division { get; set; }
        public virtual ICollection<TeacherStudent> TeacherStudents { get; set; }
        public virtual ICollection<SubjectStudent> SubjectStudents { get; set; }
        public virtual ICollection<TeacherSubjectStudent> TeacherSubjectStudents { get; set; }
        public virtual ICollection<StudentClass> StudentClass { get; set; }
    }
}