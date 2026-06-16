namespace MySchool.Contracts.Auth;

public class RegisterDto
{
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string UserType { get; set; } = "Admin";
}
