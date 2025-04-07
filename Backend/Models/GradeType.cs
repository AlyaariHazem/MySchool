using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models;

public class GradeType
{
    public int GradeTypeID { get; set; }
    public string Name { get; set; } // e.g., "Homework", "Quiz", "Midterm", etc.
    public int? MaxGrade { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public ICollection<MonthlyGrade> MonthlyGrades { get; set; }
}

