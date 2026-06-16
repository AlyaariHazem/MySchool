using MySchool.Identity.Grpc;

namespace MySchool.WebBff.Common.DTOs.Identity;

public static class UserListItemMapper
{
    public static IEnumerable<UserListItemDto> FromGrpc(GetUsersResponse result) =>
        result.Users.Select(u => new UserListItemDto
        {
            Id = u.Id,
            UserName = u.UserName,
            Email = u.Email,
            UserType = u.UserType,
            PhoneNumber = u.PhoneNumber,
            Address = u.Address,
            Gender = u.Gender,
            DateOfBirth = u.DateOfBirth,
            HireDate = u.HireDate
        });
}

public sealed class UserListItemDto
{
    public string Id { get; init; } = default!;
    public string UserName { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string UserType { get; init; } = default!;
    public string PhoneNumber { get; init; } = default!;
    public string Address { get; init; } = default!;
    public string Gender { get; init; } = default!;
    public string DateOfBirth { get; init; } = default!;
    public string HireDate { get; init; } = default!;
}
