using System.Collections.Concurrent;
using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Middleware;

/// <summary>
/// In Development, applies pending <see cref="TenantDbContext"/> EF migrations once per tenant connection string.
/// Prevents "Invalid object name 'JobPostings'" when a new migration was added but the tenant DB was not updated manually.
/// </summary>
public sealed class TenantDevAutoMigrateMiddleware
{
    private static readonly ConcurrentDictionary<string, byte> Migrated = new(StringComparer.Ordinal);
    private static readonly SemaphoreSlim Gate = new(1, 1);

    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _env;

    public TenantDevAutoMigrateMiddleware(RequestDelegate next, IWebHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context, TenantInfo tenantInfo)
    {
        if (_env.IsDevelopment()
            && context.User?.Identity?.IsAuthenticated == true
            && !string.IsNullOrWhiteSpace(tenantInfo.ConnectionString))
        {
            var cs = tenantInfo.ConnectionString;
            if (!Migrated.ContainsKey(cs))
            {
                await Gate.WaitAsync(context.RequestAborted);
                try
                {
                    if (!Migrated.ContainsKey(cs))
                    {
                        var opts = new DbContextOptionsBuilder<TenantDbContext>()
                            .UseTenantSqlServer(cs)
                            .Options;
                        await using var db = new TenantDbContext(opts, tenantInfo);
                        await db.Database.MigrateAsync(context.RequestAborted);
                        Migrated.TryAdd(cs, 1);
                    }
                }
                finally
                {
                    Gate.Release();
                }
            }
        }

        await _next(context);
    }
}
