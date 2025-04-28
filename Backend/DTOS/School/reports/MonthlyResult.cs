using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.reports;

public class MonthlyResult
{
    public int    StudentID   { get; set; }
    public string StudentName { get; set; }

    public string SchoolName  { get; set; }
    public string? SchoolURL  { get; set; }

    public string? Year   { get; set; }
    public string? Month  { get; set; }
    public string? Term   { get; set; }
    public string? Class  { get; set; }
    public string? Division  { get; set; }
    public string? Teacher   { get; set; }
    public decimal? Grade    { get; set; }
    public List<GradeSubject>? gradeSubjects  { get; set; }
}
public class GradeSubject
{
    public int SubjectID { get; set; }
    public string SubjectName { get; set; }
    public decimal? Grade { get; set; }
}