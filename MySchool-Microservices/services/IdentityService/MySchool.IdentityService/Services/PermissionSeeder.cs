using MySchool.Contracts.Authorization;
using MySchool.IdentityService.Data;
using MySchool.IdentityService.Entities;
using Microsoft.EntityFrameworkCore;

namespace MySchool.IdentityService.Services;

public static class PermissionSeeder
{
    public static async Task SeedAsync(IdentityDbContext db, CancellationToken cancellationToken = default)
    {
        var byName = await db.Permissions.AsNoTracking()
            .ToDictionaryAsync(p => p.Name, p => p.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var addedPermission = false;
        foreach (var name in PagePermissionNames.All)
        {
            if (byName.ContainsKey(name))
                continue;

            var lastDot = name.LastIndexOf('.');
            var page = lastDot > 0 ? name[..lastDot] : name;
            var action = lastDot > 0 ? name[(lastDot + 1)..] : PagePermissionNames.ActionView;
            db.Permissions.Add(new Permission { Name = name, Page = page, Action = action });
            addedPermission = true;
        }

        if (addedPermission)
            await db.SaveChangesAsync(cancellationToken);

        byName = await db.Permissions.AsNoTracking()
            .ToDictionaryAsync(p => p.Name, p => p.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var roleNames = SchoolUserRoleKeys.AllRoles;
        var existingPairs = await db.RolePermissions.AsNoTracking()
            .Where(rp => roleNames.Contains(rp.RoleName))
            .Select(rp => new { rp.RoleName, rp.PermissionId })
            .ToListAsync(cancellationToken);

        var existingSet = existingPairs
            .Select(x => (x.RoleName, x.PermissionId))
            .ToHashSet();

        foreach (var role in SchoolUserRoleKeys.AllRoles)
        {
            foreach (var permName in PagePermissionNames.All)
            {
                if (!byName.TryGetValue(permName, out var pid))
                    continue;

                if (existingSet.Contains((role, pid)))
                    continue;

                db.RolePermissions.Add(new RolePermission
                {
                    RoleName = role,
                    PermissionId = pid,
                    IsAllowed = DefaultAllowed(role, permName),
                });
                existingSet.Add((role, pid));
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static bool IsPage(string perm, string page) =>
        perm.StartsWith(page + ".", StringComparison.OrdinalIgnoreCase);

    private static bool IsAnyReportPermission(string perm) =>
        IsPage(perm, PagePermissionNames.PageReports)
        || IsPage(perm, PagePermissionNames.PageReportsFinancial)
        || IsPage(perm, PagePermissionNames.PageReportsTerm)
        || IsPage(perm, PagePermissionNames.PageReportsMonthly)
        || IsPage(perm, PagePermissionNames.PageReportsRegistration)
        || IsPage(perm, PagePermissionNames.PageReportsAllotment);

    private static bool IsAiChatPermission(string perm) =>
        IsPage(perm, PagePermissionNames.PageAiChat);

    private static bool DefaultAllowed(string role, string perm)
    {
        if (role == SchoolUserRoleKeys.SystemAdmin || role == SchoolUserRoleKeys.Manager)
            return true;

        if (role == SchoolUserRoleKeys.EducationalSupervisor)
        {
            if (perm == PagePermissionNames.P(PagePermissionNames.PageDashboard, PagePermissionNames.ActionView))
                return true;
            if (perm == PagePermissionNames.P(PagePermissionNames.PageTeachers, PagePermissionNames.ActionView))
                return true;
            if (IsPage(perm, PagePermissionNames.PageEvaluations))
                return true;
            if (IsAnyReportPermission(perm))
                return true;
            if (IsAiChatPermission(perm))
                return true;
            if (IsPage(perm, PagePermissionNames.PageGrades))
                return true;
            if (IsPage(perm, PagePermissionNames.PageCourses))
                return true;
            if (IsPage(perm, PagePermissionNames.PagePlans))
                return true;
            if (IsPage(perm, PagePermissionNames.PageExams))
                return true;
            if (IsPage(perm, PagePermissionNames.PageHomework))
                return true;
            if (IsPage(perm, PagePermissionNames.PageTests))
                return true;
            if (perm == PagePermissionNames.P(PagePermissionNames.PageSchedule, PagePermissionNames.ActionView)
                || perm == PagePermissionNames.P(PagePermissionNames.PageCalendar, PagePermissionNames.ActionView)
                || perm == PagePermissionNames.P(PagePermissionNames.PageNotifications, PagePermissionNames.ActionView)
                || perm == PagePermissionNames.P(PagePermissionNames.PageAttendance, PagePermissionNames.ActionView)
                || perm == PagePermissionNames.P(PagePermissionNames.PageEvents, PagePermissionNames.ActionView))
                return true;
            return false;
        }

        if (role == SchoolUserRoleKeys.AdministrativeSupervisor)
        {
            if (perm == PagePermissionNames.P(PagePermissionNames.PageDashboard, PagePermissionNames.ActionView))
                return true;
            if (IsPage(perm, PagePermissionNames.PageEmployees))
                return true;
            if (IsPage(perm, PagePermissionNames.PageRecruitment))
                return true;
            if (IsPage(perm, PagePermissionNames.PageRequests))
                return true;
            if (IsPage(perm, PagePermissionNames.PageComplaints))
                return true;
            if (IsPage(perm, PagePermissionNames.PageAccounts))
                return true;
            if (IsPage(perm, PagePermissionNames.PageFees))
                return true;
            if (IsPage(perm, PagePermissionNames.PagePayroll))
                return true;
            if (IsPage(perm, PagePermissionNames.PageGuardians))
                return true;
            if (IsPage(perm, PagePermissionNames.PageManagement))
                return true;
            if (IsPage(perm, PagePermissionNames.PageSettings))
                return true;
            if (IsAnyReportPermission(perm))
                return true;
            if (IsAiChatPermission(perm))
                return true;
            if (perm == PagePermissionNames.P(PagePermissionNames.PageCalendar, PagePermissionNames.ActionView)
                || perm == PagePermissionNames.P(PagePermissionNames.PageSchedule, PagePermissionNames.ActionView)
                || perm == PagePermissionNames.P(PagePermissionNames.PageNotifications, PagePermissionNames.ActionView))
                return true;
            return false;
        }

        if (role == SchoolUserRoleKeys.Teacher)
        {
            if (perm == PagePermissionNames.P(PagePermissionNames.PageDashboard, PagePermissionNames.ActionView))
                return true;
            if (perm == PagePermissionNames.P(PagePermissionNames.PageStudents, PagePermissionNames.ActionView))
                return true;
            if (IsPage(perm, PagePermissionNames.PageEvaluations))
                return true;
            if (IsPage(perm, PagePermissionNames.PageGrades))
                return true;
            if (IsPage(perm, PagePermissionNames.PageHomework))
                return true;
            if (IsPage(perm, PagePermissionNames.PageExams))
                return true;
            if (IsPage(perm, PagePermissionNames.PageAttendance))
                return true;
            if (IsPage(perm, PagePermissionNames.PageCourses))
                return true;
            if (IsPage(perm, PagePermissionNames.PagePlans))
                return true;
            if (IsPage(perm, PagePermissionNames.PageTests))
                return true;
            if (perm == PagePermissionNames.P(PagePermissionNames.PageSchedule, PagePermissionNames.ActionView)
                || perm == PagePermissionNames.P(PagePermissionNames.PageCalendar, PagePermissionNames.ActionView)
                || perm == PagePermissionNames.P(PagePermissionNames.PageNotifications, PagePermissionNames.ActionView)
                || perm == PagePermissionNames.P(PagePermissionNames.PageTeachers, PagePermissionNames.ActionView))
                return true;
            if (IsPage(perm, PagePermissionNames.PageReportsTerm)
                || IsPage(perm, PagePermissionNames.PageReportsMonthly)
                || IsPage(perm, PagePermissionNames.PageReportsRegistration))
                return true;
            if (IsAiChatPermission(perm))
                return true;
            return false;
        }

        if (role == SchoolUserRoleKeys.AdministrativeEmployee)
        {
            if (perm == PagePermissionNames.P(PagePermissionNames.PageDashboard, PagePermissionNames.ActionView))
                return true;
            if (IsPage(perm, PagePermissionNames.PageRequests))
                return true;
            if (IsPage(perm, PagePermissionNames.PageComplaints))
                return true;
            if (IsPage(perm, PagePermissionNames.PageFees))
                return true;
            if (IsPage(perm, PagePermissionNames.PageAccounts))
                return true;
            if (perm == PagePermissionNames.P(PagePermissionNames.PageNotifications, PagePermissionNames.ActionView))
                return true;
            if (IsPage(perm, PagePermissionNames.PageReportsFinancial))
                return true;
            if (IsAiChatPermission(perm))
                return true;
            return false;
        }

        if (role == SchoolUserRoleKeys.Student || role == SchoolUserRoleKeys.Guardian)
        {
            if (perm == PagePermissionNames.P(PagePermissionNames.PageDashboard, PagePermissionNames.ActionView))
                return true;
            if (perm == PagePermissionNames.P(PagePermissionNames.PageReports, PagePermissionNames.ActionView)
                || perm == PagePermissionNames.P(PagePermissionNames.PageReportsTerm, PagePermissionNames.ActionView)
                || perm == PagePermissionNames.P(PagePermissionNames.PageReportsMonthly, PagePermissionNames.ActionView)
                || perm == PagePermissionNames.P(PagePermissionNames.PageReportsRegistration, PagePermissionNames.ActionView))
                return true;
            if (perm == PagePermissionNames.P(PagePermissionNames.PageNotifications, PagePermissionNames.ActionView))
                return true;
            if (perm == PagePermissionNames.P(PagePermissionNames.PageHomework, PagePermissionNames.ActionView)
                || perm == PagePermissionNames.P(PagePermissionNames.PageExams, PagePermissionNames.ActionView)
                || perm == PagePermissionNames.P(PagePermissionNames.PageSchedule, PagePermissionNames.ActionView)
                || perm == PagePermissionNames.P(PagePermissionNames.PageFees, PagePermissionNames.ActionView)
                || perm == PagePermissionNames.P(PagePermissionNames.PageGrades, PagePermissionNames.ActionView)
                || perm == PagePermissionNames.P(PagePermissionNames.PageCalendar, PagePermissionNames.ActionView)
                || perm == PagePermissionNames.P(PagePermissionNames.PageEvents, PagePermissionNames.ActionView)
                || perm == PagePermissionNames.P(PagePermissionNames.PageHolidays, PagePermissionNames.ActionView)
                || perm == PagePermissionNames.P(PagePermissionNames.PageTests, PagePermissionNames.ActionView)
                || perm == PagePermissionNames.P(PagePermissionNames.PageCourses, PagePermissionNames.ActionView))
                return true;
            if (role == SchoolUserRoleKeys.Guardian && IsAiChatPermission(perm))
                return true;
            return false;
        }

        return false;
    }
}
