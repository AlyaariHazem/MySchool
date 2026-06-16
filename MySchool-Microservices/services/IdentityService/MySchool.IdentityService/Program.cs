using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using MySchool.BuildingBlocks.Authentication;
using MySchool.IdentityService;
using MySchool.IdentityService.Authorization;
using MySchool.IdentityService.Data;
using MySchool.IdentityService.Entities;
using MySchool.IdentityService.GrpcServices;
using MySchool.IdentityService.Interfaces;
using MySchool.IdentityService.Middleware;
using MySchool.IdentityService.Services;

var builder = WebApplication.CreateBuilder(args);

// gRPC requires HTTP/2. On cleartext (no TLS), Kestrel only enables HTTP/2 on a dedicated Http2-only port.
builder.WebHost.ConfigureKestrel((context, serverOptions) =>
{
    var httpPort = context.Configuration.GetValue("IdentityService:HttpPort", 8082);
    var grpcPort = context.Configuration.GetValue("IdentityService:GrpcPort", 8083);

    void Listen(int port, HttpProtocols protocols)
    {
        if (context.HostingEnvironment.IsDevelopment())
            serverOptions.ListenLocalhost(port, listen => listen.Protocols = protocols);
        else
            serverOptions.ListenAnyIP(port, listen => listen.Protocols = protocols);
    }

    Listen(httpPort, HttpProtocols.Http1);
    Listen(grpcPort, HttpProtocols.Http2);
});

builder.Services.AddGrpc();
builder.Services.AddIdentityFeatures();

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "MySchool Identity Service",
        Version = "v1",
        Description = "Authentication, users, roles, and permissions (internal HTTP + gRPC)."
    });
    options.DocumentFilter<MySchool.IdentityService.SwaggerServerDocumentFilter>();
});

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlIdentityConnection")));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = false;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 4;
    })
    .AddEntityFrameworkStores<IdentityDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddMySchoolJwtAuthentication(builder.Configuration);

builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorization(options =>
{
    foreach (var perm in MySchool.Contracts.Authorization.PagePermissionNames.All)
    {
        options.AddPolicy(perm, policy =>
            policy.Requirements.Add(new PermissionRequirement(perm)));
    }
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:4200"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddHttpClient<IMonolithIntegrationClient, MonolithIntegrationClient>();
builder.Services.AddScoped<IPermissionClaimService, PermissionClaimService>();
builder.Services.AddScoped<RolePermissionAdminService>();

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();
    await PermissionSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<IdentityDbContext>());

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in new[] { "ADMIN", "GUARDIAN", "STUDENT", "TEACHER", "MANAGER" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Identity startup seed skipped (database may be unavailable).");
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.UseAuthentication();
app.UseMiddleware<InternalServiceApiKeyMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<IdentityGrpcService>();

app.Run();
