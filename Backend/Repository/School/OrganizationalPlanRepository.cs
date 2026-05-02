using Backend.Data;
using Backend.DTOS.School.OrganizationalPlan;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School;

public class OrganizationalPlanRepository : IOrganizationalPlanRepository
{
    private readonly TenantDbContext _db;

    public OrganizationalPlanRepository(TenantDbContext db)
    {
        _db = db;
    }

    private static string FormatPersonName(Name? n)
    {
        if (n == null) return string.Empty;
        return string.Join(" ", new[] { n.FirstName, n.MiddleName, n.LastName }
            .Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
    }

    private async Task<int?> GetActiveYearIdForSchoolAsync(int schoolId, CancellationToken cancellationToken = default)
    {
        var yid = await _db.Years.AsNoTracking()
            .Where(y => y.SchoolID == schoolId && y.Active)
            .OrderBy(y => y.YearID)
            .Select(y => (int?)y.YearID)
            .FirstOrDefaultAsync(cancellationToken);
        if (yid is > 0)
            return yid;
        return await _db.Years.AsNoTracking()
            .Where(y => y.SchoolID == schoolId)
            .OrderByDescending(y => y.YearID)
            .Select(y => (int?)y.YearID)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task ValidateEmployeesInSchoolAsync(int schoolId, IEnumerable<int> employeeProfileIds, CancellationToken cancellationToken)
    {
        var ids = employeeProfileIds.Where(id => id > 0).Distinct().ToList();
        if (ids.Count == 0) return;
        var count = await _db.EmployeeProfiles.AsNoTracking()
            .CountAsync(e => ids.Contains(e.EmployeeProfileID) && e.SchoolID == schoolId, cancellationToken);
        if (count != ids.Count)
            throw new InvalidOperationException("One or more employee profiles were not found for this school.");
    }

    private static IEnumerable<int> CollectEmployeeIdsFromAnnualWrite(AnnualGoalWriteDto dto)
    {
        foreach (var p in dto.OperationalPlans)
        {
            if (p.OwnerEmployeeProfileID is > 0)
                yield return p.OwnerEmployeeProfileID.Value;
            foreach (var t in p.Tasks)
            {
                if (t.AssignedToEmployeeProfileID is > 0)
                    yield return t.AssignedToEmployeeProfileID.Value;
                foreach (var u in t.ProgressUpdates)
                {
                    if (u.AuthorEmployeeProfileID is > 0)
                        yield return u.AuthorEmployeeProfileID.Value;
                }
            }
        }
    }

    public async Task<IReadOnlyList<StrategicGoalListItemDto>> ListStrategicGoalsAsync(StrategicGoalFilterDto filter, CancellationToken cancellationToken = default)
    {
        filter ??= new StrategicGoalFilterDto();
        var q = _db.StrategicGoals.AsNoTracking().AsQueryable();
        if (filter.SchoolID is > 0)
            q = q.Where(x => x.SchoolID == filter.SchoolID);
        if (filter.Status is >= 0)
            q = q.Where(x => (int)x.Status == filter.Status);

        var raw = await q
            .OrderBy(x => x.SchoolID)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.StrategicGoalID)
            .Select(x => new
            {
                x.StrategicGoalID,
                x.SchoolID,
                x.ReferenceCode,
                x.Title,
                St = (int)x.Status,
                x.SortOrder,
                x.EffectiveFromUtc,
                x.EffectiveToUtc,
                x.UpdatedAtUtc,
            })
            .ToListAsync(cancellationToken);

        return raw.Select(x => new StrategicGoalListItemDto
        {
            StrategicGoalID = x.StrategicGoalID,
            SchoolID = x.SchoolID,
            ReferenceCode = x.ReferenceCode,
            Title = x.Title,
            Status = x.St,
            SortOrder = x.SortOrder,
            EffectiveFromUtc = x.EffectiveFromUtc,
            EffectiveToUtc = x.EffectiveToUtc,
            UpdatedAtUtc = x.UpdatedAtUtc,
        }).ToList();
    }

    public async Task<StrategicGoalDetailDto?> GetStrategicGoalByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var x = await _db.StrategicGoals.AsNoTracking()
            .FirstOrDefaultAsync(g => g.StrategicGoalID == id, cancellationToken);
        if (x == null) return null;
        return new StrategicGoalDetailDto
        {
            StrategicGoalID = x.StrategicGoalID,
            SchoolID = x.SchoolID,
            ReferenceCode = x.ReferenceCode,
            Title = x.Title,
            Status = (int)x.Status,
            SortOrder = x.SortOrder,
            EffectiveFromUtc = x.EffectiveFromUtc,
            EffectiveToUtc = x.EffectiveToUtc,
            UpdatedAtUtc = x.UpdatedAtUtc,
            Details = x.Details,
            CreatedAtUtc = x.CreatedAtUtc,
        };
    }

    public async Task<int> CreateStrategicGoalAsync(StrategicGoalWriteDto dto, CancellationToken cancellationToken = default)
    {
        var schoolOk = await _db.Schools.AsNoTracking().AnyAsync(s => s.SchoolID == dto.SchoolID, cancellationToken);
        if (!schoolOk)
            throw new InvalidOperationException("School was not found.");

        var now = DateTime.UtcNow;
        var e = new StrategicGoal
        {
            SchoolID = dto.SchoolID,
            ReferenceCode = dto.ReferenceCode,
            Title = dto.Title,
            Details = dto.Details,
            Status = (StrategicGoalStatus)dto.Status,
            SortOrder = dto.SortOrder,
            EffectiveFromUtc = dto.EffectiveFromUtc,
            EffectiveToUtc = dto.EffectiveToUtc,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
        _db.StrategicGoals.Add(e);
        await _db.SaveChangesAsync(cancellationToken);
        return e.StrategicGoalID;
    }

    public async Task UpdateStrategicGoalAsync(int id, StrategicGoalWriteDto dto, CancellationToken cancellationToken = default)
    {
        var e = await _db.StrategicGoals.FirstOrDefaultAsync(x => x.StrategicGoalID == id, cancellationToken)
            ?? throw new InvalidOperationException("Strategic goal was not found.");
        if (e.SchoolID != dto.SchoolID)
            throw new InvalidOperationException("School mismatch for this strategic goal.");

        var schoolOk = await _db.Schools.AsNoTracking().AnyAsync(s => s.SchoolID == dto.SchoolID, cancellationToken);
        if (!schoolOk)
            throw new InvalidOperationException("School was not found.");

        var now = DateTime.UtcNow;
        e.ReferenceCode = dto.ReferenceCode;
        e.Title = dto.Title;
        e.Details = dto.Details;
        e.Status = (StrategicGoalStatus)dto.Status;
        e.SortOrder = dto.SortOrder;
        e.EffectiveFromUtc = dto.EffectiveFromUtc;
        e.EffectiveToUtc = dto.EffectiveToUtc;
        e.UpdatedAtUtc = now;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<int?> GetSchoolIdForStrategicGoalAsync(int strategicGoalId, CancellationToken cancellationToken = default)
    {
        return _db.StrategicGoals.AsNoTracking()
            .Where(x => x.StrategicGoalID == strategicGoalId)
            .Select(x => (int?)x.SchoolID)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AnnualGoalListItemDto>> ListAnnualGoalsAsync(AnnualGoalFilterDto filter, CancellationToken cancellationToken = default)
    {
        filter ??= new AnnualGoalFilterDto();
        var q = _db.AnnualGoals.AsNoTracking().AsQueryable();

        if (filter.SchoolID is > 0)
        {
            q = q.Where(x => x.SchoolID == filter.SchoolID);
            if (filter.AcademicYearID is not > 0)
            {
                var y = await GetActiveYearIdForSchoolAsync(filter.SchoolID.Value, cancellationToken);
                if (y is > 0)
                    q = q.Where(x => x.AcademicYearID == y.Value);
            }
        }

        if (filter.AcademicYearID is > 0)
            q = q.Where(x => x.AcademicYearID == filter.AcademicYearID);
        if (filter.StrategicGoalID is > 0)
            q = q.Where(x => x.StrategicGoalID == filter.StrategicGoalID);
        if (filter.Status is >= 0)
            q = q.Where(x => (int)x.Status == filter.Status);

        var raw = await q
            .OrderBy(x => x.SchoolID)
            .ThenBy(x => x.AcademicYearID)
            .ThenBy(x => x.SortOrder)
            .Select(x => new
            {
                x.AnnualGoalID,
                x.SchoolID,
                x.AcademicYearID,
                x.StrategicGoalID,
                StTitle = x.StrategicGoal != null ? x.StrategicGoal.Title : null,
                x.Title,
                St = (int)x.Status,
                x.SortOrder,
                x.UpdatedAtUtc,
                PlanCount = x.OperationalPlans.Count,
            })
            .ToListAsync(cancellationToken);

        return raw.Select(x => new AnnualGoalListItemDto
        {
            AnnualGoalID = x.AnnualGoalID,
            SchoolID = x.SchoolID,
            AcademicYearID = x.AcademicYearID,
            StrategicGoalID = x.StrategicGoalID,
            StrategicGoalTitle = x.StTitle,
            Title = x.Title,
            Status = x.St,
            SortOrder = x.SortOrder,
            OperationalPlanCount = x.PlanCount,
            UpdatedAtUtc = x.UpdatedAtUtc,
        }).ToList();
    }

    public async Task<AnnualGoalDetailDto?> GetAnnualGoalByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var x = await _db.AnnualGoals.AsNoTracking()
            .Include(a => a.StrategicGoal)
            .Include(a => a.OperationalPlans).ThenInclude(p => p.OwnerEmployeeProfile)
            .Include(a => a.OperationalPlans).ThenInclude(p => p.Tasks).ThenInclude(t => t.AssignedToEmployeeProfile)
            .Include(a => a.OperationalPlans).ThenInclude(p => p.Tasks).ThenInclude(t => t.ProgressUpdates).ThenInclude(u => u.AuthorEmployeeProfile)
            .FirstOrDefaultAsync(a => a.AnnualGoalID == id, cancellationToken);
        if (x == null) return null;

        var list = new AnnualGoalListItemDto
        {
            AnnualGoalID = x.AnnualGoalID,
            SchoolID = x.SchoolID,
            AcademicYearID = x.AcademicYearID,
            StrategicGoalID = x.StrategicGoalID,
            StrategicGoalTitle = x.StrategicGoal?.Title,
            Title = x.Title,
            Status = (int)x.Status,
            SortOrder = x.SortOrder,
            OperationalPlanCount = x.OperationalPlans.Count,
            UpdatedAtUtc = x.UpdatedAtUtc,
        };

        return new AnnualGoalDetailDto
        {
            AnnualGoalID = list.AnnualGoalID,
            SchoolID = list.SchoolID,
            AcademicYearID = list.AcademicYearID,
            StrategicGoalID = list.StrategicGoalID,
            StrategicGoalTitle = list.StrategicGoalTitle,
            Title = list.Title,
            Status = list.Status,
            SortOrder = list.SortOrder,
            OperationalPlanCount = list.OperationalPlanCount,
            UpdatedAtUtc = list.UpdatedAtUtc,
            Details = x.Details,
            CreatedAtUtc = x.CreatedAtUtc,
            OperationalPlans = x.OperationalPlans.OrderBy(p => p.SortOrder).ThenBy(p => p.OperationalPlanID).Select(p => new OperationalPlanReadDto
            {
                OperationalPlanID = p.OperationalPlanID,
                AnnualGoalID = p.AnnualGoalID,
                Title = p.Title,
                Details = p.Details,
                Status = (int)p.Status,
                SortOrder = p.SortOrder,
                StartDateUtc = p.StartDateUtc,
                EndDateUtc = p.EndDateUtc,
                OwnerEmployeeProfileID = p.OwnerEmployeeProfileID,
                OwnerName = p.OwnerEmployeeProfile != null ? FormatPersonName(p.OwnerEmployeeProfile.FullName) : null,
                CreatedAtUtc = p.CreatedAtUtc,
                UpdatedAtUtc = p.UpdatedAtUtc,
                Tasks = p.Tasks.OrderBy(t => t.SortOrder).ThenBy(t => t.PlanTaskID).Select(t => new PlanTaskReadDto
                {
                    PlanTaskID = t.PlanTaskID,
                    OperationalPlanID = t.OperationalPlanID,
                    Title = t.Title,
                    Details = t.Details,
                    Status = (int)t.Status,
                    SortOrder = t.SortOrder,
                    ProgressPercent = t.ProgressPercent,
                    DueAtUtc = t.DueAtUtc,
                    AssignedToEmployeeProfileID = t.AssignedToEmployeeProfileID,
                    AssignedToName = t.AssignedToEmployeeProfile != null ? FormatPersonName(t.AssignedToEmployeeProfile.FullName) : null,
                    CreatedAtUtc = t.CreatedAtUtc,
                    UpdatedAtUtc = t.UpdatedAtUtc,
                    ProgressUpdates = t.ProgressUpdates.OrderByDescending(u => u.CreatedAtUtc).Select(u => new PlanProgressUpdateReadDto
                    {
                        PlanProgressUpdateID = u.PlanProgressUpdateID,
                        PlanTaskID = u.PlanTaskID,
                        Note = u.Note,
                        ProgressPercent = u.ProgressPercent,
                        AuthorEmployeeProfileID = u.AuthorEmployeeProfileID,
                        AuthorName = u.AuthorEmployeeProfile != null ? FormatPersonName(u.AuthorEmployeeProfile.FullName) : null,
                        CreatedAtUtc = u.CreatedAtUtc,
                    }).ToList(),
                }).ToList(),
            }).ToList(),
        };
    }

    private async Task ValidateAnnualGoalWriteAsync(AnnualGoalWriteDto dto, CancellationToken cancellationToken)
    {
        var schoolOk = await _db.Schools.AsNoTracking().AnyAsync(s => s.SchoolID == dto.SchoolID, cancellationToken);
        if (!schoolOk)
            throw new InvalidOperationException("School was not found.");

        var yearOk = await _db.Years.AsNoTracking()
            .AnyAsync(y => y.YearID == dto.AcademicYearID && y.SchoolID == dto.SchoolID, cancellationToken);
        if (!yearOk)
            throw new InvalidOperationException("Academic year does not belong to this school.");

        if (dto.StrategicGoalID is > 0)
        {
            var sgOk = await _db.StrategicGoals.AsNoTracking()
                .AnyAsync(g => g.StrategicGoalID == dto.StrategicGoalID && g.SchoolID == dto.SchoolID, cancellationToken);
            if (!sgOk)
                throw new InvalidOperationException("Strategic goal was not found for this school.");
        }

        await ValidateEmployeesInSchoolAsync(dto.SchoolID, CollectEmployeeIdsFromAnnualWrite(dto), cancellationToken);

        foreach (var t in dto.OperationalPlans.SelectMany(p => p.Tasks))
        {
            var pct = t.ProgressPercent;
            if (pct is < 0 or > 100)
                throw new InvalidOperationException("Task progress must be between 0 and 100.");
            foreach (var u in t.ProgressUpdates)
            {
                if (u.ProgressPercent is < 0 or > 100)
                    throw new InvalidOperationException("Progress update percent must be between 0 and 100.");
            }
        }
    }

    private async Task ReplaceOperationalTreeAsync(int annualGoalId, AnnualGoalWriteDto dto, DateTime now, CancellationToken cancellationToken)
    {
        var planIds = await _db.OperationalPlans.Where(o => o.AnnualGoalID == annualGoalId).Select(o => o.OperationalPlanID).ToListAsync(cancellationToken);
        if (planIds.Count > 0)
        {
            var taskIds = await _db.PlanTasks.Where(t => planIds.Contains(t.OperationalPlanID)).Select(t => t.PlanTaskID).ToListAsync(cancellationToken);
            if (taskIds.Count > 0)
                await _db.PlanProgressUpdates.Where(u => taskIds.Contains(u.PlanTaskID)).ExecuteDeleteAsync(cancellationToken);
            await _db.PlanTasks.Where(t => planIds.Contains(t.OperationalPlanID)).ExecuteDeleteAsync(cancellationToken);
            await _db.OperationalPlans.Where(o => o.AnnualGoalID == annualGoalId).ExecuteDeleteAsync(cancellationToken);
        }

        foreach (var pDto in dto.OperationalPlans.OrderBy(p => p.SortOrder))
        {
            var op = new OperationalPlan
            {
                AnnualGoalID = annualGoalId,
                Title = pDto.Title,
                Details = pDto.Details,
                Status = (OperationalPlanStatus)pDto.Status,
                SortOrder = pDto.SortOrder,
                StartDateUtc = pDto.StartDateUtc,
                EndDateUtc = pDto.EndDateUtc,
                OwnerEmployeeProfileID = pDto.OwnerEmployeeProfileID is > 0 ? pDto.OwnerEmployeeProfileID : null,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            };
            _db.OperationalPlans.Add(op);
            await _db.SaveChangesAsync(cancellationToken);

            foreach (var tDto in pDto.Tasks.OrderBy(t => t.SortOrder))
            {
                var task = new PlanTask
                {
                    OperationalPlanID = op.OperationalPlanID,
                    Title = tDto.Title,
                    Details = tDto.Details,
                    Status = (PlanTaskStatus)tDto.Status,
                    SortOrder = tDto.SortOrder,
                    ProgressPercent = Math.Clamp(tDto.ProgressPercent, 0, 100),
                    DueAtUtc = tDto.DueAtUtc,
                    AssignedToEmployeeProfileID = tDto.AssignedToEmployeeProfileID is > 0 ? tDto.AssignedToEmployeeProfileID : null,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now,
                };
                _db.PlanTasks.Add(task);
                await _db.SaveChangesAsync(cancellationToken);

                foreach (var uDto in tDto.ProgressUpdates)
                {
                    _db.PlanProgressUpdates.Add(new PlanProgressUpdate
                    {
                        PlanTaskID = task.PlanTaskID,
                        Note = uDto.Note,
                        ProgressPercent = uDto.ProgressPercent is >= 0 and <= 100 ? uDto.ProgressPercent : null,
                        AuthorEmployeeProfileID = uDto.AuthorEmployeeProfileID is > 0 ? uDto.AuthorEmployeeProfileID : null,
                        CreatedAtUtc = now,
                    });
                }
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CreateAnnualGoalAsync(AnnualGoalWriteDto dto, CancellationToken cancellationToken = default)
    {
        await ValidateAnnualGoalWriteAsync(dto, cancellationToken);
        var now = DateTime.UtcNow;
        var e = new AnnualGoal
        {
            SchoolID = dto.SchoolID,
            AcademicYearID = dto.AcademicYearID,
            StrategicGoalID = dto.StrategicGoalID is > 0 ? dto.StrategicGoalID : null,
            Title = dto.Title,
            Details = dto.Details,
            Status = (AnnualGoalStatus)dto.Status,
            SortOrder = dto.SortOrder,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
        _db.AnnualGoals.Add(e);
        await _db.SaveChangesAsync(cancellationToken);
        await ReplaceOperationalTreeAsync(e.AnnualGoalID, dto, now, cancellationToken);
        return e.AnnualGoalID;
    }

    public async Task UpdateAnnualGoalAsync(int id, AnnualGoalWriteDto dto, CancellationToken cancellationToken = default)
    {
        var e = await _db.AnnualGoals.FirstOrDefaultAsync(x => x.AnnualGoalID == id, cancellationToken)
            ?? throw new InvalidOperationException("Annual goal was not found.");
        if (e.SchoolID != dto.SchoolID)
            throw new InvalidOperationException("School mismatch for this annual goal.");

        await ValidateAnnualGoalWriteAsync(dto, cancellationToken);
        var now = DateTime.UtcNow;
        e.AcademicYearID = dto.AcademicYearID;
        e.StrategicGoalID = dto.StrategicGoalID is > 0 ? dto.StrategicGoalID : null;
        e.Title = dto.Title;
        e.Details = dto.Details;
        e.Status = (AnnualGoalStatus)dto.Status;
        e.SortOrder = dto.SortOrder;
        e.UpdatedAtUtc = now;
        await _db.SaveChangesAsync(cancellationToken);
        await ReplaceOperationalTreeAsync(id, dto, now, cancellationToken);
    }

    public Task<int?> GetSchoolIdForAnnualGoalAsync(int annualGoalId, CancellationToken cancellationToken = default)
    {
        return _db.AnnualGoals.AsNoTracking()
            .Where(x => x.AnnualGoalID == annualGoalId)
            .Select(x => (int?)x.SchoolID)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DepartmentGoalListItemDto>> ListDepartmentGoalsAsync(DepartmentGoalFilterDto filter, CancellationToken cancellationToken = default)
    {
        filter ??= new DepartmentGoalFilterDto();
        var q = _db.DepartmentGoals.AsNoTracking().AsQueryable();
        if (filter.SchoolID is > 0)
            q = q.Where(x => x.SchoolID == filter.SchoolID);
        if (filter.AcademicYearID is > 0)
            q = q.Where(x => x.AcademicYearID == filter.AcademicYearID);
        if (filter.Status is >= 0)
            q = q.Where(x => (int)x.Status == filter.Status);

        var raw = await q
            .OrderBy(x => x.SchoolID)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.DepartmentGoalID)
            .Select(x => new
            {
                x.DepartmentGoalID,
                x.SchoolID,
                x.AcademicYearID,
                x.StrategicGoalID,
                x.AnnualGoalID,
                x.DepartmentName,
                x.Title,
                St = (int)x.Status,
                x.SortOrder,
                x.UpdatedAtUtc,
            })
            .ToListAsync(cancellationToken);

        return raw.Select(x => new DepartmentGoalListItemDto
        {
            DepartmentGoalID = x.DepartmentGoalID,
            SchoolID = x.SchoolID,
            AcademicYearID = x.AcademicYearID,
            StrategicGoalID = x.StrategicGoalID,
            AnnualGoalID = x.AnnualGoalID,
            DepartmentName = x.DepartmentName,
            Title = x.Title,
            Status = x.St,
            SortOrder = x.SortOrder,
            UpdatedAtUtc = x.UpdatedAtUtc,
        }).ToList();
    }

    public async Task<DepartmentGoalDetailDto?> GetDepartmentGoalByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var x = await _db.DepartmentGoals.AsNoTracking()
            .Include(d => d.OwnerEmployeeProfile)
            .FirstOrDefaultAsync(d => d.DepartmentGoalID == id, cancellationToken);
        if (x == null) return null;
        return new DepartmentGoalDetailDto
        {
            DepartmentGoalID = x.DepartmentGoalID,
            SchoolID = x.SchoolID,
            AcademicYearID = x.AcademicYearID,
            StrategicGoalID = x.StrategicGoalID,
            AnnualGoalID = x.AnnualGoalID,
            DepartmentName = x.DepartmentName,
            Title = x.Title,
            Status = (int)x.Status,
            SortOrder = x.SortOrder,
            UpdatedAtUtc = x.UpdatedAtUtc,
            Details = x.Details,
            OwnerEmployeeProfileID = x.OwnerEmployeeProfileID,
            OwnerName = x.OwnerEmployeeProfile != null ? FormatPersonName(x.OwnerEmployeeProfile.FullName) : null,
            CreatedAtUtc = x.CreatedAtUtc,
        };
    }

    private async Task ValidateDepartmentGoalWriteAsync(DepartmentGoalWriteDto dto, CancellationToken cancellationToken)
    {
        var schoolOk = await _db.Schools.AsNoTracking().AnyAsync(s => s.SchoolID == dto.SchoolID, cancellationToken);
        if (!schoolOk)
            throw new InvalidOperationException("School was not found.");

        if (dto.AcademicYearID is > 0)
        {
            var yOk = await _db.Years.AsNoTracking()
                .AnyAsync(y => y.YearID == dto.AcademicYearID && y.SchoolID == dto.SchoolID, cancellationToken);
            if (!yOk)
                throw new InvalidOperationException("Academic year does not belong to this school.");
        }

        if (dto.StrategicGoalID is > 0)
        {
            var sOk = await _db.StrategicGoals.AsNoTracking()
                .AnyAsync(g => g.StrategicGoalID == dto.StrategicGoalID && g.SchoolID == dto.SchoolID, cancellationToken);
            if (!sOk)
                throw new InvalidOperationException("Strategic goal was not found for this school.");
        }

        if (dto.AnnualGoalID is > 0)
        {
            var aOk = await _db.AnnualGoals.AsNoTracking()
                .AnyAsync(a => a.AnnualGoalID == dto.AnnualGoalID && a.SchoolID == dto.SchoolID, cancellationToken);
            if (!aOk)
                throw new InvalidOperationException("Annual goal was not found for this school.");
        }

        if (dto.OwnerEmployeeProfileID is > 0)
            await ValidateEmployeesInSchoolAsync(dto.SchoolID, new[] { dto.OwnerEmployeeProfileID.Value }, cancellationToken);
    }

    public async Task<int> CreateDepartmentGoalAsync(DepartmentGoalWriteDto dto, CancellationToken cancellationToken = default)
    {
        await ValidateDepartmentGoalWriteAsync(dto, cancellationToken);
        var now = DateTime.UtcNow;
        var e = new DepartmentGoal
        {
            SchoolID = dto.SchoolID,
            AcademicYearID = dto.AcademicYearID is > 0 ? dto.AcademicYearID : null,
            StrategicGoalID = dto.StrategicGoalID is > 0 ? dto.StrategicGoalID : null,
            AnnualGoalID = dto.AnnualGoalID is > 0 ? dto.AnnualGoalID : null,
            DepartmentName = dto.DepartmentName,
            Title = dto.Title,
            Details = dto.Details,
            Status = (DepartmentGoalStatus)dto.Status,
            SortOrder = dto.SortOrder,
            OwnerEmployeeProfileID = dto.OwnerEmployeeProfileID is > 0 ? dto.OwnerEmployeeProfileID : null,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
        _db.DepartmentGoals.Add(e);
        await _db.SaveChangesAsync(cancellationToken);
        return e.DepartmentGoalID;
    }

    public async Task UpdateDepartmentGoalAsync(int id, DepartmentGoalWriteDto dto, CancellationToken cancellationToken = default)
    {
        var e = await _db.DepartmentGoals.FirstOrDefaultAsync(x => x.DepartmentGoalID == id, cancellationToken)
            ?? throw new InvalidOperationException("Department goal was not found.");
        if (e.SchoolID != dto.SchoolID)
            throw new InvalidOperationException("School mismatch for this department goal.");

        await ValidateDepartmentGoalWriteAsync(dto, cancellationToken);
        var now = DateTime.UtcNow;
        e.AcademicYearID = dto.AcademicYearID is > 0 ? dto.AcademicYearID : null;
        e.StrategicGoalID = dto.StrategicGoalID is > 0 ? dto.StrategicGoalID : null;
        e.AnnualGoalID = dto.AnnualGoalID is > 0 ? dto.AnnualGoalID : null;
        e.DepartmentName = dto.DepartmentName;
        e.Title = dto.Title;
        e.Details = dto.Details;
        e.Status = (DepartmentGoalStatus)dto.Status;
        e.SortOrder = dto.SortOrder;
        e.OwnerEmployeeProfileID = dto.OwnerEmployeeProfileID is > 0 ? dto.OwnerEmployeeProfileID : null;
        e.UpdatedAtUtc = now;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<int?> GetSchoolIdForDepartmentGoalAsync(int departmentGoalId, CancellationToken cancellationToken = default)
    {
        return _db.DepartmentGoals.AsNoTracking()
            .Where(x => x.DepartmentGoalID == departmentGoalId)
            .Select(x => (int?)x.SchoolID)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
