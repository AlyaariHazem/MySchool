using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Backend.Repository.School.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class EmployeeYearAssignmentService : IEmployeeYearAssignmentService
{
    private readonly TenantDbContext _context;
    private readonly IYearRepository _years;

    public EmployeeYearAssignmentService(TenantDbContext context, IYearRepository years)
    {
        _context = context;
        _years = years;
    }

    private TenantDbContext Db(TenantDbContext? ctx) => ctx ?? _context;

    public async Task<bool> TenantUsesYearAssignmentsAsync(TenantDbContext? context = null, CancellationToken cancellationToken = default)
    {
        return await Db(context).EmployeeYearAssignments.AsNoTracking()
            .AnyAsync(cancellationToken);
    }

    public async Task<int?> ResolveYearIdForListAsync(int? requestedYearId, TenantDbContext? context = null, CancellationToken cancellationToken = default)
    {
        if (requestedYearId is > 0)
            return requestedYearId;

        var active = await _years.GetActiveYearIdAsync(cancellationToken);
        if (active is > 0)
            return active;

        var db = Db(context);
        return await db.Years.AsNoTracking()
            .OrderByDescending(y => y.YearID)
            .Select(y => (int?)y.YearID)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<HashSet<int>> GetActiveEntityIdsForYearAsync(
        string employeeRole,
        int yearId,
        TenantDbContext? context = null,
        CancellationToken cancellationToken = default)
    {
        var db = Db(context);
        var ids = await db.EmployeeYearAssignments.AsNoTracking()
            .Where(a => a.YearID == yearId
                        && a.EmployeeRole == employeeRole
                        && a.AssignmentStatus == EmployeeYearAssignmentStatuses.Active)
            .Select(a => a.EmployeeEntityID)
            .ToListAsync(cancellationToken);

        return ids.ToHashSet();
    }

    public async Task EnsureActiveAssignmentAsync(
        string employeeRole,
        int employeeEntityId,
        int? yearId,
        TenantDbContext? context = null,
        CancellationToken cancellationToken = default)
    {
        var db = Db(context);
        var yid = yearId ?? await _years.GetActiveYearIdAsync(cancellationToken);
        if (yid is null or <= 0)
            yid = await db.Years.AsNoTracking().OrderByDescending(y => y.YearID).Select(y => (int?)y.YearID).FirstOrDefaultAsync(cancellationToken);

        if (yid is null or <= 0)
            return;

        var existing = await db.EmployeeYearAssignments
            .FirstOrDefaultAsync(
                a => a.YearID == yid && a.EmployeeRole == employeeRole && a.EmployeeEntityID == employeeEntityId,
                cancellationToken);

        if (existing != null)
        {
            if (existing.AssignmentStatus != EmployeeYearAssignmentStatuses.Active)
            {
                existing.AssignmentStatus = EmployeeYearAssignmentStatuses.Active;
                existing.ExitDate = null;
                existing.ExitReason = null;
            }
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        db.EmployeeYearAssignments.Add(new EmployeeYearAssignment
        {
            YearID = yid.Value,
            EmployeeRole = employeeRole,
            EmployeeEntityID = employeeEntityId,
            AssignmentStatus = EmployeeYearAssignmentStatuses.Active
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ArchiveEmployeeForYearAsync(
        string employeeRole,
        int employeeEntityId,
        int? yearId,
        DateTime? exitDate,
        string? exitReason,
        string? notes,
        TenantDbContext? context = null,
        CancellationToken cancellationToken = default)
    {
        var db = Db(context);
        var yid = yearId ?? await _years.GetActiveYearIdAsync(cancellationToken);
        if (yid is null or <= 0)
            yid = await db.Years.AsNoTracking().OrderByDescending(y => y.YearID).Select(y => (int?)y.YearID).FirstOrDefaultAsync(cancellationToken);

        if (yid is null or <= 0)
            return;

        var exit = exitDate ?? DateTime.UtcNow;
        var row = await db.EmployeeYearAssignments
            .FirstOrDefaultAsync(
                a => a.YearID == yid && a.EmployeeRole == employeeRole && a.EmployeeEntityID == employeeEntityId,
                cancellationToken);

        if (row != null)
        {
            row.AssignmentStatus = EmployeeYearAssignmentStatuses.Archived;
            row.ExitDate = exit;
            row.ExitReason = exitReason;
            row.Notes = notes;
        }
        else
        {
            db.EmployeeYearAssignments.Add(new EmployeeYearAssignment
            {
                YearID = yid.Value,
                EmployeeRole = employeeRole,
                EmployeeEntityID = employeeEntityId,
                AssignmentStatus = EmployeeYearAssignmentStatuses.Archived,
                ExitDate = exit,
                ExitReason = exitReason,
                Notes = notes
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RolloverContinueAsync(
        int sourceYearId,
        int targetYearId,
        IReadOnlyCollection<int> teacherIds,
        IReadOnlyCollection<int> managerIds,
        IReadOnlyCollection<int>? schoolStaffIds = null,
        TenantDbContext? context = null,
        CancellationToken cancellationToken = default)
    {
        if (sourceYearId <= 0 || targetYearId <= 0 || sourceYearId == targetYearId)
            throw new ArgumentException("Invalid year pair for rollover.");

        var db = Db(context);

        async Task CarryAsync(string role, IReadOnlyCollection<int> ids)
        {
            foreach (var id in ids.Distinct())
            {
                if (id <= 0) continue;

                var existsTarget = await db.EmployeeYearAssignments.AsNoTracking()
                    .AnyAsync(
                        a => a.YearID == targetYearId && a.EmployeeRole == role && a.EmployeeEntityID == id,
                        cancellationToken);
                if (existsTarget)
                    continue;

                var source = await db.EmployeeYearAssignments
                    .FirstOrDefaultAsync(
                        a => a.YearID == sourceYearId && a.EmployeeRole == role && a.EmployeeEntityID == id,
                        cancellationToken);

                if (source != null && source.AssignmentStatus != EmployeeYearAssignmentStatuses.Active)
                    continue;

                if (source == null && role == EmployeeYearAssignmentRoles.Teacher)
                {
                    var teacherExists = await db.Teachers.AsNoTracking().AnyAsync(t => t.TeacherID == id, cancellationToken);
                    if (!teacherExists)
                        continue;
                }

                if (source == null && role == EmployeeYearAssignmentRoles.Manager)
                {
                    var mgrExists = await db.Managers.AsNoTracking().AnyAsync(m => m.ManagerID == id, cancellationToken);
                    if (!mgrExists)
                        continue;
                }

                if (source == null && role == EmployeeYearAssignmentRoles.SchoolStaff)
                {
                    var ssExists = await db.SchoolStaff.AsNoTracking().AnyAsync(s => s.SchoolStaffID == id, cancellationToken);
                    if (!ssExists)
                        continue;
                }

                db.EmployeeYearAssignments.Add(new EmployeeYearAssignment
                {
                    YearID = targetYearId,
                    EmployeeRole = role,
                    EmployeeEntityID = id,
                    AssignmentStatus = EmployeeYearAssignmentStatuses.Active
                });
            }
        }

        await CarryAsync(EmployeeYearAssignmentRoles.Teacher, teacherIds);
        await CarryAsync(EmployeeYearAssignmentRoles.Manager, managerIds);
        await CarryAsync(EmployeeYearAssignmentRoles.SchoolStaff, schoolStaffIds ?? Array.Empty<int>());
        await db.SaveChangesAsync(cancellationToken);
    }
}
