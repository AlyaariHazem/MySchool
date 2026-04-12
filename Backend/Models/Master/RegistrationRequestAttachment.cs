namespace Backend.Models.Master;

/// <summary>File uploaded with a <see cref="RegistrationRequest"/> (served under wwwroot/uploads).</summary>
public class RegistrationRequestAttachment
{
    public int Id { get; set; }

    public int RegistrationRequestId { get; set; }

    /// <summary>Original client file name.</summary>
    public string OriginalFileName { get; set; } = default!;

    /// <summary>Relative to wwwroot, e.g. uploads/RegistrationRequests/12/guid.pdf</summary>
    public string RelativePath { get; set; } = default!;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public RegistrationRequest RegistrationRequest { get; set; } = default!;
}
