using MySchool.Contracts.Users;

namespace MySchool.IdentityService.Features.Users.GetUsers;

public sealed class GetUsersResponse
{
    public IReadOnlyList<UserAccountDto> Users { get; init; } = Array.Empty<UserAccountDto>();
}
