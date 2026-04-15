using System.Security.Claims;
using System.Text.Json;
using Backend.Common;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Backend.Middleware
{
    public sealed class TenantResolutionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public TenantResolutionMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext context, TenantInfo tenantInfo, DatabaseContext adminDb, IMemoryCache cache)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            var path = context.Request.Path.Value?.ToLower() ?? "";
            if (path.StartsWith("/api/auth") || path.StartsWith("/api/tenant"))
            {
                await _next(context);
                return;
            }

            var tenantIdStr = context.User.FindFirstValue("TenantId");
            var hasTenant = int.TryParse(tenantIdStr, out var tenantId);

            // Platform admins can browse master-only routes (school catalog, dashboard aggregates, etc.) without TenantId.
            // School-scoped APIs need a resolved tenant connection; otherwise TenantDbContext is registered with no SQL
            // provider and throws "No database provider has been configured for this DbContext."
            if (PlatformAdminHelper.IsPlatformAdminUnrestricted(context.User))
            {
                if (hasTenant)
                    await TrySetTenantConnectionAsync(tenantInfo, tenantId, adminDb, cache);
                else if (!AllowsPlatformAdminWithoutTenantConnection(path))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        error = "TenantRequired",
                        message =
                            "Select a school first. Platform admins must call POST /api/auth/select-tenant or use a JWT that includes TenantId before using school-scoped APIs (students, classes, grades, etc.)."
                    }));
                    return;
                }

                await _next(context);
                return;
            }

            if (!hasTenant)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    error = "TenantRequired",
                    message = "Select a school (tenant) first. Use POST /api/auth/select-tenant or login with TenantId when you belong to multiple schools."
                }));
                return;
            }

            await TrySetTenantConnectionAsync(tenantInfo, tenantId, adminDb, cache);
            await _next(context);
        }

        private static async Task TrySetTenantConnectionAsync(
            TenantInfo tenantInfo,
            int tenantId,
            DatabaseContext adminDb,
            IMemoryCache cache)
        {
            tenantInfo.TenantId = tenantId;

            var cacheKey = $"tenant:cs:{tenantId}";
            if (!cache.TryGetValue(cacheKey, out string? cs))
            {
                cs = await adminDb.Tenants.AsNoTracking()
                    .Where(t => t.TenantId == tenantId)
                    .Select(t => t.ConnectionString)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrWhiteSpace(cs))
                {
                    throw new InvalidOperationException(
                        $"Tenant {tenantId} not found or connection string is missing in the admin database. " +
                        "If you restored or reset the database, repopulate the Tenants table (and ConnectionString) or log in again after fixing data.");
                }

                cache.Set(cacheKey, cs, TimeSpan.FromMinutes(30));
            }

            tenantInfo.ConnectionString = cs;
        }

        /// <summary>
        /// Routes that only use <see cref="DatabaseContext"/> or create ad hoc <see cref="TenantDbContext"/> instances
        /// (see master-dashboard / school-catalog helpers) — safe when <see cref="TenantInfo.ConnectionString"/> is empty.
        /// </summary>
        private static bool AllowsPlatformAdminWithoutTenantConnection(string path)
        {
            if (path.StartsWith("/api/school", StringComparison.Ordinal))
                return true;
            if (path.StartsWith("/api/dashboard", StringComparison.Ordinal))
                return true;
            if (path.StartsWith("/api/manager", StringComparison.Ordinal))
                return true;
            if (path.StartsWith("/api/rolepermissions", StringComparison.Ordinal))
                return true;
            if (path.StartsWith("/api/databaserestore", StringComparison.Ordinal))
                return true;
            if (path.StartsWith("/api/tenantseed", StringComparison.Ordinal))
                return true;
            return false;
        }
    }
}
