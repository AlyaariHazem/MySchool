using Microsoft.Extensions.Options;

namespace MySchool.Gateway.OpenApi;

public static class OpenApiProxyExtensions
{
    public static IServiceCollection AddOpenApiProxy(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OpenApiProxyOptions>(configuration.GetSection(OpenApiProxyOptions.SectionName));
        services.AddHttpClient("OpenApiProxy", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(15);
        });
        return services;
    }

    public static WebApplication MapOpenApiProxy(this WebApplication app)
    {
        app.MapGet("/openapi/{service}/{**path}", async (
            string service,
            string path,
            IHttpClientFactory httpClientFactory,
            IOptions<OpenApiProxyOptions> options,
            CancellationToken cancellationToken) =>
        {
            if (!options.Value.Services.TryGetValue(service, out var baseUrl)
                || string.IsNullOrWhiteSpace(baseUrl))
            {
                return Results.NotFound(new
                {
                    message = $"Unknown OpenAPI service '{service}'.",
                    known = options.Value.Services.Keys.ToList()
                });
            }

            baseUrl = baseUrl.TrimEnd('/');
            var targetPath = string.IsNullOrEmpty(path) ? "v1/swagger.json" : path.TrimStart('/');
            var targetUrl = $"{baseUrl}/swagger/{targetPath}";

            try
            {
                var client = httpClientFactory.CreateClient("OpenApiProxy");
                using var response = await client.GetAsync(targetUrl, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync(cancellationToken);
                    return Results.Problem(
                        detail: $"Downstream returned {(int)response.StatusCode} from {targetUrl}. {body}",
                        statusCode: StatusCodes.Status502BadGateway,
                        title: $"Failed to load OpenAPI from {service}");
                }

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return Results.Content(json, "application/json");
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return Results.Problem(
                    detail: $"Could not reach {service} at {baseUrl}. Start that service, then retry. ({ex.Message})",
                    statusCode: StatusCodes.Status502BadGateway,
                    title: $"Service '{service}' is unreachable");
            }
        });

        return app;
    }
}
