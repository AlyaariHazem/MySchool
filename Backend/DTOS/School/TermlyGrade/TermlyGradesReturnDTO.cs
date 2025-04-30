using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.TermlyGrade;

public class TermlyGradesReturnDTO
{
    public int? TermlyGradeID { get; set; }
    public int StudentID { get; set; }
    public string? StudentName { get; set; }
    public string? StudentURL { get; set; }
    public string? Note { get; set; }
    public decimal? Grade { get; set; }
    public int TermID { get; set; }
    public int SubjectID { get; set; }
    public string? SubjectName { get; set; }
}
