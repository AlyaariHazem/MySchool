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
                // Public job board: anonymous callers send X-Tenant-Id (or ?tenantId=) so tenant-scoped APIs can run.
                // Fallback: PublicRecruitment:DefaultTenantId in appsettings (single-school deployments).
                if (IsAnonymousPublicJobBoardApi(context.Request.Path, context.Request.Method))
                {
                    if (!TryGetAnonymousTenantId(context, out var anonTenantId))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(JsonSerializer.Serialize(new
                        {
                            error = "TenantRequired",
                            message =
                                "Public job board requests need a school (tenant). Send header X-Tenant-Id, add ?tenantId= to the API URL, or set PublicRecruitment:DefaultTenantId in appsettings.",
                        }));
                        return;
                    }

                    await TrySetTenantConnectionAsync(tenantInfo, anonTenantId, adminDb, cache);
                }

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

        /// <summary>Anonymous APIs used by the public job postings UI (browse + apply).</summary>
        private static bool IsAnonymousPublicJobBoardApi(PathString path, string method)
        {
            var m = method.ToUpperInvariant();
            var p = path.Value ?? "";

            if (path.StartsWithSegments("/api/recruitment/job-postings", StringComparison.OrdinalIgnoreCase))
                return m == "GET";

            if (p.Equals("/api/recruitment/job-applications", StringComparison.OrdinalIgnoreCase))
                return m == "POST";

            if (path.StartsWithSegments("/api/year", StringComparison.OrdinalIgnoreCase))
                return m == "GET";

            if (path.StartsWithSegments("/api/employees/job-types", StringComparison.OrdinalIgnoreCase))
                return m == "GET";

            if (path.StartsWithSegments("/api/school", StringComparison.OrdinalIgnoreCase))
                return m == "GET";

            return false;
        }

        private bool TryGetAnonymousTenantId(HttpContext context, out int tenantId)
        {
            tenantId = 0;
            if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerVals))
            {
                var hv = headerVals.FirstOrDefault();
                if (int.TryParse(hv, out var h) && h > 0)
                {
                    tenantId = h;
                    return true;
                }
            }

            if (context.Request.Query.TryGetValue("tenantId", out var qv))
            {
                if (int.TryParse(qv.FirstOrDefault(), out var q) && q > 0)
                {
                    tenantId = q;
                    return true;
                }
            }

            var def = _configuration["PublicRecruitment:DefaultTenantId"];
            if (int.TryParse(def, out var d) && d > 0)
            {
                tenantId = d;
                return true;
            }

            return false;
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
