using Backend.DTOS;

namespace Backend.DTOS.School.Auth;

/// <summary>Body for <c>POST /auth/ApproveRequest/{id}</c> when approving a <c>STUDENT</c> registration.</summary>
public class ApproveRegistrationRequestDto
{
    /// <summary>Required for student enrollment: division (includes class via FK).</summary>
    public int? DivisionID { get; set; }

    /// <summary>If set, link the new student to this guardian (must exist in the school tenant DB).</summary>
    public int? ExistingGuardianId { get; set; }

    /// <summary>Financial link amount (same meaning as add-student flow).</summary>
    public decimal Amount { get; set; }

    // New guardian (when <see cref="ExistingGuardianId"/> is null or 0)
    public string? GuardianEmail { get; set; }
    public string? GuardianPassword { get; set; }
    public string? GuardianFullName { get; set; }
    public string? GuardianPhone { get; set; }
    public string? GuardianGender { get; set; }
    public string? GuardianAddress { get; set; }
    public string? GuardianType { get; set; }
    public DateTime? GuardianDOB { get; set; }

    /// <summary>Optional overrides; otherwise name is parsed from registration full name / username.</summary>
    public string? StudentFirstName { get; set; }
    public string? StudentMiddleName { get; set; }
    public string? StudentLastName { get; set; }

    public List<DisCount>? Discounts { get; set; }
}
