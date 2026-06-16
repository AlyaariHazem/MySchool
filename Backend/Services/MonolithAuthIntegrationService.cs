using Backend.Data;
using Backend.DTOS;
using Backend.DTOS.Internal;
using Backend.Interfaces;
using Backend.Models;
using Backend.Models.Master;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public sealed class MonolithAuthIntegrationService : IMonolithAuthIntegrationService
{
    private readonly DatabaseContext _context;
    private readonly ITenantMembershipService _tenantMembership;
    private readonly ISchoolRoleResolver _schoolRoleResolver;

    public MonolithAuthIntegrationService(
        DatabaseContext context,
        ITenantMembershipService tenantMembership,
        ISchoolRoleResolver schoolRoleResolver)
    {
        _context = context;
        _tenantMembership = tenantMembership;
        _schoolRoleResolver = schoolRoleResolver;
    }

    public async Task<LoginEnrichmentResponseDto> GetLoginEnrichmentAsync(
        string userId,
        string userType,
        int? requestedTenantId = null,
        CancellationToken cancellationToken = default)
    {
        var response = new LoginEnrichmentResponseDto();

        if (string.Equals(userType, "ADMIN", StringComparison.OrdinalIgnoreCase))
            return response;

        var tenantChoices = await GetTenantSummariesAsync(userId, cancellationToken);
        response.Tenants = tenantChoices.Count > 1 ? tenantChoices : null;

        int? tenantId = null;
        TenantRole? membershipTenantRole = null;

        if (tenantChoices.Count > 0)
        {
            if (tenantChoices.Count == 1)
            {
                tenantId = tenantChoices[0].TenantId;
                membershipTenantRole = tenantChoices[0].TenantRole;
            }
            else if (requestedTenantId is { } requestedTid && tenantChoices.Any(t => t.TenantId == requestedTid))
            {
                var picked = tenantChoices.First(t => t.TenantId == requestedTid);
                tenantId = picked.TenantId;
                membershipTenantRole = picked.TenantRole;
            }
            else if (tenantChoices.Count > 1)
            {
                var pick = tenantChoices[0];
                tenantId = pick.TenantId;
                membershipTenantRole = pick.TenantRole;
            }
        }

        Tenant? tenantEntity = null;
        if (tenantId.HasValue)
        {
            tenantEntity = await _context.Tenants.AsNoTracking()
                .FirstOrDefaultAsync(t => t.TenantId == tenantId.Value, cancellationToken);
        }

        if (tenantEntity == null && tenantId.HasValue)
        {
            tenantId = null;
            membershipTenantRole = null;
        }

        if (tenantEntity == null && !tenantId.HasValue && tenantChoices.Count <= 1)
        {
            var resolvedTenantId = await ResolveTenantIdForTeacherStudentGuardianAsync(userId, userType, cancellationToken);
            if (resolvedTenantId.HasValue)
            {
                tenantEntity = await _context.Tenants.AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TenantId == resolvedTenantId.Value, cancellationToken);
            }

            if (tenantEntity != null)
            {
                tenantId = tenantEntity.TenantId;
                var roleFromType = TenantRoleFromUserType(userType);
                if (roleFromType.HasValue)
                {
                    membershipTenantRole = roleFromType;
                    await EnsureUserTenantAsync(userId, tenantEntity.TenantId, roleFromType.Value, cancellationToken);
                }
            }
        }

        dynamic? schoolData = null;
        string? managerName = null;
        int yearId = 1;
        string? userName = null;
        string? tenantDatabaseName = null;

        if (tenantEntity != null)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(tenantEntity.ConnectionString);
                tenantDatabaseName = builder.InitialCatalog;
            }
            catch
            {
                tenantDatabaseName = tenantEntity.ConnectionString;
            }

            var tenantInfo = new TenantInfo
            {
                TenantId = tenantId,
                ConnectionString = tenantEntity.ConnectionString
            };

            var tenantOptions = new DbContextOptionsBuilder<TenantDbContext>()
                .UseTenantSqlServer(tenantEntity.ConnectionString)
                .Options;

            await using var tenantContext = new TenantDbContext(tenantOptions, tenantInfo);

            if (string.Equals(userType, "MANAGER", StringComparison.OrdinalIgnoreCase))
            {
                var manager = await tenantContext.Managers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.UserID == userId, cancellationToken);

                if (manager != null)
                {
                    var school = await tenantContext.Schools
                        .AsNoTracking()
                        .Where(s => s.SchoolID == manager.SchoolID)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (school != null)
                    {
                        var activeYearId = await tenantContext.Years
                            .AsNoTracking()
                            .Where(y => y.SchoolID == school.SchoolID && y.Active == true)
                            .Select(y => (int?)y.YearID)
                            .FirstOrDefaultAsync(cancellationToken);

                        schoolData = new
                        {
                            SchoolName = school.SchoolName,
                            SchoolId = school.SchoolID,
                            ManagerFirstName = manager.FullName?.FirstName,
                            ManagerLastName = manager.FullName?.LastName,
                            ActiveYearId = activeYearId
                        };
                    }
                }
            }

            if (schoolData == null && string.Equals(userType, "TEACHER", StringComparison.OrdinalIgnoreCase))
            {
                var teacherRow = await tenantContext.Teachers.AsNoTracking()
                    .FirstOrDefaultAsync(t => t.UserID == userId, cancellationToken);
                if (teacherRow != null)
                {
                    var mgr = await tenantContext.Managers.AsNoTracking()
                        .FirstOrDefaultAsync(m => m.ManagerID == teacherRow.ManagerID, cancellationToken);
                    if (mgr != null)
                    {
                        var school = await tenantContext.Schools.AsNoTracking()
                            .FirstOrDefaultAsync(s => s.SchoolID == mgr.SchoolID, cancellationToken);
                        if (school != null)
                        {
                            var activeYearId = await tenantContext.Years.AsNoTracking()
                                .Where(y => y.SchoolID == school.SchoolID && y.Active == true)
                                .OrderBy(y => y.YearID)
                                .Select(y => (int?)y.YearID)
                                .FirstOrDefaultAsync(cancellationToken);
                            schoolData = new
                            {
                                SchoolName = school.SchoolName,
                                SchoolId = school.SchoolID,
                                ManagerFirstName = (string?)null,
                                ManagerLastName = (string?)null,
                                ActiveYearId = activeYearId
                            };
                        }
                    }
                }
            }

            if (schoolData == null && string.Equals(userType, "STUDENT", StringComparison.OrdinalIgnoreCase))
            {
                var studentSchoolId = await tenantContext.Students.AsNoTracking()
                    .Where(s => s.UserID == userId)
                    .Select(s => (int?)s.Division.Class.Stage.Year.SchoolID)
                    .FirstOrDefaultAsync(cancellationToken);
                if (studentSchoolId is int sid)
                {
                    var school = await tenantContext.Schools.AsNoTracking()
                        .FirstOrDefaultAsync(s => s.SchoolID == sid, cancellationToken);
                    var activeYearId = await tenantContext.Years.AsNoTracking()
                        .Where(y => y.SchoolID == sid && y.Active == true)
                        .OrderBy(y => y.YearID)
                        .Select(y => (int?)y.YearID)
                        .FirstOrDefaultAsync(cancellationToken);
                    schoolData = new
                    {
                        SchoolName = school?.SchoolName,
                        SchoolId = sid,
                        ManagerFirstName = (string?)null,
                        ManagerLastName = (string?)null,
                        ActiveYearId = activeYearId
                    };
                }
            }

            yearId = schoolData?.ActiveYearId ?? 1;
            managerName = (schoolData?.ManagerFirstName + " " + schoolData?.ManagerLastName)?.Trim();
            userName = schoolData?.ManagerFirstName;

            if (string.Equals(userType, "TEACHER", StringComparison.OrdinalIgnoreCase))
            {
                var teacher = await tenantContext.Teachers.AsNoTracking()
                    .FirstOrDefaultAsync(t => t.UserID == userId, cancellationToken);
                if (teacher?.FullName != null)
                {
                    var fn = teacher.FullName;
                    var parts = new[] { fn.FirstName, fn.MiddleName, fn.LastName }
                        .Where(p => !string.IsNullOrWhiteSpace(p));
                    var display = string.Join(" ", parts).Trim();
                    if (display.Length > 0)
                    {
                        userName = fn.FirstName;
                        managerName = display;
                    }
                }
            }

            if (string.Equals(userType, "STUDENT", StringComparison.OrdinalIgnoreCase))
            {
                var student = await tenantContext.Students.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.UserID == userId, cancellationToken);
                if (student?.FullName != null)
                {
                    var fn = student.FullName;
                    var parts = new[] { fn.FirstName, fn.MiddleName, fn.LastName }
                        .Where(p => !string.IsNullOrWhiteSpace(p));
                    var display = string.Join(" ", parts).Trim();
                    if (display.Length > 0)
                    {
                        userName = fn.FirstName;
                        managerName = display;
                    }
                }
            }
        }

        if (!tenantId.HasValue)
        {
            var resolved = await ResolveTenantIdForLoginAsync(userId, userType, cancellationToken);
            if (resolved.HasValue)
            {
                tenantId = resolved;
                var mem = await GetMembershipAsync(userId, resolved.Value, cancellationToken);
                if (mem != null)
                    membershipTenantRole = mem.TenantRole;
                else
                {
                    var roleFromType = TenantRoleFromUserType(userType);
                    membershipTenantRole = roleFromType;
                    if (roleFromType.HasValue)
                        await EnsureUserTenantAsync(userId, resolved.Value, roleFromType.Value, cancellationToken);
                }
            }
        }

        if (tenantId.HasValue)
            await TouchTenantAccessAsync(userId, tenantId.Value, cancellationToken);

        response.SchoolName = schoolData?.SchoolName;
        response.ManagerName = managerName;
        response.UserName = userName;
        response.SchoolId = schoolData?.SchoolId;
        response.YearId = yearId;
        response.TenantId = tenantId;
        response.TenantDatabase = tenantDatabaseName;
        response.MembershipTenantRole = membershipTenantRole;

        if (tenantChoices.Count > 1 && !tenantId.HasValue)
            response.Tenants = tenantChoices;
        else if (tenantChoices.Count <= 1)
            response.Tenants = null;

        return response;
    }

    public Task<IReadOnlyList<UserTenantSummaryDto>> GetTenantSummariesAsync(
        string userId,
        CancellationToken cancellationToken = default) =>
        _tenantMembership.GetTenantSummariesAsync(userId, cancellationToken);

    public Task<UserTenant?> GetMembershipAsync(
        string userId,
        int tenantId,
        CancellationToken cancellationToken = default) =>
        _tenantMembership.GetMembershipAsync(userId, tenantId, cancellationToken);

    public async Task TouchTenantAccessAsync(
        string userId,
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        var utRow = await _context.UserTenants.FirstOrDefaultAsync(ut =>
            ut.UserId == userId && ut.TenantId == tenantId && ut.IsActive, cancellationToken);
        if (utRow == null)
            return;

        utRow.LastAccessedUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task EnsureUserTenantAsync(
        string userId,
        int tenantId,
        TenantRole role,
        CancellationToken cancellationToken = default)
    {
        var exists = await _context.UserTenants.AnyAsync(
            ut => ut.UserId == userId && ut.TenantId == tenantId, cancellationToken);
        if (exists)
            return;

        _context.UserTenants.Add(new UserTenant
        {
            UserId = userId,
            TenantId = tenantId,
            TenantRole = role,
            IsActive = true,
            LastAccessedUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int?> ResolveTenantIdForLoginAsync(
        string userId,
        string? userType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userType) || userType.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
            return null;

        var fromMembership = await _tenantMembership.ResolveTenantIdForIssuedTokenAsync(userId, cancellationToken);
        if (fromMembership.HasValue)
            return fromMembership.Value;

        return await ResolveTenantIdForTeacherStudentGuardianAsync(userId, userType, cancellationToken);
    }

    public Task<string?> ResolveSchoolRoleKeyAsync(
        string userId,
        int tenantId,
        CancellationToken cancellationToken = default) =>
        _schoolRoleResolver.ResolveSchoolRoleKeyAsync(userId, tenantId, cancellationToken);

    public async Task<int?> ResolveTenantIdForTeacherStudentGuardianAsync(
        string userId,
        string userType,
        CancellationToken cancellationToken = default)
    {
        var tenants = await _context.Tenants.AsNoTracking()
            .Select(t => new { t.TenantId, t.ConnectionString })
            .ToListAsync(cancellationToken);

        foreach (var row in tenants)
        {
            if (string.IsNullOrWhiteSpace(row.ConnectionString))
                continue;

            try
            {
                var tenantInfo = new TenantInfo { TenantId = row.TenantId, ConnectionString = row.ConnectionString };
                var opts = new DbContextOptionsBuilder<TenantDbContext>()
                    .UseTenantSqlServer(row.ConnectionString)
                    .Options;

                await using var ctx = new TenantDbContext(opts, tenantInfo);

                var match = false;
                if (userType.Equals("MANAGER", StringComparison.OrdinalIgnoreCase))
                    match = await ctx.Managers.AsNoTracking().AnyAsync(m => m.UserID == userId, cancellationToken);
                else if (userType.Equals("TEACHER", StringComparison.OrdinalIgnoreCase))
                    match = await ctx.Teachers.AsNoTracking().AnyAsync(t => t.UserID == userId, cancellationToken);
                else if (userType.Equals("STUDENT", StringComparison.OrdinalIgnoreCase))
                    match = await ctx.Students.AsNoTracking().AnyAsync(s => s.UserID == userId, cancellationToken);
                else if (userType.Equals("GUARDIAN", StringComparison.OrdinalIgnoreCase))
                    match = await ctx.Guardians.AsNoTracking().AnyAsync(g => g.UserID == userId, cancellationToken);

                if (match)
                    return row.TenantId;
            }
            catch
            {
                // Ignore unreachable or misconfigured tenant DBs
            }
        }

        return null;
    }

    private static TenantRole? TenantRoleFromUserType(string userType)
    {
        if (string.Equals(userType, "MANAGER", StringComparison.OrdinalIgnoreCase))
            return TenantRole.SchoolAdmin;
        if (string.Equals(userType, "TEACHER", StringComparison.OrdinalIgnoreCase))
            return TenantRole.Teacher;
        if (string.Equals(userType, "STUDENT", StringComparison.OrdinalIgnoreCase))
            return TenantRole.Student;
        if (string.Equals(userType, "GUARDIAN", StringComparison.OrdinalIgnoreCase))
            return TenantRole.Parent;
        return null;
    }
}
