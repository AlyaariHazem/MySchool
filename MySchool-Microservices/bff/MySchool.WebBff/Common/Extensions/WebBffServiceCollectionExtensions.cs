using MySchool.BuildingBlocks.Authentication;
using MySchool.Identity.Grpc;
using MySchool.WebBff.GrpcServices;
using MySchool.WebBff.Infrastructure.Cookies;

namespace MySchool.WebBff.Common.Extensions;

public static class WebBffServiceCollectionExtensions
{
    public static IServiceCollection AddWebBffServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMySchoolJwtAuthentication(configuration);
        services.AddAuthorization();

        var grpcUrl = configuration["IdentityService:GrpcUrl"] ?? "http://localhost:8083";
        services.AddGrpcClient<IdentityGrpc.IdentityGrpcClient>(options =>
        {
            options.Address = new Uri(grpcUrl);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            EnableMultipleHttp2Connections = true
        });

        services.AddScoped<IIdentityGrpcGateway, IdentityGrpcGateway>();
        services.AddSingleton<IRefreshTokenCookieWriter, RefreshTokenCookieWriter>();

        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"));

        services.AddOpenApiProxy(configuration);

        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
                policy.WithOrigins(
                        configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                        ?? ["http://localhost:4200", "https://localhost:4200"])
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });

        return services;
    }
}
