using Microsoft.AspNetCore.Identity;
using MySchool.IdentityService.Entities;

namespace MySchool.IdentityService.Features.Auth.Register;

public sealed class RegisterHandler
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<RegisterResponse> HandleAsync(RegisterCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.UserName)
            || string.IsNullOrWhiteSpace(command.Password)
            || string.IsNullOrWhiteSpace(command.Email))
        {
            return new RegisterResponse { Success = false, ErrorMessage = "Invalid request data." };
        }

        var user = new ApplicationUser
        {
            UserName = command.UserName,
            Email = command.Email,
            UserType = command.UserType
        };

        var result = await _userManager.CreateAsync(user, command.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return new RegisterResponse { Success = false, ErrorMessage = errors };
        }

        if (!string.IsNullOrEmpty(command.UserType))
            await _userManager.AddToRoleAsync(user, command.UserType);

        return new RegisterResponse
        {
            Success = true,
            Message = "User created successfully."
        };
    }
}
