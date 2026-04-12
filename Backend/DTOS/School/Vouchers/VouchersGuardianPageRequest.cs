namespace Backend.DTOS.School.Vouchers;

/// <summary>
/// POST body for paginated guardian / student voucher summary rows.
/// </summary>
public class VouchersGuardianPageRequest
{
    /// <summary>Optional; when null or omitted, all guardians matching the year filter are included.</summary>
    public int? GuardianID { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 8;
}
