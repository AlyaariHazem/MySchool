using System.Collections.Generic;

namespace Backend.DTOS.School.Auth;

public class PendingRegistrationRequestDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string? FullName { get; set; }
    public string Gender { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string RequestedRole { get; set; } = default!;
    public int TenantId { get; set; }
    public string SchoolName { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public List<RegistrationAttachmentDto> Attachments { get; set; } = new();
}
