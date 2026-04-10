using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Common;
using Backend.Data;
using Backend.DTOS.Dashboard;
using Backend.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository;

public class DashboardRepository : IDashboardRepository
{
    private readonly TenantDbContext _tenantContext;
    private readonly DatabaseContext _masterDb;
    private readonly TenantInfo _tenantInfo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DashboardRepository(
        TenantDbContext tenantContext,
        DatabaseContext masterDb,
        TenantInfo tenantInfo,
        IHttpContextAccessor httpContextAccessor)
    {
        _tenantContext = tenantContext;
        _masterDb = masterDb;
        _tenantInfo = tenantInfo;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>Admin with no JWT tenant: aggregate metrics across all school databases.</summary>
    private bool UseMasterDashboard() =>
        string.IsNullOrEmpty(_tenantInfo.ConnectionString)
        && PlatformAdminHelper.IsPlatformAdminUnrestricted(_httpContextAccessor.HttpContext?.User);

    private async Task<TenantDbContext> CreateTenantDbForTenantIdAsync(int tenantId)
    {
        var row = await _masterDb.Tenants.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TenantId == tenantId);
        if (row == null || string.IsNullOrWhiteSpace(row.ConnectionString))
            throw new InvalidOperationException(
                $"Tenant {tenantId} was not found or has no connection string in the master database.");

        var ti = new TenantInfo { TenantId = tenantId, ConnectionString = row.ConnectionString };
        var ob = new DbContextOptionsBuilder<TenantDbContext>();
        ob.UseTenantSqlServer(row.ConnectionString);
        return new TenantDbContext(ob.Options, ti);
    }

    private async Task<List<Models.Tenant>> GetTenantsWithConnectionsAsync() =>
        await _masterDb.Tenants.AsNoTracking()
            .Where(t => !string.IsNullOrWhiteSpace(t.ConnectionString))
            .OrderBy(t => t.SchoolName)
            .ToListAsync();

    public async Task<DashboardSummaryDTO> GetDashboardSummaryAsync()
    {
        if (UseMasterDashboard())
        {
            decimal totalMoney = 0;
            var parents = 0;
            var teachers = 0;
            var students = 0;
            foreach (var tenant in await GetTenantsWithConnectionsAsync())
            {
                await using var db = await CreateTenantDbForTenantIdAsync(tenant.TenantId);
                totalMoney += await db.Vouchers.SumAsync(v => (decimal?)v.Receipt) ?? 0;
                parents += await db.Guardians.CountAsync();
                teachers += await db.Teachers.CountAsync();
                students += await db.Students.CountAsync();
            }

            return new DashboardSummaryDTO
            {
                TotalMoney = totalMoney,
                ParentsCount = parents,
                TeachersCount = teachers,
                StudentsCount = students
            };
        }

        var totalMoneyOne = await _tenantContext.Vouchers.SumAsync(v => v.Receipt);
        var parentsCount = await _tenantContext.Guardians.CountAsync();
        var teachersCount = await _tenantContext.Teachers.CountAsync();
        var studentsCount = await _tenantContext.Students.CountAsync();

        return new DashboardSummaryDTO
        {
            TotalMoney = totalMoneyOne,
            ParentsCount = parentsCount,
            TeachersCount = teachersCount,
            StudentsCount = studentsCount
        };
    }

    public async Task<List<RecentExamDTO>> GetRecentExamsAsync()
    {
        if (UseMasterDashboard())
        {
            var all = new List<RecentExamDTO>();
            var examId = 1;
            foreach (var tenant in await GetTenantsWithConnectionsAsync())
            {
                await using var db = await CreateTenantDbForTenantIdAsync(tenant.TenantId);
                var (batch, next) = await BuildRecentExamsFromTenantAsync(db, examId);
                examId = next;
                all.AddRange(batch);
            }

            return all
                .OrderByDescending(e => e.Date)
                .Take(7)
                .ToList();
        }

        var (single, _) = await BuildRecentExamsFromTenantAsync(_tenantContext, 1);
        return single;
    }

    private static async Task<(List<RecentExamDTO> Items, int NextExamId)> BuildRecentExamsFromTenantAsync(
        TenantDbContext db,
        int examIdStart)
    {
        var coursePlans = await db.CoursePlans
            .Include(cp => cp.Subject)
            .Include(cp => cp.Class)
            .Include(cp => cp.Division)
            .Include(cp => cp.Term)
            .Include(cp => cp.Year)
            .OrderByDescending(cp => cp.YearID)
            .ThenByDescending(cp => cp.TermID)
            .Take(7)
            .ToListAsync();

        if (coursePlans.Count == 0)
            return (new List<RecentExamDTO>(), examIdStart);

        var examIdSeq = examIdStart;
        var recentExams = coursePlans.Select((cp, index) => new RecentExamDTO
        {
            ExamId = examIdSeq++,
            Date = cp.Year?.YearDateStart ?? DateTime.Now.AddDays(-index),
            Time = "10:00 AM",
            DivisionName = cp.Division?.DivisionName ?? "",
            ClassName = cp.Class?.ClassName ?? "",
            SubjectName = cp.Subject?.SubjectName ?? "",
            ExamType = "C"
        }).ToList();

        return (recentExams, examIdSeq);
    }

    public async Task<List<RecentExamDTO>> GetAllExamsAsync()
    {
        if (UseMasterDashboard())
        {
            var all = new List<RecentExamDTO>();
            var examId = 1;
            foreach (var tenant in await GetTenantsWithConnectionsAsync())
            {
                await using var db = await CreateTenantDbForTenantIdAsync(tenant.TenantId);
                var (batch, next) = await BuildAllExamsFromTenantAsync(db, examId);
                examId = next;
                all.AddRange(batch);
            }

            return all
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.ExamId)
                .ToList();
        }

        var (single, _) = await BuildAllExamsFromTenantAsync(_tenantContext, 1);
        return single;
    }

    private static async Task<(List<RecentExamDTO> Items, int NextExamId)> BuildAllExamsFromTenantAsync(
        TenantDbContext db,
        int examIdStart)
    {
        var coursePlans = await db.CoursePlans
            .Include(cp => cp.Subject)
            .Include(cp => cp.Class)
            .Include(cp => cp.Division)
            .Include(cp => cp.Term)
            .Include(cp => cp.Year)
            .ToListAsync();

        coursePlans = coursePlans
            .OrderByDescending(cp => cp.Year?.YearDateStart ?? DateTime.MinValue)
            .ThenByDescending(cp => cp.TermID)
            .ThenBy(cp => cp.ClassID)
            .ThenBy(cp => cp.SubjectID)
            .ToList();

        if (coursePlans.Count == 0)
            return (new List<RecentExamDTO>(), examIdStart);

        var examIdSeq = examIdStart;
        var list = coursePlans.Select((cp, index) => new RecentExamDTO
        {
            ExamId = examIdSeq++,
            Date = cp.Year?.YearDateStart ?? DateTime.Now.AddDays(-index),
            Time = "10:00 AM",
            DivisionName = cp.Division?.DivisionName ?? "",
            ClassName = cp.Class?.ClassName ?? "",
            SubjectName = cp.Subject?.SubjectName ?? "",
            ExamType = "C"
        }).ToList();

        return (list, examIdSeq);
    }

    public async Task<List<StudentEnrollmentTrendDTO>> GetStudentEnrollmentTrendAsync()
    {
        if (UseMasterDashboard())
        {
            var merged = new Dictionary<int, int>();
            for (var y = 2025; y <= 2028; y++)
                merged[y] = 0;

            foreach (var tenant in await GetTenantsWithConnectionsAsync())
            {
                await using var db = await CreateTenantDbForTenantIdAsync(tenant.TenantId);
                var local = await BuildStudentEnrollmentTrendForTenantAsync(db);
                foreach (var row in local)
                    merged[row.Year] = merged.GetValueOrDefault(row.Year, 0) + row.StudentCount;
            }

            return merged.OrderBy(k => k.Key)
                .Select(k => new StudentEnrollmentTrendDTO { Year = k.Key, StudentCount = k.Value })
                .ToList();
        }

        return await BuildStudentEnrollmentTrendForTenantAsync(_tenantContext);
    }

    private static async Task<List<StudentEnrollmentTrendDTO>> BuildStudentEnrollmentTrendForTenantAsync(
        TenantDbContext db)
    {
        var years = await db.Years
            .Select(y => new { y.YearID, CalendarYear = y.YearDateStart.Year })
            .ToListAsync();

        if (years.Count == 0)
            return GetEmptyTrendStatic();

        var yearIdToCalendarYear = years.ToDictionary(y => y.YearID, y => y.CalendarYear);

        var studentsWithClassYear = await db.Students
            .Include(s => s.Division)
            .ThenInclude(d => d.Class)
            .ThenInclude(c => c.Year)
            .Where(s => s.Division != null && s.Division.Class != null && s.Division.Class.YearID.HasValue)
            .Select(s => new { s.StudentID, YearID = s.Division.Class.YearID!.Value })
            .ToListAsync();

        var monthlyGradeYears = await db.MonthlyGrades
            .Select(mg => new { mg.StudentID, YearID = mg.YearID })
            .Distinct()
            .ToListAsync();

        var termlyGradeYears = await db.TermlyGrades
            .Select(tg => new { tg.StudentID, YearID = tg.YearID })
            .Distinct()
            .ToListAsync();

        var allStudentYearPairs = studentsWithClassYear
            .Union(monthlyGradeYears)
            .Union(termlyGradeYears)
            .GroupBy(x => new { x.StudentID, x.YearID })
            .Select(g => new { g.Key.StudentID, g.Key.YearID })
            .ToList();

        if (allStudentYearPairs.Count == 0)
            return GetEmptyTrendStatic();

        var enrollmentYears = new Dictionary<int, int>();
        foreach (var pair in allStudentYearPairs)
        {
            if (yearIdToCalendarYear.TryGetValue(pair.YearID, out var calendarYear))
                enrollmentYears[calendarYear] = enrollmentYears.GetValueOrDefault(calendarYear, 0) + 1;
        }

        var trend = new List<StudentEnrollmentTrendDTO>();
        for (var year = 2025; year <= 2028; year++)
        {
            trend.Add(new StudentEnrollmentTrendDTO
            {
                Year = year,
                StudentCount = enrollmentYears.GetValueOrDefault(year, 0)
            });
        }

        return trend;
    }

    private static List<StudentEnrollmentTrendDTO> GetEmptyTrendStatic()
    {
        var trend = new List<StudentEnrollmentTrendDTO>();
        for (var year = 2025; year <= 2028; year++)
        {
            trend.Add(new StudentEnrollmentTrendDTO
            {
                Year = year,
                StudentCount = 0
            });
        }

        return trend;
    }

    public async Task<TeacherWorkspaceDTO> GetTeacherWorkspaceAsync(int teacherId)
    {
        if (UseMasterDashboard())
        {
            foreach (var tenant in await GetTenantsWithConnectionsAsync())
            {
                await using var db = await CreateTenantDbForTenantIdAsync(tenant.TenantId);
                var hasWork =
                    await db.CoursePlans.AsNoTracking().AnyAsync(cp => cp.TeacherID == teacherId)
                    || await db.Classes.AsNoTracking().AnyAsync(c => c.TeacherID == teacherId);
                if (hasWork)
                    return await BuildTeacherWorkspaceForTenantAsync(db, teacherId);
            }

            return new TeacherWorkspaceDTO();
        }

        return await BuildTeacherWorkspaceForTenantAsync(_tenantContext, teacherId);
    }

    private static async Task<TeacherWorkspaceDTO> BuildTeacherWorkspaceForTenantAsync(
        TenantDbContext db,
        int teacherId)
    {
        var classIdsFromPlans = await db.CoursePlans
            .AsNoTracking()
            .Where(cp => cp.TeacherID == teacherId)
            .Select(cp => cp.ClassID)
            .Distinct()
            .ToListAsync();

        var classIdsHomeroom = await db.Classes
            .AsNoTracking()
            .Where(c => c.TeacherID == teacherId)
            .Select(c => c.ClassID)
            .Distinct()
            .ToListAsync();

        var classIds = classIdsFromPlans.Union(classIdsHomeroom).ToHashSet();

        var subjectCount = await db.CoursePlans
            .AsNoTracking()
            .Where(cp => cp.TeacherID == teacherId)
            .Select(cp => cp.SubjectID)
            .Distinct()
            .CountAsync();

        var studentCount = 0;
        if (classIds.Count > 0)
        {
            studentCount = await (
                from s in db.Students.AsNoTracking()
                join d in db.Divisions.AsNoTracking() on s.DivisionID equals d.DivisionID
                where classIds.Contains(d.ClassID)
                select s.StudentID
            ).Distinct().CountAsync();
        }

        var examId = 1;
        var coursePlans = await db.CoursePlans
            .AsNoTracking()
            .Where(cp => cp.TeacherID == teacherId)
            .Include(cp => cp.Subject)
            .Include(cp => cp.Class)
            .Include(cp => cp.Division)
            .Include(cp => cp.Term)
            .Include(cp => cp.Year)
            .OrderByDescending(cp => cp.YearID)
            .ThenByDescending(cp => cp.TermID)
            .Take(10)
            .ToListAsync();

        var recent = coursePlans.Select((cp, index) => new RecentExamDTO
        {
            ExamId = examId++,
            Date = cp.Year?.YearDateStart ?? DateTime.UtcNow.AddDays(-index),
            Time = "10:00 AM",
            DivisionName = cp.Division?.DivisionName ?? "",
            ClassName = cp.Class?.ClassName ?? "",
            SubjectName = cp.Subject?.SubjectName ?? "",
            ExamType = "C"
        }).ToList();

        return new TeacherWorkspaceDTO
        {
            Summary = new TeacherWorkspaceSummaryDTO
            {
                ClassCount = classIds.Count,
                StudentCount = studentCount,
                SubjectCount = subjectCount
            },
            RecentCoursePlans = recent
        };
    }

    public async Task<TeacherWorkspaceDTO> GetSchoolTeachingWorkspaceAsync()
    {
        if (UseMasterDashboard())
        {
            var classCount = 0;
            var studentCount = 0;
            var subjectIds = new HashSet<int>();
            var allRecent = new List<RecentExamDTO>();
            var examId = 1;

            foreach (var tenant in await GetTenantsWithConnectionsAsync())
            {
                await using var db = await CreateTenantDbForTenantIdAsync(tenant.TenantId);
                classCount += await db.Classes.AsNoTracking().CountAsync(c => c.State);
                studentCount += await db.Students.AsNoTracking().CountAsync();
                var subs = await db.CoursePlans.AsNoTracking()
                    .Select(cp => cp.SubjectID)
                    .Distinct()
                    .ToListAsync();
                foreach (var s in subs)
                    subjectIds.Add(s);

                var coursePlans = await db.CoursePlans
                    .AsNoTracking()
                    .Include(cp => cp.Subject)
                    .Include(cp => cp.Class)
                    .Include(cp => cp.Division)
                    .Include(cp => cp.Term)
                    .Include(cp => cp.Year)
                    .OrderByDescending(cp => cp.YearID)
                    .ThenByDescending(cp => cp.TermID)
                    .Take(10)
                    .ToListAsync();

                allRecent.AddRange(coursePlans.Select((cp, index) => new RecentExamDTO
                {
                    ExamId = examId++,
                    Date = cp.Year?.YearDateStart ?? DateTime.UtcNow.AddDays(-index),
                    Time = "10:00 AM",
                    DivisionName = cp.Division?.DivisionName ?? "",
                    ClassName = cp.Class?.ClassName ?? "",
                    SubjectName = cp.Subject?.SubjectName ?? "",
                    ExamType = "C"
                }));
            }

            return new TeacherWorkspaceDTO
            {
                Summary = new TeacherWorkspaceSummaryDTO
                {
                    ClassCount = classCount,
                    StudentCount = studentCount,
                    SubjectCount = subjectIds.Count
                },
                RecentCoursePlans = allRecent
                    .OrderByDescending(r => r.Date)
                    .Take(10)
                    .ToList()
            };
        }

        var classCnt = await _tenantContext.Classes.AsNoTracking().CountAsync(c => c.State);
        var studentCnt = await _tenantContext.Students.AsNoTracking().CountAsync();
        var subjectCnt = await _tenantContext.CoursePlans.AsNoTracking()
            .Select(cp => cp.SubjectID)
            .Distinct()
            .CountAsync();

        var plans = await _tenantContext.CoursePlans
            .AsNoTracking()
            .Include(cp => cp.Subject)
            .Include(cp => cp.Class)
            .Include(cp => cp.Division)
            .Include(cp => cp.Term)
            .Include(cp => cp.Year)
            .OrderByDescending(cp => cp.YearID)
            .ThenByDescending(cp => cp.TermID)
            .Take(10)
            .ToListAsync();

        var idSeq = 1;
        var recent = plans.Select((cp, index) => new RecentExamDTO
        {
            ExamId = idSeq++,
            Date = cp.Year?.YearDateStart ?? DateTime.UtcNow.AddDays(-index),
            Time = "10:00 AM",
            DivisionName = cp.Division?.DivisionName ?? "",
            ClassName = cp.Class?.ClassName ?? "",
            SubjectName = cp.Subject?.SubjectName ?? "",
            ExamType = "C"
        }).ToList();

        return new TeacherWorkspaceDTO
        {
            Summary = new TeacherWorkspaceSummaryDTO
            {
                ClassCount = classCnt,
                StudentCount = studentCnt,
                SubjectCount = subjectCnt
            },
            RecentCoursePlans = recent
        };
    }
    public async Task<StudentDashboardDTO?> GetStudentDashboardAsync(string userId)
    {
        if (UseMasterDashboard() || string.IsNullOrEmpty(userId))
            return null;

        var student = await _tenantContext.Students
            .AsNoTracking()
            .Include(s => s.Division!)
                .ThenInclude(d => d.Class!)
                    .ThenInclude(c => c.Stage!)
            .Include(s => s.Division!)
                .ThenInclude(d => d.Class!)
                    .ThenInclude(c => c.Year!)
            .FirstOrDefaultAsync(s => s.UserID == userId);

        if (student == null)
            return null;

        var fn = student.FullName;
        var displayName = string.Join(" ",
            new[] { fn?.FirstName, fn?.MiddleName, fn?.LastName }.Where(p => !string.IsNullOrWhiteSpace(p))).Trim();

        var y = student.Division?.Class?.Year;
        var academicYearLabel = y != null
            ? y.YearDateStart.ToString("yyyy")
            : (student.Division?.Class?.ClassYear ?? string.Empty);

        return new StudentDashboardDTO
        {
            StudentId = student.StudentID,
            DisplayName = displayName,
            ClassName = student.Division?.Class?.ClassName ?? string.Empty,
            StageName = student.Division?.Class?.Stage?.StageName ?? string.Empty,
            DivisionName = student.Division?.DivisionName ?? string.Empty,
            AcademicYearLabel = academicYearLabel
        };
    }
}
