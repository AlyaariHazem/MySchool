using Backend.Data;
using Backend.DTOS.School.CentralPoints;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School;

public class CentralPointsRepository : ICentralPointsRepository
{
    private readonly TenantDbContext _db;

    private static readonly (string Code, string DisplayName, int SortOrder)[] DefaultSources =
    [
        ("ACHIEVEMENT", "Achievement / إنجاز", 10),
        ("ACTIVITY", "Activity / نشاط", 20),
        ("VIOLATION", "Violation / مخالفة", 30),
        ("DAILY_EVALUATION", "Daily evaluation / تقييم يومي", 40),
        ("EMPLOYEE_REQUEST", "Commitment & requests / التزام وطلبات", 50),
        ("COMPLAINT", "Complaint / شكوى", 60),
        ("CONCERN", "Concern / مخاوف", 70),
        ("MANUAL", "Manual adjustment / يدوي", 80),
    ];

    public CentralPointsRepository(TenantDbContext db)
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

    public async Task EnsureDefaultSourcesAsync(CancellationToken cancellationToken = default)
    {
        if (await _db.PointsSources.AnyAsync(cancellationToken))
            return;
        foreach (var (code, displayName, sortOrder) in DefaultSources)
        {
            _db.PointsSources.Add(new PointsSource
            {
                Code = code,
                DisplayName = displayName,
                SortOrder = sortOrder,
                IsActive = true,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PointsSourceDto>> ListSourcesAsync(bool activeOnly, CancellationToken cancellationToken = default)
    {
        await EnsureDefaultSourcesAsync(cancellationToken);
        var q = _db.PointsSources.AsNoTracking().AsQueryable();
        if (activeOnly)
            q = q.Where(s => s.IsActive);
        var rows = await q.OrderBy(s => s.SortOrder).ThenBy(s => s.Code).ToListAsync(cancellationToken);
        return rows.Select(s => new PointsSourceDto
        {
            PointsSourceID = s.PointsSourceID,
            Code = s.Code,
            DisplayName = s.DisplayName,
            Description = s.Description,
            SortOrder = s.SortOrder,
            IsActive = s.IsActive,
        }).ToList();
    }

    public async Task<IReadOnlyList<PointsRuleDto>> ListRulesAsync(PointsRuleFilterDto filter, CancellationToken cancellationToken = default)
    {
        filter ??= new PointsRuleFilterDto();
        var q = _db.PointsRules.AsNoTracking().AsQueryable();
        if (filter.SchoolID is > 0)
        {
            q = q.Where(r => r.SchoolID == null || r.SchoolID == filter.SchoolID);
            var activeYear = await GetActiveYearIdForSchoolAsync(filter.SchoolID.Value, cancellationToken);
            if (activeYear is > 0)
                q = q.Where(r => r.AcademicYearID == null || r.AcademicYearID == activeYear);
        }
        if (filter.PointsSourceID is > 0)
            q = q.Where(r => r.PointsSourceID == filter.PointsSourceID);
        if (filter.ActiveOnly == true)
            q = q.Where(r => r.IsActive);

        var raw = await q
            .OrderByDescending(r => r.Priority)
            .ThenBy(r => r.PointsSourceID)
            .ThenBy(r => r.RuleKey)
            .ToListAsync(cancellationToken);

        var sourceCodes = await _db.PointsSources.AsNoTracking()
            .Where(s => raw.Select(x => x.PointsSourceID).Distinct().Contains(s.PointsSourceID))
            .ToDictionaryAsync(s => s.PointsSourceID, s => s.Code, cancellationToken);

        return raw.Select(r => new PointsRuleDto
        {
            PointsRuleID = r.PointsRuleID,
            SchoolID = r.SchoolID,
            AcademicYearID = r.AcademicYearID,
            PointsSourceID = r.PointsSourceID,
            PointsSourceCode = sourceCodes.GetValueOrDefault(r.PointsSourceID, string.Empty),
            RuleKey = r.RuleKey,
            DeltaPoints = r.DeltaPoints,
            Priority = r.Priority,
            IsActive = r.IsActive,
            EffectiveFromUtc = r.EffectiveFromUtc,
            EffectiveToUtc = r.EffectiveToUtc,
        }).ToList();
    }

    public async Task<int> CreateRuleAsync(PointsRuleWriteDto dto, CancellationToken cancellationToken = default)
    {
        int? yearId = null;
        if (dto.SchoolID is int sid && sid > 0)
        {
            yearId = await GetActiveYearIdForSchoolAsync(sid, cancellationToken);
            if (yearId is not > 0)
                throw new InvalidOperationException("No academic year found for this school.");
        }

        var row = new PointsRule
        {
            SchoolID = dto.SchoolID,
            AcademicYearID = yearId,
            PointsSourceID = dto.PointsSourceID,
            RuleKey = string.IsNullOrWhiteSpace(dto.RuleKey) ? "*" : dto.RuleKey.Trim(),
            DeltaPoints = dto.DeltaPoints,
            Priority = dto.Priority,
            IsActive = dto.IsActive,
            EffectiveFromUtc = dto.EffectiveFromUtc,
            EffectiveToUtc = dto.EffectiveToUtc,
        };
        _db.PointsRules.Add(row);
        await _db.SaveChangesAsync(cancellationToken);
        return row.PointsRuleID;
    }

    public async Task UpdateRuleAsync(int ruleId, PointsRuleWriteDto dto, int? managerSchoolIdOnly, CancellationToken cancellationToken = default)
    {
        var row = await _db.PointsRules.FirstOrDefaultAsync(r => r.PointsRuleID == ruleId, cancellationToken)
                  ?? throw new InvalidOperationException("Points rule was not found.");
        if (managerSchoolIdOnly is int ms)
        {
            if (row.SchoolID != ms)
                throw new InvalidOperationException("You may only update rules for your school.");
        }

        row.SchoolID = dto.SchoolID;
        if (dto.SchoolID is int sid2 && sid2 > 0)
        {
            var y = await GetActiveYearIdForSchoolAsync(sid2, cancellationToken);
            if (y is not > 0)
                throw new InvalidOperationException("No academic year found for this school.");
            row.AcademicYearID = y;
        }
        else
            row.AcademicYearID = null;
        row.PointsSourceID = dto.PointsSourceID;
        row.RuleKey = string.IsNullOrWhiteSpace(dto.RuleKey) ? "*" : dto.RuleKey.Trim();
        row.DeltaPoints = dto.DeltaPoints;
        row.Priority = dto.Priority;
        row.IsActive = dto.IsActive;
        row.EffectiveFromUtc = dto.EffectiveFromUtc;
        row.EffectiveToUtc = dto.EffectiveToUtc;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<PointsLedgerListItemDto> Items, int TotalCount)> ListLedgerAsync(
        PointsLedgerFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        filter ??= new PointsLedgerFilterDto();
        var take = Math.Clamp(filter.Take, 1, 200);
        var skip = Math.Max(0, filter.Skip);

        var q = _db.PointsLedgers.AsNoTracking().AsQueryable();
        if (filter.SchoolID is > 0)
        {
            q = q.Where(l => l.SchoolID == filter.SchoolID);
            var activeYear = await GetActiveYearIdForSchoolAsync(filter.SchoolID.Value, cancellationToken);
            if (activeYear is > 0)
                q = q.Where(l => l.AcademicYearID == activeYear);
        }
        if (filter.EmployeeProfileID is > 0)
            q = q.Where(l => l.EmployeeProfileID == filter.EmployeeProfileID);
        if (filter.PointsSourceID is > 0)
            q = q.Where(l => l.PointsSourceID == filter.PointsSourceID);
        if (filter.FromUtc is DateTime from)
            q = q.Where(l => l.CreatedAtUtc >= from);
        if (filter.ToUtc is DateTime to)
            q = q.Where(l => l.CreatedAtUtc <= to);

        var total = await q.CountAsync(cancellationToken);

        var ledgerRows = await q
            .OrderByDescending(l => l.CreatedAtUtc)
            .ThenByDescending(l => l.PointsLedgerID)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        var txIds = ledgerRows.Select(l => l.PointsTransactionID).Distinct().ToList();
        var txs = await _db.PointsTransactions.AsNoTracking()
            .Where(t => txIds.Contains(t.PointsTransactionID))
            .ToDictionaryAsync(t => t.PointsTransactionID, cancellationToken);

        var sourceIds = ledgerRows.Select(l => l.PointsSourceID).Distinct().ToList();
        var sourceCodes = await _db.PointsSources.AsNoTracking()
            .Where(s => sourceIds.Contains(s.PointsSourceID))
            .ToDictionaryAsync(s => s.PointsSourceID, s => s.Code, cancellationToken);

        var empIds = ledgerRows.Select(l => l.EmployeeProfileID).Distinct().ToList();
        var empNames = await _db.EmployeeProfiles.AsNoTracking()
            .Where(e => empIds.Contains(e.EmployeeProfileID))
            .ToDictionaryAsync(e => e.EmployeeProfileID, e => FormatPersonName(e.FullName), cancellationToken);

        var page = ledgerRows.Select(l =>
        {
            txs.TryGetValue(l.PointsTransactionID, out var tr);
            return new PointsLedgerListItemDto
            {
                PointsLedgerID = l.PointsLedgerID,
                PointsTransactionID = l.PointsTransactionID,
                EmployeeProfileID = l.EmployeeProfileID,
                EmployeeDisplayName = empNames.GetValueOrDefault(l.EmployeeProfileID),
                SchoolID = l.SchoolID,
                AcademicYearID = l.AcademicYearID,
                PointsSourceCode = sourceCodes.GetValueOrDefault(l.PointsSourceID, string.Empty),
                PointsRuleID = l.PointsRuleID,
                DeltaPoints = l.DeltaPoints,
                Memo = l.Memo,
                CreatedAtUtc = l.CreatedAtUtc,
                CorrelationEntityType = tr?.CorrelationEntityType,
                CorrelationEntityID = tr?.CorrelationEntityID,
                IdempotencyKey = tr?.IdempotencyKey,
            };
        }).ToList();

        return (page, total);
    }

    public async Task<PointsBalanceDto?> GetBalanceAsync(int employeeProfileId, int schoolId, CancellationToken cancellationToken = default)
    {
        var academicYearId = await GetActiveYearIdForSchoolAsync(schoolId, cancellationToken)
            ?? throw new InvalidOperationException("No academic year found for this school.");
        var row = await _db.PointsBalanceSnapshots.AsNoTracking()
            .FirstOrDefaultAsync(
                b => b.EmployeeProfileID == employeeProfileId && b.SchoolID == schoolId && b.AcademicYearID == academicYearId,
                cancellationToken);
        if (row == null)
            return null;
        return new PointsBalanceDto
        {
            EmployeeProfileID = row.EmployeeProfileID,
            SchoolID = row.SchoolID,
            AcademicYearID = row.AcademicYearID,
            TotalPoints = row.TotalPoints,
            UpdatedAtUtc = row.UpdatedAtUtc,
        };
    }

    private async Task<PointsRule?> ResolveRuleAsync(
        int sourceId,
        int schoolId,
        int yearId,
        string ruleKey,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        var key = string.IsNullOrWhiteSpace(ruleKey) ? "*" : ruleKey.Trim();

        var rules = await _db.PointsRules
            .AsNoTracking()
            .Where(r => r.IsActive && r.PointsSourceID == sourceId)
            .Where(r => r.SchoolID == null || r.SchoolID == schoolId)
            .Where(r => r.AcademicYearID == null || r.AcademicYearID == yearId)
            .Where(r => r.RuleKey == key || r.RuleKey == "*")
            .Where(r => (r.EffectiveFromUtc == null || r.EffectiveFromUtc <= utcNow)
                        && (r.EffectiveToUtc == null || r.EffectiveToUtc >= utcNow))
            .ToListAsync(cancellationToken);

        return rules
            .OrderByDescending(r => string.Equals(r.RuleKey, key, StringComparison.Ordinal) ? 1 : 0)
            .ThenByDescending(r => r.Priority)
            .ThenByDescending(r => r.SchoolID != null ? 1 : 0)
            .ThenByDescending(r => r.AcademicYearID != null ? 1 : 0)
            .FirstOrDefault();
    }

    public async Task<PostCentralPointsResultDto> PostAsync(PostCentralPointsDto dto, int? postedByEmployeeProfileId, CancellationToken cancellationToken = default)
    {
        if (dto.EmployeeProfileID <= 0 || dto.SchoolID <= 0)
            throw new InvalidOperationException("Employee and school are required.");

        var yearId = await GetActiveYearIdForSchoolAsync(dto.SchoolID, cancellationToken)
            ?? throw new InvalidOperationException("No academic year found for this school.");

        var code = (dto.PointsSourceCode ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(code))
            throw new InvalidOperationException("Points source code is required.");

        await EnsureDefaultSourcesAsync(cancellationToken);

        var source = await _db.PointsSources.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Code == code && s.IsActive, cancellationToken)
            ?? throw new InvalidOperationException($"Unknown or inactive points source: {code}.");

        var empOk = await _db.EmployeeProfiles.AsNoTracking()
            .AnyAsync(e => e.EmployeeProfileID == dto.EmployeeProfileID && e.SchoolID == dto.SchoolID, cancellationToken);
        if (!empOk)
            throw new InvalidOperationException("Employee profile was not found for this school.");

        if (!string.IsNullOrWhiteSpace(dto.IdempotencyKey))
        {
            var existing = await _db.PointsTransactions
                .Include(t => t.LedgerLines)
                .FirstOrDefaultAsync(t => t.IdempotencyKey == dto.IdempotencyKey, cancellationToken);
            if (existing != null)
            {
                var line = existing.LedgerLines.FirstOrDefault();
                var bal = await _db.PointsBalanceSnapshots.AsNoTracking()
                    .FirstOrDefaultAsync(
                        b => b.EmployeeProfileID == dto.EmployeeProfileID && b.SchoolID == dto.SchoolID && b.AcademicYearID == existing.AcademicYearID,
                        cancellationToken);
                return new PostCentralPointsResultDto
                {
                    PointsTransactionID = existing.PointsTransactionID,
                    PointsLedgerID = line?.PointsLedgerID ?? 0,
                    AppliedDeltaPoints = line?.DeltaPoints ?? 0,
                    MatchedPointsRuleID = line?.PointsRuleID,
                    NewBalanceTotal = bal?.TotalPoints ?? 0,
                    WasIdempotentReplay = true,
                };
            }
        }

        var utcNow = DateTime.UtcNow;
        int delta;
        PointsRule? matchedRule = null;
        if (dto.OverrideDeltaPoints is int ov)
        {
            delta = ov;
        }
        else
        {
            matchedRule = await ResolveRuleAsync(source.PointsSourceID, dto.SchoolID, yearId, dto.RuleKey, utcNow, cancellationToken);
            if (matchedRule == null)
                throw new InvalidOperationException(
                    $"No active points rule matched for source {code}, key '{dto.RuleKey ?? "*"}', school {dto.SchoolID}, year {yearId}.");
            delta = matchedRule.DeltaPoints;
        }

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var trx = new PointsTransaction
            {
                SchoolID = dto.SchoolID,
                AcademicYearID = yearId,
                PostedAtUtc = utcNow,
                PostedByEmployeeProfileID = postedByEmployeeProfileId,
                IdempotencyKey = string.IsNullOrWhiteSpace(dto.IdempotencyKey) ? null : dto.IdempotencyKey.Trim(),
                CorrelationEntityType = string.IsNullOrWhiteSpace(dto.CorrelationEntityType) ? null : dto.CorrelationEntityType.Trim(),
                CorrelationEntityID = dto.CorrelationEntityID,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
            };
            _db.PointsTransactions.Add(trx);
            await _db.SaveChangesAsync(cancellationToken);

            var ledger = new PointsLedger
            {
                PointsTransactionID = trx.PointsTransactionID,
                EmployeeProfileID = dto.EmployeeProfileID,
                SchoolID = dto.SchoolID,
                AcademicYearID = yearId,
                PointsSourceID = source.PointsSourceID,
                PointsRuleID = matchedRule?.PointsRuleID,
                DeltaPoints = delta,
                Memo = string.IsNullOrWhiteSpace(dto.Memo) ? null : dto.Memo.Trim(),
                CreatedAtUtc = utcNow,
            };
            _db.PointsLedgers.Add(ledger);
            await _db.SaveChangesAsync(cancellationToken);

            var snap = await _db.PointsBalanceSnapshots
                .FirstOrDefaultAsync(
                    b => b.EmployeeProfileID == dto.EmployeeProfileID && b.SchoolID == dto.SchoolID && b.AcademicYearID == yearId,
                    cancellationToken);
            if (snap == null)
            {
                snap = new PointsBalanceSnapshot
                {
                    EmployeeProfileID = dto.EmployeeProfileID,
                    SchoolID = dto.SchoolID,
                    AcademicYearID = yearId,
                    TotalPoints = delta,
                    UpdatedAtUtc = utcNow,
                    LastPointsLedgerID = ledger.PointsLedgerID,
                };
                _db.PointsBalanceSnapshots.Add(snap);
            }
            else
            {
                snap.TotalPoints += delta;
                snap.UpdatedAtUtc = utcNow;
                snap.LastPointsLedgerID = ledger.PointsLedgerID;
            }

            await _db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            return new PostCentralPointsResultDto
            {
                PointsTransactionID = trx.PointsTransactionID,
                PointsLedgerID = ledger.PointsLedgerID,
                AppliedDeltaPoints = delta,
                MatchedPointsRuleID = matchedRule?.PointsRuleID,
                NewBalanceTotal = snap.TotalPoints,
                WasIdempotentReplay = false,
            };
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<int> RebuildBalanceSnapshotAsync(int employeeProfileId, int schoolId, CancellationToken cancellationToken = default)
    {
        var academicYearId = await GetActiveYearIdForSchoolAsync(schoolId, cancellationToken)
            ?? throw new InvalidOperationException("No academic year found for this school.");
        var total = await _db.PointsLedgers.AsNoTracking()
            .Where(l => l.EmployeeProfileID == employeeProfileId && l.SchoolID == schoolId && l.AcademicYearID == academicYearId)
            .SumAsync(l => l.DeltaPoints, cancellationToken);

        var lastId = await _db.PointsLedgers.AsNoTracking()
            .Where(l => l.EmployeeProfileID == employeeProfileId && l.SchoolID == schoolId && l.AcademicYearID == academicYearId)
            .OrderByDescending(l => l.PointsLedgerID)
            .Select(l => (int?)l.PointsLedgerID)
            .FirstOrDefaultAsync(cancellationToken);

        var utc = DateTime.UtcNow;
        var snap = await _db.PointsBalanceSnapshots
            .FirstOrDefaultAsync(
                b => b.EmployeeProfileID == employeeProfileId && b.SchoolID == schoolId && b.AcademicYearID == academicYearId,
                cancellationToken);
        if (snap == null)
        {
            snap = new PointsBalanceSnapshot
            {
                EmployeeProfileID = employeeProfileId,
                SchoolID = schoolId,
                AcademicYearID = academicYearId,
                TotalPoints = total,
                UpdatedAtUtc = utc,
                LastPointsLedgerID = lastId,
            };
            _db.PointsBalanceSnapshots.Add(snap);
        }
        else
        {
            snap.TotalPoints = total;
            snap.UpdatedAtUtc = utc;
            snap.LastPointsLedgerID = lastId;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return total;
    }
}
