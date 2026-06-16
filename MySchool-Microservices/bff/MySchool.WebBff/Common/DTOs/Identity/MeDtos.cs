using MySchool.Identity.Grpc;

namespace MySchool.WebBff.Common.DTOs.Identity;

public static class MeResponseMapper
{
    public static MeResponseDto ToJson(GetCurrentUserResponse result) => new()
    {
        User = new MeUserDto
        {
            Id = result.User.Id,
            UserName = result.User.UserName,
            Email = result.User.Email,
            UserType = result.User.UserType,
            PhoneNumber = result.User.PhoneNumber,
            Address = result.User.Address,
            Gender = result.User.Gender,
            DateOfBirth = result.User.DateOfBirth,
            HireDate = result.User.HireDate
        },
        Roles = result.Roles
    };
}

public sealed class MeResponseDto
{
    public MeUserDto User { get; init; } = default!;
    public IEnumerable<string> Roles { get; init; } = Array.Empty<string>();
}

public sealed class MeUserDto
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
