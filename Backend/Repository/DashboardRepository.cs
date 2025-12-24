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

    public async Task<List<StudentEnrollmentTrendDTO>> GetStudentEnrollmentTrendAsync()
    {
        // Get all students from tenant database
        var students = await _tenantContext.Students
            .Select(s => s.UserID)
            .Distinct()
            .ToListAsync();

        if (students == null || !students.Any())
            return GetEmptyTrend();

        // Fetch user data from admin database to get HireDate
        var enrollmentYears = new Dictionary<int, int>();
        
        foreach (var userId in students)
        {
            if (string.IsNullOrEmpty(userId))
                continue;

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user != null)
            {
                var year = user.HireDate.Year;
                enrollmentYears[year] = enrollmentYears.GetValueOrDefault(year, 0) + 1;
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

