using MySchool.Contracts.Auth;
using MySchool.Identity.Grpc;

namespace MySchool.WebBff.GrpcServices;

public interface IIdentityGrpcGateway
{
    Task<LoginResponse> LoginAsync(LoginDto request, CancellationToken cancellationToken = default);
    Task<RegisterResponse> RegisterAsync(RegisterDto request, CancellationToken cancellationToken = default);
    Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<GetCurrentUserResponse> GetCurrentUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<GetUsersResponse> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<GetRolesResponse> GetRolesAsync(CancellationToken cancellationToken = default);
}
