using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MySchool.IdentityService.Entities;
using MySchool.IdentityService.Mapping;

namespace MySchool.IdentityService.Features.Users.GetUsers;

public sealed class GetUsersHandler
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUsersHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<GetUsersResponse> HandleAsync(GetUsersQuery query, CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users.AsNoTracking().ToListAsync(cancellationToken);
        return new GetUsersResponse
        {
            Users = users.Select(UserAccountMapper.ToDto).ToList()
        };
    }
}
