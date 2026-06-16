using Microsoft.AspNetCore.Identity;
using MySchool.IdentityService.Entities;
using MySchool.IdentityService.Mapping;

namespace MySchool.IdentityService.Features.Auth.GetCurrentUser;

public sealed class GetCurrentUserHandler
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetCurrentUserHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<GetCurrentUserResponse> HandleAsync(GetCurrentUserQuery query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.UserId))
            return new GetCurrentUserResponse { Success = false, ErrorMessage = "User id is required." };

        var user = await _userManager.FindByIdAsync(query.UserId);
        if (user is null)
            return new GetCurrentUserResponse { Success = false, ErrorMessage = "User not found." };

        var roles = await _userManager.GetRolesAsync(user);
        return new GetCurrentUserResponse
        {
            Success = true,
            User = UserAccountMapper.ToDto(user),
            Roles = roles.ToList()
        };
    }
}
