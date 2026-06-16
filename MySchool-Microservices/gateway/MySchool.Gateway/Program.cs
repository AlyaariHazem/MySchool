using Microsoft.Extensions.Options;
using MySchool.Gateway.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddOpenApiProxy(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? ["http://localhost:4200"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

app.UseCors("Frontend");

app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "MySchool API Gateway";
    options.SwaggerEndpoint("/openapi/identity/v1/swagger.json", "Identity Service");
    options.SwaggerEndpoint("/openapi/monolith/v1/swagger.json", "School API (Monolith)");
});

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/health", (IOptions<OpenApiProxyOptions> openApi) =>
{
    return Results.Ok(new
    {
        status = "healthy",
        service = "MySchool.Gateway",
        openApiServices = openApi.Value.Services
    });
});

app.MapOpenApiProxy();
app.MapReverseProxy();

app.Run();
