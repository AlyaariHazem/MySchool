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

            // Platform admins may access any route without 403; if the token includes TenantId, resolve DB for that school.
            if (PlatformAdminHelper.IsPlatformAdminUnrestricted(context.User))
            {
                if (hasTenant)
                    await TrySetTenantConnectionAsync(tenantInfo, tenantId, adminDb, cache);
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

    }
}
