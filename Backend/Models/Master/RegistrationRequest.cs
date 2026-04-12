using System;

namespace Backend.Models.Master;

/// <summary>
/// Public signup request before an Identity user exists; approved rows become real users + <see cref="UserTenant"/>.
/// </summary>
public class RegistrationRequest
{
    public int Id { get; set; }

    public string UserName { get; set; } = default!;
    public string NormalizedUserName { get; set; } = default!;

    /// <summary>Display phone as entered; duplicate checks use <see cref="NormalizedPhone"/>.</summary>
    public string PhoneNumber { get; set; } = default!;

    /// <summary>Digits-only key for uniqueness (same as stored on <see cref="ApplicationUser.PhoneNumberNormalized"/> when approved).</summary>
    public string NormalizedPhone { get; set; } = default!;

    /// <summary>Synthetic email for Identity uniqueness: <c>{NormalizedPhone}@phone.registration.local</c> — not used for login.</summary>
    public string? Email { get; set; }

    public string? NormalizedEmail { get; set; }

    public string Gender { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }

    /// <summary>ASP.NET Identity–compatible password hash (see <see cref="Microsoft.AspNetCore.Identity.PasswordHasher{TUser}"/>).</summary>
    public string PasswordHash { get; set; } = default!;

    public string? FullName { get; set; }

    /// <summary>Identity role name: STUDENT or GUARDIAN only for public flow.</summary>
    public string RequestedRole { get; set; } = default!;

    public int TenantId { get; set; }

    public RegistrationRequestStatus Status { get; set; } = RegistrationRequestStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReviewedAt { get; set; }

    public string? ReviewedByUserId { get; set; }

    public string? RejectionReason { get; set; }

    public Tenant Tenant { get; set; } = default!;

    public ICollection<RegistrationRequestAttachment> Attachments { get; set; } = new List<RegistrationRequestAttachment>();
}
