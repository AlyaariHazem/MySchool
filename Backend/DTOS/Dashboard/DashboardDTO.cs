using System;
using System.Collections.Generic;

namespace Backend.DTOS.Dashboard;

public class DashboardDTO
{
    public DashboardSummaryDTO Summary { get; set; } = new();
    public List<RecentExamDTO> RecentExams { get; set; } = new();
    public List<StudentEnrollmentTrendDTO> StudentEnrollmentTrend { get; set; } = new();
}

public class DashboardSummaryDTO
{
    public decimal TotalMoney { get; set; }
    public int ParentsCount { get; set; }
    public int TeachersCount { get; set; }
    public int StudentsCount { get; set; }
}

public class RecentExamDTO
{
    public int ExamId { get; set; }
    public DateTime Date { get; set; }
    public string Time { get; set; } = string.Empty;
    public string DivisionName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string ExamType { get; set; } = string.Empty;
}

public class StudentEnrollmentTrendDTO
{
    public int Year { get; set; }
    public int StudentCount { get; set; }
}

/// <summary>Single-screen summary for teachers: their classes, students, subjects, and upcoming course-plan items.</summary>
public class TeacherWorkspaceDTO
{
    public TeacherWorkspaceSummaryDTO Summary { get; set; } = new();
    public List<RecentExamDTO> RecentCoursePlans { get; set; } = new();
}

public class TeacherWorkspaceSummaryDTO
{
    public int ClassCount { get; set; }
    public int StudentCount { get; set; }
    public int SubjectCount { get; set; }
}

