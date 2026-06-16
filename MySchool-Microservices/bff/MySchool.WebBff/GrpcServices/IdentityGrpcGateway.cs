using MySchool.Contracts.Auth;
using MySchool.Identity.Grpc;

namespace MySchool.WebBff.GrpcServices;

public sealed class IdentityGrpcGateway : IIdentityGrpcGateway
{
    private readonly IdentityGrpc.IdentityGrpcClient _client;

    public IdentityGrpcGateway(IdentityGrpc.IdentityGrpcClient client)
    {
        _client = client;
    }

    public Task<LoginResponse> LoginAsync(LoginDto request, CancellationToken cancellationToken = default)
    {
        var grpcRequest = new LoginRequest
        {
            UserName = request.UserName,
            Password = request.Password,
            UserType = request.userType
        };
        if (request.TenantId.HasValue)
            grpcRequest.TenantId = request.TenantId.Value;

        return _client.LoginAsync(grpcRequest, cancellationToken: cancellationToken).ResponseAsync;
    }

    public Task<RegisterResponse> RegisterAsync(RegisterDto request, CancellationToken cancellationToken = default) =>
        _client.RegisterAsync(new RegisterRequest
        {
            UserName = request.UserName,
            Password = request.Password,
            Email = request.Email,
            UserType = request.UserType
        }, cancellationToken: cancellationToken).ResponseAsync;

    public Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default) =>
        _client.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = refreshToken }, cancellationToken: cancellationToken).ResponseAsync;

    public Task<GetCurrentUserResponse> GetCurrentUserAsync(string userId, CancellationToken cancellationToken = default) =>
        _client.GetCurrentUserAsync(new GetCurrentUserRequest { UserId = userId }, cancellationToken: cancellationToken).ResponseAsync;

    public Task<GetUsersResponse> GetUsersAsync(CancellationToken cancellationToken = default) =>
        _client.GetUsersAsync(new GetUsersRequest(), cancellationToken: cancellationToken).ResponseAsync;

    public Task<GetRolesResponse> GetRolesAsync(CancellationToken cancellationToken = default) =>
        _client.GetRolesAsync(new GetRolesRequest(), cancellationToken: cancellationToken).ResponseAsync;
}
