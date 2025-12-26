using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.DTOS.Dashboard;
using Backend.Interfaces;
using Backend.Models;
using Backend.Repository.School.Implements;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository;

public class DashboardRepository : IDashboardRepository
{
    private readonly TenantDbContext _tenantContext;
    private readonly IUserRepository _userRepository;

    public DashboardRepository(TenantDbContext tenantContext, IUserRepository userRepository)
    {
        _tenantContext = tenantContext;
        _userRepository = userRepository;
    }

    public async Task<DashboardSummaryDTO> GetDashboardSummaryAsync()
    {
        // Get total money from vouchers
        var totalMoney = await _tenantContext.Vouchers
            .SumAsync(v => v.Receipt);

        // Get counts directly from database
        var parentsCount = await _tenantContext.Guardians.CountAsync();
        var teachersCount = await _tenantContext.Teachers.CountAsync();
        var studentsCount = await _tenantContext.Students.CountAsync();

        return new DashboardSummaryDTO
        {
            TotalMoney = totalMoney,
            ParentsCount = parentsCount,
            TeachersCount = teachersCount,
            StudentsCount = studentsCount
        };
    }

    public async Task<List<RecentExamDTO>> GetRecentExamsAsync()
    {
        // Get recent course plans with all related data
        var coursePlans = await _tenantContext.CoursePlans
            .Include(cp => cp.Subject)
            .Include(cp => cp.Class)
            .Include(cp => cp.Division)
            .Include(cp => cp.Term)
            .Include(cp => cp.Year)
            .OrderByDescending(cp => cp.YearID)
            .ThenByDescending(cp => cp.TermID)
            .Take(7)
            .ToListAsync();

        if (coursePlans == null || !coursePlans.Any())
            return new List<RecentExamDTO>();

        var recentExams = coursePlans.Select((cp, index) => new RecentExamDTO
        {
            ExamId = cp.YearID * 10000 + cp.TermID * 1000 + cp.ClassID * 100 + cp.SubjectID + index,
            Date = cp.Year?.YearDateStart ?? DateTime.Now.AddDays(-index),
            Time = "10:00 AM", // Default time - adjust if you have exam time in your model
            DivisionName = cp.Division?.DivisionName ?? "",
            ClassName = cp.Class?.ClassName ?? "",
            SubjectName = cp.Subject?.SubjectName ?? "",
            ExamType = "C" // Default exam type - adjust based on your model
        }).ToList();

        return recentExams;
    }

    public async Task<List<RecentExamDTO>> GetAllExamsAsync()
    {
        // Get all course plans (exams) with all related data
        var coursePlans = await _tenantContext.CoursePlans
            .Include(cp => cp.Subject)
            .Include(cp => cp.Class)
            .Include(cp => cp.Division)
            .Include(cp => cp.Term)
            .Include(cp => cp.Year)
            .ToListAsync();

        // Order in memory to avoid null propagating operator in expression tree
        coursePlans = coursePlans
            .OrderByDescending(cp => cp.Year?.YearDateStart ?? DateTime.MinValue)
            .ThenByDescending(cp => cp.TermID)
            .ThenBy(cp => cp.ClassID)
            .ThenBy(cp => cp.SubjectID)
            .ToList();

        if (coursePlans == null || !coursePlans.Any())
            return new List<RecentExamDTO>();

        var exams = coursePlans.Select((cp, index) => new RecentExamDTO
        {
            ExamId = cp.YearID * 10000 + cp.TermID * 1000 + cp.ClassID * 100 + cp.SubjectID + index,
            Date = cp.Year?.YearDateStart ?? DateTime.Now.AddDays(-index),
            Time = "10:00 AM", // Default time - adjust if you have exam time in your model
            DivisionName = cp.Division?.DivisionName ?? "",
            ClassName = cp.Class?.ClassName ?? "",
            SubjectName = cp.Subject?.SubjectName ?? "",
            ExamType = "C" // Default exam type - adjust based on your model
        }).ToList();

        return exams;
    }

    public async Task<List<StudentEnrollmentTrendDTO>> GetStudentEnrollmentTrendAsync()
    {
        // Get all years first to map YearID to calendar year
        var years = await _tenantContext.Years
            .Select(y => new { y.YearID, CalendarYear = y.YearDateStart.Year })
            .ToListAsync();

        if (!years.Any())
            return GetEmptyTrend();

        // Create a dictionary to map YearID to CalendarYear
        var yearIdToCalendarYear = years.ToDictionary(y => y.YearID, y => y.CalendarYear);

        // Get students with their current class year (Student -> Division -> Class -> Year)
        var studentsWithClassYear = await _tenantContext.Students
            .Include(s => s.Division)
                .ThenInclude(d => d.Class)
                    .ThenInclude(c => c.Year)
            .Where(s => s.Division != null && s.Division.Class != null && s.Division.Class.YearID.HasValue)
            .Select(s => new { s.StudentID, YearID = s.Division.Class.YearID!.Value })
            .ToListAsync();

        // Get distinct student-year combinations from MonthlyGrades (for historical data)
        var monthlyGradeYears = await _tenantContext.MonthlyGrades
            .Select(mg => new { mg.StudentID, YearID = mg.YearID })
            .Distinct()
            .ToListAsync();

        // Get distinct student-year combinations from TermlyGrades (for historical data)
        var termlyGradeYears = await _tenantContext.TermlyGrades
            .Select(tg => new { tg.StudentID, YearID = tg.YearID })
            .Distinct()
            .ToListAsync();

        // Combine all sources: current class year + historical grade years
        // This ensures we count students in their current year even if they don't have grades yet
        var allStudentYearPairs = studentsWithClassYear
            .Union(monthlyGradeYears)
            .Union(termlyGradeYears)
            .GroupBy(x => new { x.StudentID, x.YearID })
            .Select(g => new { g.Key.StudentID, g.Key.YearID })
            .ToList();

        if (!allStudentYearPairs.Any())
            return GetEmptyTrend();

        // Count students by calendar year (mapping YearID to CalendarYear)
        var enrollmentYears = new Dictionary<int, int>();
        
        foreach (var pair in allStudentYearPairs)
        {
            if (yearIdToCalendarYear.TryGetValue(pair.YearID, out int calendarYear))
            {
                enrollmentYears[calendarYear] = enrollmentYears.GetValueOrDefault(calendarYear, 0) + 1;
            }
        }

        // Generate trend for years 2025-2028
        var trend = new List<StudentEnrollmentTrendDTO>();
        for (int year = 2025; year <= 2028; year++)
        {
            trend.Add(new StudentEnrollmentTrendDTO
            {
                Year = year,
                StudentCount = enrollmentYears.GetValueOrDefault(year, 0)
            });
        }

        return trend;
    }

    private List<StudentEnrollmentTrendDTO> GetEmptyTrend()
    {
        var trend = new List<StudentEnrollmentTrendDTO>();
        for (int year = 2025; year <= 2028; year++)
        {
            trend.Add(new StudentEnrollmentTrendDTO
            {
                Year = year,
                StudentCount = 0
            });
        }
        return trend;
    }
}

