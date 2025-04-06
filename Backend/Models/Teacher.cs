using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace Backend.Models
{
  public class Teacher
  {
    [Key]
    public int TeacherID { get; set; }
    [Required]
    public Name FullName { get; set; }
    public string UserID { get; set; }
    public virtual ApplicationUser ApplicationUser { get; set; }
    [Required]
    public int ManagerID { get; set; }
    [JsonIgnore]
    public Manager Manager { get; set; }
    public virtual ICollection<Salary> Salaries { get; set; }
    public virtual ICollection<TeacherStudent> TeacherStudents { get; set; }
    public virtual ICollection<TeacherSubjectStudent> TeacherSubjectStudents { get; set; }
    public ICollection<CoursePlan> CoursePlans { get; set; }

  }
}