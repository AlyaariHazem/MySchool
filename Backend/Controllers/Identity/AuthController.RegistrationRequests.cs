using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Backend.Common;
using Backend.DTOS;
using Backend.DTOS.School.Accounts;
using Backend.DTOS.School.Auth;
using Backend.DTOS.School.Guardians;
using Backend.DTOS.School.StudentClassFee;
using Backend.Data;
using Backend.Models;
using Backend.Models.Master;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
    public Task<IActionResult> ApproveRequest(int id, [FromBody] ApproveRegistrationRequestDto? body) =>
        ApproveOrRejectCoreAsync(id, approve: true, reason: null, approveBody: body);

    [HttpPost("RejectRequest/{id:int}")]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public Task<IActionResult> RejectRequest(int id, [FromBody] RejectRegistrationRequestDto? body) =>
        ApproveOrRejectCoreAsync(id, approve: false, reason: body?.Reason, approveBody: null);

    private async Task<IActionResult> ApproveOrRejectCoreAsync(int id, bool approve, string? reason, ApproveRegistrationRequestDto? approveBody)
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

                if (string.Equals(req.RequestedRole, "STUDENT", StringComparison.OrdinalIgnoreCase))
                {
                    var enrollErr = await EnrollApprovedStudentInTenantAsync(req, user, approveBody);
                    if (enrollErr != null)
                    {
                        await userManager.DeleteAsync(user);
                        await tx.RollbackAsync();
                        return enrollErr;
                    }
                }

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

    private async Task<TenantDbContext> CreateTenantDbContextForEnrollmentAsync(int tenantId)
    {
        var row = await _context.Tenants.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TenantId == tenantId);
        if (row == null || string.IsNullOrWhiteSpace(row.ConnectionString))
            throw new InvalidOperationException(
                $"Tenant {tenantId} was not found or has no connection string in the master database.");

        var ti = new TenantInfo { TenantId = tenantId, ConnectionString = row.ConnectionString };
        var ob = new DbContextOptionsBuilder<TenantDbContext>();
        ob.UseTenantSqlServer(row.ConnectionString);
        return new TenantDbContext(ob.Options, ti);
    }

    private async Task<IActionResult?> EnrollApprovedStudentInTenantAsync(
        RegistrationRequest req,
        ApplicationUser user,
        ApproveRegistrationRequestDto? dto)
    {
        if (dto == null || !dto.DivisionID.HasValue || dto.DivisionID.Value <= 0)
            return BadRequest(new { message = "يجب اختيار الشعبة (DivisionID) عند قبول تسجيل طالب." });

        TenantDbContext tenantDb;
        try
        {
            tenantDb = await CreateTenantDbContextForEnrollmentAsync(req.TenantId);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        using var tenantUnitOfWork = ActivatorUtilities.CreateInstance<UnitOfWork>(_serviceProvider, tenantDb);
        var studentManagement = ActivatorUtilities.CreateInstance<StudentManagementService>(
            _serviceProvider, tenantDb, tenantUnitOfWork);

        if (!await studentManagement.DivisionExistsAsync(dto.DivisionID.Value))
            return BadRequest(new { message = "الشعبة غير صالحة لهذه المدرسة." });

        var (first, middle, last) = ParseStudentNameParts(req, dto);
        var student = new Student
        {
            StudentID = 0,
            FullName = new Name
            {
                FirstName = first,
                MiddleName = middle ?? string.Empty,
                LastName = last
            },
            DivisionID = dto.DivisionID.Value,
            StudentDOB = req.DateOfBirth ?? DateTime.UtcNow.Date,
            GuardianID = 0,
            UserID = user.Id,
            ImageURL = null,
            PlaceBirth = null
        };

        List<StudentClassFeeDTO>? fees = dto.Discounts?.Select(d => new StudentClassFeeDTO
        {
            FeeClassID = d.FeeClassID,
            AmountDiscount = d.AmountDiscount,
            NoteDiscount = d.NoteDiscount,
            Mandatory = d.Mandatory,
            StudentID = 0
        }).ToList();

        var asgParam = new AccountStudentGuardian { Amount = dto.Amount };

        try
        {
            if (dto.ExistingGuardianId.HasValue && dto.ExistingGuardianId.Value > 0)
            {
                var g = await tenantUnitOfWork.Guardians.GetGuardianByIdAsync(dto.ExistingGuardianId.Value);
                if (g == null)
                    return NotFound(new { message = "ولي الأمر غير موجود." });

                await studentManagement.EnrollRegistrationApprovedStudentToExistingGuardianAsync(
                    g, user.Id, student, fees, asgParam);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dto.GuardianEmail) || string.IsNullOrWhiteSpace(dto.GuardianFullName))
                    return BadRequest(new { message = "عند عدم اختيار ولي أمر موجود، يلزم البريد والاسم الكامل لولي الأمر الجديد." });

                var guardianUser = new ApplicationUser
                {
                    UserName = "Guardian_" + Guid.NewGuid().ToString("N").Substring(0, 8),
                    Email = dto.GuardianEmail.Trim(),
                    Address = dto.GuardianAddress ?? string.Empty,
                    Gender = string.IsNullOrWhiteSpace(dto.GuardianGender) ? "Male" : dto.GuardianGender.Trim(),
                    PhoneNumber = dto.GuardianPhone ?? string.Empty,
                    UserType = "Guardian",
                    HireDate = DateTime.UtcNow
                };

                var guardian = new Guardian
                {
                    FullName = dto.GuardianFullName.Trim(),
                    Type = dto.GuardianType,
                    GuardianDOB = dto.GuardianDOB ?? DateTime.UtcNow.Date
                };

                var account = new AccountsDTO
                {
                    AccountName = dto.GuardianFullName.Trim(),
                    Note = "",
                    State = true,
                    TypeAccountID = 1
                };

                var pwd = string.IsNullOrWhiteSpace(dto.GuardianPassword) ? "Guardian" : dto.GuardianPassword!;

                await studentManagement.EnrollRegistrationApprovedStudentWithNewGuardianAsync(
                    guardianUser,
                    pwd,
                    guardian,
                    user.Id,
                    student,
                    account,
                    asgParam,
                    fees);
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        return null;
    }

    private static (string First, string Middle, string Last) ParseStudentNameParts(
        RegistrationRequest req,
        ApproveRegistrationRequestDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.StudentFirstName) || !string.IsNullOrWhiteSpace(dto.StudentLastName))
        {
            return (
                dto.StudentFirstName?.Trim() ?? "",
                dto.StudentMiddleName?.Trim() ?? "",
                dto.StudentLastName?.Trim() ?? "");
        }

        if (!string.IsNullOrWhiteSpace(req.FullName))
        {
            var parts = req.FullName.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return (parts[0], "", "");
            if (parts.Length == 2)
                return (parts[0], "", parts[1]);
            return (parts[0], string.Join(" ", parts.Skip(1).Take(parts.Length - 2)), parts[^1]);
        }

        return (req.UserName.Trim(), "", "");
    }
}
