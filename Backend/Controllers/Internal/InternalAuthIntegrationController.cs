using Backend.DTOS.Internal;
using Backend.Interfaces;
using Backend.Models.Master;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.Internal;

[Route("api/internal/auth")]
[ApiController]
[AllowAnonymous]
public sealed class InternalAuthIntegrationController : ControllerBase
{
    private readonly IMonolithAuthIntegrationService _authIntegration;

    public InternalAuthIntegrationController(IMonolithAuthIntegrationService authIntegration)
    {
        _authIntegration = authIntegration;
    }

    [HttpPost("login-enrichment")]
    public async Task<ActionResult<LoginEnrichmentResponseDto>> LoginEnrichment(
        [FromBody] LoginEnrichmentRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.UserType))
            return BadRequest(new { message = "UserId and UserType are required." });

        var result = await _authIntegration.GetLoginEnrichmentAsync(
            request.UserId,
            request.UserType,
            request.RequestedTenantId,
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("tenant-summaries/{userId}")]
    public async Task<IActionResult> TenantSummaries(string userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new { message = "UserId is required." });

        var list = await _authIntegration.GetTenantSummariesAsync(userId, cancellationToken);
        return Ok(list);
    }

    [HttpGet("membership/{userId}/{tenantId:int}")]
    public async Task<IActionResult> Membership(string userId, int tenantId, CancellationToken cancellationToken)
    {
        var mem = await _authIntegration.GetMembershipAsync(userId, tenantId, cancellationToken);
        if (mem == null)
            return NotFound();
        return Ok(mem);
    }

    [HttpPost("touch-tenant-access")]
    public async Task<IActionResult> TouchTenantAccess(
        [FromBody] TouchTenantAccessRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            return BadRequest(new { message = "UserId is required." });

        await _authIntegration.TouchTenantAccessAsync(request.UserId, request.TenantId, cancellationToken);
        return Ok(new { message = "Tenant access updated." });
    }

    [HttpPost("ensure-user-tenant")]
    public async Task<IActionResult> EnsureUserTenant(
        [FromBody] EnsureUserTenantRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            return BadRequest(new { message = "UserId is required." });

        await _authIntegration.EnsureUserTenantAsync(
            request.UserId,
            request.TenantId,
            request.TenantRole,
            cancellationToken);
        return Ok(new { message = "User tenant ensured." });
    }

    [HttpGet("school-role/{userId}/{tenantId:int}")]
    public async Task<IActionResult> SchoolRole(string userId, int tenantId, CancellationToken cancellationToken)
    {
        var role = await _authIntegration.ResolveSchoolRoleKeyAsync(userId, tenantId, cancellationToken);
        if (string.IsNullOrEmpty(role))
            return NotFound();
        return Ok(new { schoolRole = role });
    }
}
