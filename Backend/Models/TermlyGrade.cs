using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models;

public class TermlyGrade
{
    public int TermlyGradeID { get; set; }
    public int StudentID { get; set; }
    public Student Student { get; set; }
    public decimal Grade { get; set; }= 0;
    public int TermID { get; set; }
    public Term Term { get; set; }
    public int ClassID { get; set; }
    public Class Class { get; set; }
    public int SubjectID { get; set; }
    public Subject Subject { get; set; }
    public string? Note { get; set; }
}
