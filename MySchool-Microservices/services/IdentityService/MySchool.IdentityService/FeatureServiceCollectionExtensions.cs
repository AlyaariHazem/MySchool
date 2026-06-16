using MySchool.IdentityService.Features.Auth.GetCurrentUser;
using MySchool.IdentityService.Features.Auth.Login;
using MySchool.IdentityService.Features.Auth.RefreshToken;
using MySchool.IdentityService.Features.Auth.Register;
using MySchool.IdentityService.Features.Roles.GetRoles;
using MySchool.IdentityService.Features.Users.GetUsers;
using MySchool.IdentityService.Services;

namespace MySchool.IdentityService;

public static class FeatureServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityFeatures(this IServiceCollection services)
    {
        services.AddScoped<IJwtTokenFactory, JwtTokenFactory>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IUserClaimsBuilder, UserClaimsBuilder>();

        services.AddScoped<LoginHandler>();
        services.AddScoped<RegisterHandler>();
        services.AddScoped<RefreshTokenHandler>();
        services.AddScoped<GetCurrentUserHandler>();
        services.AddScoped<GetUsersHandler>();
        services.AddScoped<GetRolesHandler>();

        return services;
    }
}
