using System.Security.Claims;
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

            // Check if user is ADMIN
            var userType = context.User.FindFirstValue("UserType");
            bool isAdmin = context.User.IsInRole("ADMIN") || (userType != null && userType.Equals("ADMIN", StringComparison.OrdinalIgnoreCase));

            // If ADMIN user, use admin database connection for ALL endpoints
            // This allows ADMIN to access all data from the School Database (master database)
            if (isAdmin)
            {
                var adminConnectionString = _configuration.GetConnectionString("SqlAdminConnection");
                if (!string.IsNullOrWhiteSpace(adminConnectionString))
                {
                    tenantInfo.ConnectionString = adminConnectionString;
                    tenantInfo.TenantId = null; // No tenant ID for admin users accessing admin database
                }
                else
                {
                    throw new InvalidOperationException("Admin connection string (SqlAdminConnection) is not configured.");
                }
                await _next(context);
                return;
            }

            // For non-admin users, resolve tenant from JWT claim
            // Non-admin users MUST have a TenantId claim to access tenant-specific data
            var tenantIdStr = context.User.FindFirstValue("TenantId");
            if (!int.TryParse(tenantIdStr, out var tenantId))
            {
                // If no TenantId claim exists, this is an error for non-admin users
                // They need to have a tenant assigned to access tenant-specific endpoints
                throw new InvalidOperationException(
                    $"User '{context.User.Identity?.Name}' does not have a TenantId claim. " +
                    "Non-admin users must be associated with a tenant to access tenant-specific data.");
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
                    throw new InvalidOperationException($"Tenant {tenantId} not found or connection string is missing.");

                cache.Set(cacheKey, cs, TimeSpan.FromMinutes(30));
            }

            // Set the tenant-specific connection string
            tenantInfo.ConnectionString = cs;

            await _next(context);
        }
    }
}
