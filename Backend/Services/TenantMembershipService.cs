using Backend.Data;
using Backend.DTOS;
using Backend.Interfaces;
using Backend.Models.Master;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public sealed class TenantMembershipService : ITenantMembershipService
{
    private readonly DatabaseContext _db;

    public TenantMembershipService(DatabaseContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<UserTenantSummaryDto>> GetTenantSummariesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var rows = await _db.UserTenants.AsNoTracking()
            .Where(ut => ut.UserId == userId && ut.IsActive)
            .Join(_db.Tenants.AsNoTracking(),
                ut => ut.TenantId,
                t => t.TenantId,
                (ut, t) => new { ut, t })
            .OrderByDescending(x => x.ut.LastAccessedUtc)
            .ThenBy(x => x.t.SchoolName)
            .Select(x => new UserTenantSummaryDto
            {
                TenantId = x.t.TenantId,
                SchoolName = x.t.SchoolName,
                TenantRole = x.ut.TenantRole,
                IsDefaultSuggestion = false
            })
            .ToListAsync(cancellationToken);

        for (var i = 0; i < rows.Count; i++)
            rows[i].IsDefaultSuggestion = i == 0;

        return rows;
    }

    public async Task<int?> GetDefaultTenantIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var id = await _db.UserTenants.AsNoTracking()
            .Where(ut => ut.UserId == userId && ut.IsActive)
            .OrderByDescending(ut => ut.LastAccessedUtc)
            .Select(ut => (int?)ut.TenantId)
            .FirstOrDefaultAsync(cancellationToken);

        return id;
    }

    public Task<UserTenant?> GetMembershipAsync(string userId, int tenantId, CancellationToken cancellationToken = default)
    {
        return _db.UserTenants.AsNoTracking()
            .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TenantId == tenantId && ut.IsActive, cancellationToken);
    }

    public async Task<int?> ResolveTenantIdForIssuedTokenAsync(string userId, CancellationToken cancellationToken = default)
    {
        var active = await _db.UserTenants.AsNoTracking()
            .Where(ut => ut.UserId == userId && ut.IsActive)
            .OrderByDescending(ut => ut.LastAccessedUtc)
            .ThenBy(ut => ut.TenantId)
            .ToListAsync(cancellationToken);

        if (active.Count == 0)
            return null;
        if (active.Count == 1)
            return active[0].TenantId;

        var first = active[0];
        return first.LastAccessedUtc != null ? first.TenantId : null;
    }
}
