using System.Security.Claims;
using System.Text.Json;
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
            // Skip tenant resolution for unauthenticated requests
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            // Skip tenant resolution for admin endpoints (auth, tenant management)
            var path = context.Request.Path.Value?.ToLower() ?? "";
            if (path.StartsWith("/api/auth") || 
                path.StartsWith("/api/tenant") || 
                (path.StartsWith("/api/manager") && context.User.IsInRole("ADMIN")))
            {
                await _next(context);
                return;
            }

            // Resolve tenant from JWT for all school data. Platform admins use master DB only on /api/tenant etc. (skipped above).
            // Everyone else (including ADMIN role when using school APIs) must have TenantId in the token.
            var tenantIdStr = context.User.FindFirstValue("TenantId");
            if (!int.TryParse(tenantIdStr, out var tenantId))
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

            tenantInfo.TenantId = tenantId;

            // Get connection string for this tenant (with caching)
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

            // Set the tenant-specific connection string
            tenantInfo.ConnectionString = cs;

            await _next(context);
        }
    }
}
