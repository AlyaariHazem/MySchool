using MySchool.Contracts.Users;
using MySchool.IdentityService.Entities;

namespace MySchool.IdentityService.Mapping;

public static class UserAccountMapper
{
    public static UserAccountDto ToDto(ApplicationUser user) => new()
    {
        Id = user.Id,
        UserName = user.UserName,
        NormalizedUserName = user.NormalizedUserName,
        Email = user.Email,
        NormalizedEmail = user.NormalizedEmail,
        EmailConfirmed = user.EmailConfirmed,
        PasswordHash = user.PasswordHash,
        PhoneNumber = user.PhoneNumber,
        SecurityStamp = user.SecurityStamp,
        ConcurrencyStamp = user.ConcurrencyStamp,
        Address = user.Address,
        Gender = user.Gender,
        DateOfBirth = user.DateOfBirth,
        PhoneNumberNormalized = user.PhoneNumberNormalized,
        HireDate = user.HireDate,
        UserType = user.UserType
    };

    public static void ApplyToEntity(UserAccountDto dto, ApplicationUser user)
    {
        user.UserName = dto.UserName;
        user.NormalizedUserName = dto.NormalizedUserName;
        user.Email = dto.Email;
        user.NormalizedEmail = dto.NormalizedEmail;
        user.EmailConfirmed = dto.EmailConfirmed;
        user.PasswordHash = dto.PasswordHash;
        user.PhoneNumber = dto.PhoneNumber;
        user.SecurityStamp = dto.SecurityStamp;
        user.ConcurrencyStamp = dto.ConcurrencyStamp;
        user.Address = dto.Address;
        user.Gender = dto.Gender;
        user.DateOfBirth = dto.DateOfBirth;
        user.PhoneNumberNormalized = dto.PhoneNumberNormalized;
        user.HireDate = dto.HireDate;
        user.UserType = dto.UserType;
    }

    public static ApplicationUser ToEntity(UserAccountDto dto)
    {
        var user = new ApplicationUser { Id = dto.Id };
        ApplyToEntity(dto, user);
        return user;
    }
}
