namespace Backend.DTOS.School.CoursePlan;

/// <summary>Composite key for a course plan (same as PK: year, teacher, class, division, subject, term).</summary>
public class CoursePlanKeyDTO
{
    public int YearID { get; set; }
    public int TeacherID { get; set; }
    public int ClassID { get; set; }
    public int DivisionID { get; set; }
    public int SubjectID { get; set; }
    public int TermID { get; set; }
}
