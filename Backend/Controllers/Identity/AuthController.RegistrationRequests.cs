using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Backend.Common;
using Backend.DTOS.School.Auth;
using Backend.Models;
using Backend.Models.Master;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

public partial class AuthController
{
    private const long MaxAttachmentBytes = 10 * 1024 * 1024;
    private const int MaxAttachmentsPerRequest = 12;
    private static readonly HashSet<string> AllowedAttachmentExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".jpg", ".jpeg", ".png", ".webp" };

    private static readonly HashSet<string> PublicRegistrationRoles =
        new(StringComparer.OrdinalIgnoreCase) { "STUDENT", "GUARDIAN" };

    [HttpGet("PublicSchools")]
    [AllowAnonymous]
    public async Task<IActionResult> PublicSchools()
    {
        var list = await _context.Tenants.AsNoTracking()
            .OrderBy(t => t.SchoolName)
            .Select(t => new PublicSchoolOptionDto
            {
                TenantId = t.TenantId,
                SchoolName = t.SchoolName
            })
            .ToListAsync();

        return Ok(list);
    }

    /// <summary>Multipart: form fields + optional files named <c>attachments</c>.</summary>
    [HttpPost("RequestRegistration")]
    [AllowAnonymous]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(80 * 1024 * 1024)]
    public async Task<IActionResult> RequestRegistration([FromForm] RequestRegistrationFormDto dto)
    {
        if (dto == null)
            return BadRequest(new { message = "Invalid form." });

        if (!string.Equals(dto.Password, dto.ConfirmPassword, StringComparison.Ordinal))
            return BadRequest(new { message = "Password and confirmation do not match." });

        var role = (dto.RequestedRole ?? string.Empty).Trim();
        if (!PublicRegistrationRoles.Contains(role))
            return BadRequest(new { message = "Only STUDENT or GUARDIAN registration is allowed." });

        if (string.IsNullOrWhiteSpace(dto.Gender))
            return BadRequest(new { message = "Gender is required." });

        var tenant = await _context.Tenants.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TenantId == dto.TenantId);
        if (tenant == null)
            return BadRequest(new { message = "School not found." });

        var normName = userManager.NormalizeName(dto.UserName);
        if (string.IsNullOrEmpty(normName))
            return BadRequest(new { message = "Invalid username." });

        var normPhone = RegistrationPhoneHelper.NormalizeDigits(dto.PhoneNumber);
        if (normPhone.Length < 8)
            return BadRequest(new { message = "Enter a valid phone number." });

        var syntheticEmail = $"{normPhone}@phone.registration.local";
        var normEmail = userManager.NormalizeEmail(syntheticEmail);
        if (string.IsNullOrEmpty(normEmail))
            return BadRequest(new { message = "Could not build account email." });

        DateTime? dob = null;
        if (!string.IsNullOrWhiteSpace(dto.DateOfBirth))
        {
            if (!DateTime.TryParse(dto.DateOfBirth, System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                    out var parsed))
                return BadRequest(new { message = "Invalid date of birth. Use yyyy-MM-dd." });
            dob = parsed.Date;
        }

        if (await userManager.Users.AnyAsync(u => u.NormalizedUserName == normName || u.NormalizedEmail == normEmail))
            return Conflict(new { message = "Username or phone is already registered." });

        if (await userManager.Users.AnyAsync(u => u.PhoneNumberNormalized == normPhone))
            return Conflict(new { message = "This phone number is already registered." });

        if (await _context.RegistrationRequests.AnyAsync(r =>
                r.Status == RegistrationRequestStatus.Pending &&
                (r.NormalizedUserName == normName || r.NormalizedPhone == normPhone)))
            return Conflict(new { message = "A pending registration already exists for this username or phone." });

        var formFiles = Request.Form.Files
            .Where(f => f.Name.Equals("attachments", StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (formFiles.Count > MaxAttachmentsPerRequest)
            return BadRequest(new { message = $"At most {MaxAttachmentsPerRequest} files allowed." });

        foreach (var f in formFiles)
        {
            if (f.Length > MaxAttachmentBytes)
                return BadRequest(new { message = $"File {f.FileName} exceeds maximum size." });
            var ext = Path.GetExtension(f.FileName);
            if (string.IsNullOrEmpty(ext) || !AllowedAttachmentExtensions.Contains(ext))
                return BadRequest(new { message = $"File type not allowed: {f.FileName}" });
        }

        var hasher = new PasswordHasher<ApplicationUser>();
        var hash = hasher.HashPassword(null!, dto.Password);

        var row = new RegistrationRequest
        {
            UserName = dto.UserName.Trim(),
            NormalizedUserName = normName,
            PhoneNumber = dto.PhoneNumber.Trim(),
            NormalizedPhone = normPhone,
            Email = syntheticEmail,
            NormalizedEmail = normEmail,
            PasswordHash = hash,
            FullName = string.IsNullOrWhiteSpace(dto.FullName) ? null : dto.FullName.Trim(),
            Gender = dto.Gender.Trim(),
            DateOfBirth = dob,
            RequestedRole = role.ToUpperInvariant(),
            TenantId = dto.TenantId,
            Status = RegistrationRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.RegistrationRequests.Add(row);
            await _context.SaveChangesAsync();

            var webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");
            var uploadsRoot = Path.Combine(webRoot, "uploads", "RegistrationRequests", row.Id.ToString());
            Directory.CreateDirectory(uploadsRoot);

            foreach (var f in formFiles)
            {
                var ext = Path.GetExtension(f.FileName);
                var stored = $"{Guid.NewGuid():N}{ext}";
                var physical = Path.Combine(uploadsRoot, stored);
                await using (var stream = System.IO.File.Create(physical))
                {
                    await f.CopyToAsync(stream);
                }

                var relativePath = $"RegistrationRequests/{row.Id}/{stored}";
                _context.RegistrationRequestAttachments.Add(new RegistrationRequestAttachment
                {
                    RegistrationRequestId = row.Id,
                    OriginalFileName = Path.GetFileName(f.FileName),
                    RelativePath = relativePath,
                    ContentType = f.ContentType ?? string.Empty,
                    SizeBytes = f.Length,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (formFiles.Count > 0)
                await _context.SaveChangesAsync();

            await tx.CommitAsync();
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            TryDeleteRegistrationFolder(row.Id);
            return Conflict(new { message = "Could not create registration request (duplicate username or phone)." });
        }
        catch
        {
            await tx.RollbackAsync();
            TryDeleteRegistrationFolder(row.Id);
            throw;
        }

        return Ok(new { message = "Your request is pending school approval." });
    }

    private void TryDeleteRegistrationFolder(int requestId)
    {
        try
        {
            var dir = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads", "RegistrationRequests", requestId.ToString());
            if (Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
        }
        catch
        {
            // best effort
        }
    }

    [HttpGet("PendingRequests")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<IActionResult> PendingRequests()
    {
        var reviewerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(reviewerId))
            return Unauthorized();

        var q = _context.RegistrationRequests
            .AsNoTracking()
            .Include(r => r.Attachments)
            .Where(r => r.Status == RegistrationRequestStatus.Pending);

        if (!PlatformAdminHelper.IsPlatformAdminUnrestricted(User))
        {
            var allowed = await _context.UserTenants.AsNoTracking()
                .Where(ut => ut.UserId == reviewerId && ut.IsActive)
                .Select(ut => ut.TenantId)
                .ToListAsync();

            q = q.Where(r => allowed.Contains(r.TenantId));
        }

        var rows = await q
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var tenantNames = await _context.Tenants.AsNoTracking()
            .ToDictionaryAsync(t => t.TenantId, t => t.SchoolName);

        var list = rows.Select(r => new PendingRegistrationRequestDto
        {
            Id = r.Id,
            UserName = r.UserName,
            PhoneNumber = r.PhoneNumber,
            FullName = r.FullName,
            Gender = r.Gender,
            DateOfBirth = r.DateOfBirth,
            RequestedRole = r.RequestedRole,
            TenantId = r.TenantId,
            SchoolName = tenantNames.GetValueOrDefault(r.TenantId, string.Empty),
            CreatedAt = r.CreatedAt,
            Attachments = r.Attachments.Select(a => new RegistrationAttachmentDto
            {
                FileName = a.OriginalFileName,
                Url = _apiBase.UploadsFile(a.RelativePath)
            }).ToList()
        }).ToList();

        return Ok(list);
    }

    [HttpPost("ApproveRequest/{id:int}")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public Task<IActionResult> ApproveRequest(int id) => ApproveOrRejectCoreAsync(id, approve: true, reason: null);

    [HttpPost("RejectRequest/{id:int}")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public Task<IActionResult> RejectRequest(int id, [FromBody] RejectRegistrationRequestDto? body) =>
        ApproveOrRejectCoreAsync(id, approve: false, reason: body?.Reason);

    private async Task<IActionResult> ApproveOrRejectCoreAsync(int id, bool approve, string? reason)
    {
        var reviewerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(reviewerId))
            return Unauthorized();

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync<IActionResult>(async () =>
        {
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var req = await _context.RegistrationRequests
                    .FirstOrDefaultAsync(r => r.Id == id && r.Status == RegistrationRequestStatus.Pending);

                if (req == null)
                {
                    await tx.RollbackAsync();
                    return NotFound(new { message = "Pending request not found." });
                }

                if (!await CanReviewerManageTenantAsync(reviewerId, req.TenantId))
                {
                    await tx.RollbackAsync();
                    return Forbid();
                }

                if (!approve)
                {
                    req.Status = RegistrationRequestStatus.Rejected;
                    req.ReviewedAt = DateTime.UtcNow;
                    req.ReviewedByUserId = reviewerId;
                    req.RejectionReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();
                    return Ok(new { message = "Request rejected." });
                }

                if (await userManager.Users.AnyAsync(u =>
                        u.NormalizedUserName == req.NormalizedUserName
                        || (u.NormalizedEmail != null && u.NormalizedEmail == req.NormalizedEmail)
                        || (u.PhoneNumberNormalized != null && u.PhoneNumberNormalized == req.NormalizedPhone)))
                {
                    await tx.RollbackAsync();
                    return Conflict(new { message = "A user with this username or phone already exists." });
                }

                var user = new ApplicationUser
                {
                    UserName = req.UserName,
                    Email = req.Email,
                    NormalizedEmail = req.NormalizedEmail,
                    PhoneNumber = req.PhoneNumber,
                    PhoneNumberNormalized = req.NormalizedPhone,
                    Gender = req.Gender,
                    DateOfBirth = req.DateOfBirth,
                    UserType = req.RequestedRole,
                    EmailConfirmed = true,
                    HireDate = DateTime.UtcNow
                };

                var createResult = await userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    await tx.RollbackAsync();
                    return BadRequest(new
                    {
                        message = "Could not create user.",
                        errors = createResult.Errors.Select(e => e.Description)
                    });
                }

                user.PasswordHash = req.PasswordHash;
                var updateResult = await userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    await userManager.DeleteAsync(user);
                    await tx.RollbackAsync();
                    return BadRequest(new
                    {
                        message = "Could not set password.",
                        errors = updateResult.Errors.Select(e => e.Description)
                    });
                }

                var roleResult = await userManager.AddToRoleAsync(user, req.RequestedRole);
                if (!roleResult.Succeeded)
                {
                    await userManager.DeleteAsync(user);
                    await tx.RollbackAsync();
                    return BadRequest(new
                    {
                        message = "Could not assign role.",
                        errors = roleResult.Errors.Select(e => e.Description)
                    });
                }

                var tenantRole = MapIdentityRoleToTenantRole(req.RequestedRole);
                if (!tenantRole.HasValue)
                {
                    await userManager.DeleteAsync(user);
                    await tx.RollbackAsync();
                    return BadRequest(new { message = "Invalid requested role." });
                }

                _context.UserTenants.Add(new UserTenant
                {
                    UserId = user.Id,
                    TenantId = req.TenantId,
                    TenantRole = tenantRole.Value,
                    IsActive = true,
                    LastAccessedUtc = DateTime.UtcNow
                });

                req.Status = RegistrationRequestStatus.Approved;
                req.ReviewedAt = DateTime.UtcNow;
                req.ReviewedByUserId = reviewerId;
                req.RejectionReason = null;

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return Ok(new { message = "Request approved and user created." });
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        });
    }

    private async Task<bool> CanReviewerManageTenantAsync(string reviewerUserId, int tenantId)
    {
        if (PlatformAdminHelper.IsPlatformAdminUnrestricted(User))
            return true;

        return await _context.UserTenants.AnyAsync(ut =>
            ut.UserId == reviewerUserId && ut.TenantId == tenantId && ut.IsActive);
    }

    private static TenantRole? MapIdentityRoleToTenantRole(string identityRole)
    {
        if (string.Equals(identityRole, "STUDENT", StringComparison.OrdinalIgnoreCase))
            return TenantRole.Student;
        if (string.Equals(identityRole, "GUARDIAN", StringComparison.OrdinalIgnoreCase))
            return TenantRole.Parent;
        return null;
    }
}
