using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models;

public class MonthlyGrade
{

    // Student/Subject
    public int StudentID { get; set; }
    public Student Student { get; set; }
    // Student/Subject
    public int YearID { get; set; }
    public Year Year { get; set; }
    public int TermID { get; set; }
    public Term Term { get; set; }

    public int SubjectID { get; set; }
    public Subject Subject { get; set; }

    // Link to which month (which also links to which term)
    public int MonthID { get; set; }
    public Month Month { get; set; }

    // Add Class reference
    public int ClassID { get; set; }
    public Class Class { get; set; }

    // GradeType if you want different kinds of grades (Exam, Oral, etc.)
    public int GradeTypeID { get; set; }
    public GradeType GradeType { get; set; }

    // The actual grade
    public decimal? Grade { get; set; }
}

