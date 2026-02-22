using System;

namespace Backend.DTOS.School.Students;

public class UnregisteredStudentDTO
{
    public int StudentID { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? CurrentClassName { get; set; }
    public string? CurrentStageName { get; set; }
    public string? CurrentDivisionName { get; set; }
    public int CurrentDivisionID { get; set; }
    public int? CurrentYearID { get; set; }
}

public class PromoteStudentRequestDTO
{
    public int StudentID { get; set; }
    public int NewDivisionID { get; set; }
}

public class PromoteStudentsRequestDTO
{
    public List<PromoteStudentRequestDTO> Students { get; set; } = new List<PromoteStudentRequestDTO>();
    public int? TargetYearID { get; set; } // Optional: target year for promotion
    public bool CopyCoursePlansFromCurrentYear { get; set; } = false; // If true, copy course plans from student's current year instead of active year
}

public class PromoteStudentResultDTO
{
    public int StudentID { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int? NewDivisionID { get; set; }
}

public class PromoteStudentsResponseDTO
{
    public List<PromoteStudentResultDTO> Results { get; set; } = new List<PromoteStudentResultDTO>();
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
}
