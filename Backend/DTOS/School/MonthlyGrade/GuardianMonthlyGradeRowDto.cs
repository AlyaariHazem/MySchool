using Backend.DTOS.School.GradeTypes;

namespace Backend.DTOS.School.MonthlyGrade;

/// <summary>One row per student + subject + month (aggregated grade types), for guardian monthly report.</summary>
public class GuardianMonthlyGradeRowDto
{
    public int StudentID { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int YearID { get; set; }
    public int TermID { get; set; }
    public string? TermName { get; set; }
    public int MonthID { get; set; }
    public string? MonthName { get; set; }
    public int ClassID { get; set; }
    public string? ClassName { get; set; }
    public int SubjectID { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public List<GradeTypeMonthDTO> Grades { get; set; } = new();
}
