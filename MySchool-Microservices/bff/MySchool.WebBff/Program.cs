using Microsoft.Extensions.Options;
using MySchool.WebBff.Common.Extensions;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(listen => listen.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "MySchool Web BFF",
        Version = "v1",
        Description = "HTTP API for Angular; identity via gRPC; school APIs via YARP."
    });
});

builder.Services.AddWebBffServices(builder.Configuration);

var app = builder.Build();

app.UseCors("Frontend");
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "MySchool Web BFF";
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Web BFF");
    options.SwaggerEndpoint("/openapi/backend/v1/swagger.json", "School API (Backend)");
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/health", (IOptions<OpenApiProxyOptions> openApi) =>
    Results.Ok(new
    {
        status = "healthy",
        service = "MySchool.WebBff",
        openApiServices = openApi.Value.Services
    }));

app.MapOpenApiProxy();
app.MapReverseProxy();

app.Run();
