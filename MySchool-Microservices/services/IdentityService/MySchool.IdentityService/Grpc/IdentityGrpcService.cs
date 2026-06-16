using Google.Protobuf.WellKnownTypes;
using MySchool.Contracts.Users;
using MySchool.Identity.Grpc;
using LoginFeature = MySchool.IdentityService.Features.Auth.Login;
using RegisterFeature = MySchool.IdentityService.Features.Auth.Register;
using RefreshFeature = MySchool.IdentityService.Features.Auth.RefreshToken;
using CurrentUserFeature = MySchool.IdentityService.Features.Auth.GetCurrentUser;
using UsersFeature = MySchool.IdentityService.Features.Users.GetUsers;
using RolesFeature = MySchool.IdentityService.Features.Roles.GetRoles;

namespace MySchool.IdentityService.GrpcServices;

public sealed class IdentityGrpcService : IdentityGrpc.IdentityGrpcBase
{
    private readonly LoginFeature.LoginHandler _loginHandler;
    private readonly RegisterFeature.RegisterHandler _registerHandler;
    private readonly RefreshFeature.RefreshTokenHandler _refreshTokenHandler;
    private readonly CurrentUserFeature.GetCurrentUserHandler _getCurrentUserHandler;
    private readonly UsersFeature.GetUsersHandler _getUsersHandler;
    private readonly RolesFeature.GetRolesHandler _getRolesHandler;

    public IdentityGrpcService(
        LoginFeature.LoginHandler loginHandler,
        RegisterFeature.RegisterHandler registerHandler,
        RefreshFeature.RefreshTokenHandler refreshTokenHandler,
        CurrentUserFeature.GetCurrentUserHandler getCurrentUserHandler,
        UsersFeature.GetUsersHandler getUsersHandler,
        RolesFeature.GetRolesHandler getRolesHandler)
    {
        _loginHandler = loginHandler;
        _registerHandler = registerHandler;
        _refreshTokenHandler = refreshTokenHandler;
        _getCurrentUserHandler = getCurrentUserHandler;
        _getUsersHandler = getUsersHandler;
        _getRolesHandler = getRolesHandler;
    }

    public override async Task<LoginResponse> Login(LoginRequest request, global::Grpc.Core.ServerCallContext context)
    {
        var result = await _loginHandler.HandleAsync(new LoginFeature.LoginCommand
        {
            UserName = request.UserName,
            Password = request.Password,
            UserType = request.UserType,
            TenantId = request.HasTenantId ? request.TenantId : null
        }, context.CancellationToken);

        var response = new LoginResponse
        {
            Success = result.Success,
            ErrorMessage = result.ErrorMessage ?? string.Empty,
            UserName = result.UserName ?? string.Empty,
            Token = result.Token ?? string.Empty,
            Expiration = result.Expiration?.ToString("O") ?? string.Empty,
            SchoolRole = result.SchoolRole ?? string.Empty,
            SchoolName = result.SchoolName ?? string.Empty,
            ManagerName = result.ManagerName ?? string.Empty,
            SchoolId = result.SchoolId ?? 0,
            YearId = result.YearId,
            TenantId = result.TenantId ?? 0,
            TenantDatabase = result.TenantDatabase ?? string.Empty,
            RefreshToken = result.RefreshToken ?? string.Empty
        };

        response.Permissions.AddRange(result.Permissions);
        if (result.Tenants is not null)
        {
            foreach (var tenant in result.Tenants)
            {
                response.Tenants.Add(new TenantSummary
                {
                    TenantId = tenant.TenantId,
                    SchoolName = tenant.SchoolName,
                    TenantRole = (int)tenant.TenantRole,
                    IsDefaultSuggestion = tenant.IsDefaultSuggestion
                });
            }
        }

        if (result.RefreshTokenExpires.HasValue)
            response.RefreshTokenExpires = Timestamp.FromDateTime(
                DateTime.SpecifyKind(result.RefreshTokenExpires.Value, DateTimeKind.Utc));

        return response;
    }

    public override async Task<RegisterResponse> Register(RegisterRequest request, global::Grpc.Core.ServerCallContext context)
    {
        var result = await _registerHandler.HandleAsync(new RegisterFeature.RegisterCommand
        {
            UserName = request.UserName,
            Password = request.Password,
            Email = request.Email,
            UserType = request.UserType
        }, context.CancellationToken);

        return new RegisterResponse
        {
            Success = result.Success,
            ErrorMessage = result.ErrorMessage ?? string.Empty,
            Message = result.Message ?? string.Empty
        };
    }

    public override async Task<RefreshTokenResponse> RefreshToken(RefreshTokenRequest request, global::Grpc.Core.ServerCallContext context)
    {
        var result = await _refreshTokenHandler.HandleAsync(new RefreshFeature.RefreshTokenCommand
        {
            RefreshToken = request.RefreshToken
        }, context.CancellationToken);

        var response = new RefreshTokenResponse
        {
            Success = result.Success,
            ErrorMessage = result.ErrorMessage ?? string.Empty,
            Token = result.Token ?? string.Empty,
            Expiration = result.Expiration?.ToString("O") ?? string.Empty,
            RefreshToken = result.RefreshToken ?? string.Empty
        };

        if (result.RefreshTokenExpires.HasValue)
            response.RefreshTokenExpires = Timestamp.FromDateTime(
                DateTime.SpecifyKind(result.RefreshTokenExpires.Value, DateTimeKind.Utc));

        return response;
    }

    public override async Task<GetCurrentUserResponse> GetCurrentUser(GetCurrentUserRequest request, global::Grpc.Core.ServerCallContext context)
    {
        var result = await _getCurrentUserHandler.HandleAsync(new CurrentUserFeature.GetCurrentUserQuery
        {
            UserId = request.UserId
        }, context.CancellationToken);

        var response = new GetCurrentUserResponse
        {
            Success = result.Success,
            ErrorMessage = result.ErrorMessage ?? string.Empty
        };

        if (result.User is not null)
            response.User = ToProtoUser(result.User);

        response.Roles.AddRange(result.Roles);
        return response;
    }

    public override async Task<GetUsersResponse> GetUsers(GetUsersRequest request, global::Grpc.Core.ServerCallContext context)
    {
        var result = await _getUsersHandler.HandleAsync(new UsersFeature.GetUsersQuery(), context.CancellationToken);
        var response = new GetUsersResponse();
        response.Users.AddRange(result.Users.Select(ToProtoUser));
        return response;
    }

    public override async Task<GetRolesResponse> GetRoles(GetRolesRequest request, global::Grpc.Core.ServerCallContext context)
    {
        var result = await _getRolesHandler.HandleAsync(new RolesFeature.GetRolesQuery(), context.CancellationToken);
        var response = new GetRolesResponse();
        response.Roles.AddRange(result.Roles.Select(r => new RoleItem
        {
            Id = r.Id,
            Name = r.Name
        }));
        return response;
    }

    private static UserAccount ToProtoUser(UserAccountDto dto) => new()
    {
        Id = dto.Id,
        UserName = dto.UserName ?? string.Empty,
        NormalizedUserName = dto.NormalizedUserName ?? string.Empty,
        Email = dto.Email ?? string.Empty,
        NormalizedEmail = dto.NormalizedEmail ?? string.Empty,
        EmailConfirmed = dto.EmailConfirmed,
        PhoneNumber = dto.PhoneNumber ?? string.Empty,
        Address = dto.Address ?? string.Empty,
        Gender = dto.Gender ?? string.Empty,
        DateOfBirth = dto.DateOfBirth?.ToString("O") ?? string.Empty,
        PhoneNumberNormalized = dto.PhoneNumberNormalized ?? string.Empty,
        HireDate = dto.HireDate.ToString("O"),
        UserType = dto.UserType ?? string.Empty
    };
}
