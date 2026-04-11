namespace Backend.DTOS.School.Exams;

public class ExamTypeDto
{
    public int ExamTypeID { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public class ExamSessionDto
{
    public int ExamSessionID { get; set; }
    public int YearID { get; set; }
    public int TermID { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreateExamSessionDto
{
    public int YearID { get; set; }
    public int TermID { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class UpdateExamSessionDto
{
    public int ExamSessionID { get; set; }
    public int YearID { get; set; }
    public int TermID { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class ScheduledExamListDto
{
    public int ScheduledExamID { get; set; }
    public int? ExamSessionID { get; set; }
    public int ExamTypeID { get; set; }
    public string? ExamTypeName { get; set; }
    public int YearID { get; set; }
    public int TermID { get; set; }
    public int ClassID { get; set; }
    public string? ClassName { get; set; }
    public int DivisionID { get; set; }
    public string? DivisionName { get; set; }
    public int SubjectID { get; set; }
    public string? SubjectName { get; set; }
    public int TeacherID { get; set; }
    public string? TeacherName { get; set; }
    public DateTime ExamDate { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string? Room { get; set; }
    public decimal TotalMarks { get; set; }
    public decimal PassingMarks { get; set; }
    public bool SchedulePublished { get; set; }
    public bool ResultsPublished { get; set; }
    public string? Notes { get; set; }
}

public class CreateScheduledExamDto
{
    public int? ExamSessionID { get; set; }
    public int ExamTypeID { get; set; }
    public int YearID { get; set; }
    public int TermID { get; set; }
    public int ClassID { get; set; }
    public int DivisionID { get; set; }
    public int SubjectID { get; set; }
    public int TeacherID { get; set; }
    public DateTime ExamDate { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string? Room { get; set; }
    public decimal TotalMarks { get; set; }
    public decimal PassingMarks { get; set; }
    public bool SchedulePublished { get; set; }
    public bool ResultsPublished { get; set; }
    public string? Notes { get; set; }
}

public class UpdateScheduledExamDto : CreateScheduledExamDto
{
    public int ScheduledExamID { get; set; }
}

public class ExamResultRowDto
{
    public int ExamResultID { get; set; }
    public int StudentID { get; set; }
    public string? StudentName { get; set; }
    public decimal? Score { get; set; }
    public bool IsAbsent { get; set; }
    public string? Remarks { get; set; }
}

public class BulkExamResultsDto
{
    public List<ExamResultUpsertDto> Rows { get; set; } = new();
}

public class ExamResultUpsertDto
{
    public int ExamResultID { get; set; }
    public int StudentID { get; set; }
    public decimal? Score { get; set; }
    public bool IsAbsent { get; set; }
    public string? Remarks { get; set; }
}

public class StudentExamCardDto
{
    public int ScheduledExamID { get; set; }
    public string ExamTypeName { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string DivisionName { get; set; } = string.Empty;
    public DateTime ExamDate { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string? Room { get; set; }
    public bool SchedulePublished { get; set; }
    public bool ResultsPublished { get; set; }
    public decimal TotalMarks { get; set; }
    public decimal PassingMarks { get; set; }
    public decimal? Score { get; set; }
    public bool IsAbsent { get; set; }
    public string? Remarks { get; set; }
    public bool Passed { get; set; }
}

public class ClassExamSheetReportDto
{
    public int ScheduledExamID { get; set; }
    public string ExamTypeName { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string DivisionName { get; set; } = string.Empty;
    public DateTime ExamDate { get; set; }
    public decimal TotalMarks { get; set; }
    public decimal PassingMarks { get; set; }
    public List<ExamResultRowDto> Rows { get; set; } = new();
    public decimal AverageScore { get; set; }
    public int PassCount { get; set; }
    public int FailCount { get; set; }
    public int AbsentCount { get; set; }
}

public class SubjectPerformanceReportDto
{
    public int SubjectID { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public int ExamCount { get; set; }
    public decimal AverageScore { get; set; }
}

public class TopWeakStudentDto
{
    public int StudentID { get; set; }
    public string? StudentName { get; set; }
    public decimal AverageScore { get; set; }
}

public class ExamFilterQuery
{
    public int? YearID { get; set; }
    public int? TermID { get; set; }
    public int? ClassID { get; set; }
    public int? DivisionID { get; set; }
    public int? SubjectID { get; set; }
    public int? TeacherID { get; set; }
    public bool? UpcomingOnly { get; set; }
}
