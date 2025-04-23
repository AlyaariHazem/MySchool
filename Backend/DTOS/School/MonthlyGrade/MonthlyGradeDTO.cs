using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.MonthlyGrade;

public class MonthlyGradeDTO
{
    public int StudentID { get; set; }
    public int SubjectID { get; set; }
    public int YearID { get; set; }
    public int MonthID { get; set; }
    public int ClassID { get; set; }
    public int GradeTypeID { get; set; }
    public decimal? Grade { get; set; }
    public int TermID { get; set; }
}
