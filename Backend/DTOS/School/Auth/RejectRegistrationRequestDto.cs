using System.ComponentModel.DataAnnotations;

namespace Backend.DTOS.School.Auth;

public class RejectRegistrationRequestDto
{
    [MaxLength(2000)]
    public string? Reason { get; set; }
}
