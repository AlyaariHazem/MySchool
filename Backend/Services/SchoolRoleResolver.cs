using Backend.Common;
using Backend.Data;
using Backend.Interfaces;
using Backend.Models.Master;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class SchoolRoleResolver : ISchoolRoleResolver
{
    private readonly DatabaseContext _admin;

    public SchoolRoleResolver(DatabaseContext admin)
    {
        _admin = admin;
    }

    public async Task<string?> ResolveSchoolRoleKeyAsync(string userId, int tenantId, CancellationToken cancellationToken = default)
    {
        var cs = await _admin.Tenants.AsNoTracking()
            .Where(t => t.TenantId == tenantId)
            .Select(t => t.ConnectionString)
            .FirstOrDefaultAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(cs))
            return null;

        var tenantInfo = new TenantInfo { TenantId = tenantId, ConnectionString = cs };
        var opts = new DbContextOptionsBuilder<TenantDbContext>()
            .UseTenantSqlServer(cs)
            .Options;

        await using var ctx = new TenantDbContext(opts, tenantInfo);

        var staff = await ctx.SchoolStaff.AsNoTracking()
            .Where(s => s.UserID == userId)
            .Select(s => s.StaffRole)
            .FirstOrDefaultAsync(cancellationToken);
        if (!string.IsNullOrEmpty(staff))
            return staff.Trim();

        if (await ctx.Managers.AsNoTracking().AnyAsync(m => m.UserID == userId, cancellationToken))
            return SchoolUserRoleKeys.Manager;
        if (await ctx.Teachers.AsNoTracking().AnyAsync(t => t.UserID == userId, cancellationToken))
            return SchoolUserRoleKeys.Teacher;
        if (await ctx.Students.AsNoTracking().AnyAsync(s => s.UserID == userId, cancellationToken))
            return SchoolUserRoleKeys.Student;
        if (await ctx.Guardians.AsNoTracking().AnyAsync(g => g.UserID == userId, cancellationToken))
            return SchoolUserRoleKeys.Guardian;

        return null;
    }
}
