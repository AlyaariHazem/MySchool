namespace Backend.DTOS.School.Auth;

/// <summary>Filters for POST /auth/PendingRequests. All properties optional.</summary>
public class PendingRegistrationRequestsFilterDto
{
    /// <summary>Inclusive lower bound on <see cref="Models.Master.RegistrationRequest.CreatedAt"/> (UTC).</summary>
    public DateTime? CreatedFromUtc { get; set; }

    /// <summary>Inclusive upper bound on <see cref="Models.Master.RegistrationRequest.CreatedAt"/> (UTC).</summary>
    public DateTime? CreatedToUtc { get; set; }

    /// <summary>Case-insensitive substring match on gender.</summary>
    public string? Gender { get; set; }

    /// <summary>Substring match on display phone (and normalized digits when applicable).</summary>
    public string? PhoneNumberContains { get; set; }

    /// <summary>Exact tenant/school id.</summary>
    public int? TenantId { get; set; }

    /// <summary>Case-insensitive substring match on school name (master Tenants).</summary>
    public string? SchoolNameContains { get; set; }
}
