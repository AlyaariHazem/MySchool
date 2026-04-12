namespace Backend.DTOS.School.Auth;

/// <summary>Multipart form fields for public registration (files sent separately as <c>attachments</c>).</summary>
public class RequestRegistrationFormDto
{
    public int TenantId { get; set; }

    public string UserName { get; set; } = default!;

    public string PhoneNumber { get; set; } = default!;

    public string Password { get; set; } = default!;

    public string ConfirmPassword { get; set; } = default!;

    public string RequestedRole { get; set; } = default!;

    public string? FullName { get; set; }

    /// <summary>Arabic or short values, e.g. ذكر / أنثى.</summary>
    public string Gender { get; set; } = default!;

    /// <summary>ISO date yyyy-MM-dd.</summary>
    public string? DateOfBirth { get; set; }
}
