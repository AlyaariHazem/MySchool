using Backend.Common;
using Backend.Data;
using Backend.Models.Master;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

/// <summary>Seeds <see cref="Permission"/> and <see cref="RolePermission"/> defaults (idempotent).</summary>
public static class PermissionSeeder
{
    public static async Task SeedAsync(DatabaseContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Permissions.AnyAsync(cancellationToken))
            return;

        var permissions = new List<Permission>();
        foreach (var name in PagePermissionNames.All)
        {
            var lastDot = name.LastIndexOf('.');
            var page = lastDot > 0 ? name[..lastDot] : name;
            var action = lastDot > 0 ? name[(lastDot + 1)..] : PagePermissionNames.ActionView;
            permissions.Add(new Permission { Name = name, Page = page, Action = action });
        }

        db.Permissions.AddRange(permissions);
        await db.SaveChangesAsync(cancellationToken);

        var byName = await db.Permissions.AsNoTracking()
            .ToDictionaryAsync(p => p.Name, p => p.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var role in SchoolUserRoleKeys.AllRoles)
        {
            foreach (var permName in PagePermissionNames.All)
            {
                if (!byName.TryGetValue(permName, out var pid))
                    continue;
                db.RolePermissions.Add(new RolePermission
                {
                    RoleName = role,
                    PermissionId = pid,
                    IsAllowed = DefaultAllowed(role, permName)
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static bool DefaultAllowed(string role, string perm)
    {
        if (role == SchoolUserRoleKeys.SystemAdmin)
            return true;

        if (role == SchoolUserRoleKeys.Manager)
            return true;

        if (role == SchoolUserRoleKeys.EducationalSupervisor)
        {
            return perm.StartsWith(PagePermissionNames.PageEvaluations + ".", StringComparison.OrdinalIgnoreCase)
                   || perm.StartsWith(PagePermissionNames.PageReports + ".", StringComparison.OrdinalIgnoreCase)
                   || perm == PagePermissionNames.P(PagePermissionNames.PageTeachers, PagePermissionNames.ActionView)
                   || perm == PagePermissionNames.P(PagePermissionNames.PageDashboard, PagePermissionNames.ActionView);
        }

        if (role == SchoolUserRoleKeys.AdministrativeSupervisor)
        {
            return perm.StartsWith(PagePermissionNames.PageEmployees + ".", StringComparison.OrdinalIgnoreCase)
                   || perm.StartsWith(PagePermissionNames.PageRequests + ".", StringComparison.OrdinalIgnoreCase)
                   || perm.StartsWith(PagePermissionNames.PageComplaints + ".", StringComparison.OrdinalIgnoreCase)
                   || perm == PagePermissionNames.P(PagePermissionNames.PageDashboard, PagePermissionNames.ActionView);
        }

        if (role == SchoolUserRoleKeys.Teacher)
        {
            return perm == PagePermissionNames.P(PagePermissionNames.PageDashboard, PagePermissionNames.ActionView)
                   || perm.StartsWith(PagePermissionNames.PageEvaluations + ".", StringComparison.OrdinalIgnoreCase)
                   || perm == PagePermissionNames.P(PagePermissionNames.PageStudents, PagePermissionNames.ActionView);
        }

        if (role == SchoolUserRoleKeys.AdministrativeEmployee)
        {
            return perm == PagePermissionNames.P(PagePermissionNames.PageDashboard, PagePermissionNames.ActionView)
                   || perm.StartsWith(PagePermissionNames.PageRequests + ".", StringComparison.OrdinalIgnoreCase)
                   || perm == PagePermissionNames.P(PagePermissionNames.PageComplaints, PagePermissionNames.ActionView)
                   || perm == PagePermissionNames.P(PagePermissionNames.PageComplaints, PagePermissionNames.ActionCreate);
        }

        if (role == SchoolUserRoleKeys.Student || role == SchoolUserRoleKeys.Guardian)
        {
            return perm == PagePermissionNames.P(PagePermissionNames.PageDashboard, PagePermissionNames.ActionView)
                   || perm == PagePermissionNames.P(PagePermissionNames.PageReports, PagePermissionNames.ActionView);
        }

        return false;
    }
}
