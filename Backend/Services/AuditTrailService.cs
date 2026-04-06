using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Backend.Services;

public sealed class AuditTrailService : IAuditTrailService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TenantInfo _tenantInfo;
    private readonly TenantDbContext _tenantDb;
    private readonly ILogger<AuditTrailService> _logger;

    public AuditTrailService(
        IHttpContextAccessor httpContextAccessor,
        TenantInfo tenantInfo,
        TenantDbContext tenantDb,
        ILogger<AuditTrailService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _tenantInfo = tenantInfo;
        _tenantDb = tenantDb;
        _logger = logger;
    }

    public async Task RecordAsync(string category, string action, object? details = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(category))
            category = "General";
        if (string.IsNullOrWhiteSpace(action))
            action = "Unknown";

        var (userId, displayName) = ResolveActor();
        var correlationId = _httpContextAccessor.HttpContext?.TraceIdentifier;
        var detailsJson = details == null ? null : JsonSerializer.Serialize(details, JsonOptions);

        _logger.LogInformation(
            "AUDIT {AuditCategory} {AuditAction} at {TimestampUtc:o} by {ActorUserId} ({ActorDisplayName}) TenantId={TenantId} CorrelationId={CorrelationId} Details={AuditDetails}",
            category,
            action,
            DateTime.UtcNow,
            userId ?? "(anonymous)",
            displayName ?? "",
            _tenantInfo.TenantId,
            correlationId ?? "",
            detailsJson ?? "");

        if (string.IsNullOrWhiteSpace(_tenantInfo.ConnectionString))
            return;

        try
        {
            _tenantDb.AuditLogs.Add(new AuditLog
            {
                Category = category.Length > 128 ? category[..128] : category,
                Action = action.Length > 256 ? action[..256] : action,
                ActorUserId = string.IsNullOrEmpty(userId) ? null : (userId.Length > 450 ? userId[..450] : userId),
                ActorDisplayName = string.IsNullOrEmpty(displayName)
                    ? null
                    : (displayName.Length > 512 ? displayName[..512] : displayName),
                DetailsJson = detailsJson,
                CreatedAtUtc = DateTime.UtcNow,
                TenantId = _tenantInfo.TenantId,
                CorrelationId = string.IsNullOrEmpty(correlationId)
                    ? null
                    : (correlationId.Length > 128 ? correlationId[..128] : correlationId)
            });

            await _tenantDb.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to persist audit row for {AuditCategory}/{AuditAction}", category, action);
        }
    }

    private (string? UserId, string? DisplayName) ResolveActor()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
            return (null, null);

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");

        var name = user.FindFirstValue(ClaimTypes.Name)
            ?? user.FindFirstValue("name")
            ?? user.Identity.Name;

        var email = user.FindFirstValue(ClaimTypes.Email);
        var display = !string.IsNullOrWhiteSpace(name) ? name : email;
        return (userId, display);
    }
}
