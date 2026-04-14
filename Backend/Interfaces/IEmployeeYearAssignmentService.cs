using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Models;

namespace Backend.Interfaces;

/// <summary>
/// Per school-year status for teachers/managers without duplicating person rows.
/// </summary>
public interface IEmployeeYearAssignmentService
{
    /// <summary>True once any assignment row exists for this tenant (enables year-scoped lists).</summary>
    Task<bool> TenantUsesYearAssignmentsAsync(TenantDbContext? context = null, CancellationToken cancellationToken = default);

    /// <summary>Effective year for API lists: explicit request, else active academic year, else max(YearID).</summary>
    Task<int?> ResolveYearIdForListAsync(int? requestedYearId, TenantDbContext? context = null, CancellationToken cancellationToken = default);

    Task<HashSet<int>> GetActiveEntityIdsForYearAsync(string employeeRole, int yearId, TenantDbContext? context = null, CancellationToken cancellationToken = default);

    /// <summary>Idempotent: ensures one Active row for the given year (current DB or overridden context).</summary>
    Task EnsureActiveAssignmentAsync(
        string employeeRole,
        int employeeEntityId,
        int? yearId,
        TenantDbContext? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>Marks the assignment for the given year as archived (employee deactivated for that year). Does not delete persons or users.</summary>
    Task ArchiveEmployeeForYearAsync(
        string employeeRole,
        int employeeEntityId,
        int? yearId,
        System.DateTime? exitDate,
        string? exitReason,
        string? notes,
        TenantDbContext? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>Copy selected staff into the target year as Active without duplicating Teacher/Manager rows.</summary>
    Task RolloverContinueAsync(
        int sourceYearId,
        int targetYearId,
        IReadOnlyCollection<int> teacherIds,
        IReadOnlyCollection<int> managerIds,
        IReadOnlyCollection<int>? schoolStaffIds = null,
        TenantDbContext? context = null,
        CancellationToken cancellationToken = default);
}
