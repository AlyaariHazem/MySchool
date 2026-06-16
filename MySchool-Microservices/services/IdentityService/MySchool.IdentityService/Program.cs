using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MySchool.BuildingBlocks.Authentication;
using MySchool.Contracts.Authorization;
using MySchool.IdentityService.Authorization;
using MySchool.IdentityService.Data;
using MySchool.IdentityService.Entities;
using MySchool.IdentityService.Interfaces;
using MySchool.IdentityService.Middleware;
using MySchool.IdentityService.Services;

var builder = WebApplication.CreateBuilder(args);

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
        Description = "Authentication, users, roles, and permissions."
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
    foreach (var perm in PagePermissionNames.All)
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
    app.Logger.LogWarning(ex, "Identity startup seed skipped (database may be unavailable). Swagger and API docs still work.");
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.UseAuthentication();
app.UseMiddleware<InternalServiceApiKeyMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();
