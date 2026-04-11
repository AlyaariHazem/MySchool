using System.Text;
using Backend;
using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Backend.Repository;
using Backend.Repository.School;
using Backend.Repository.School.Classes;
using Backend.Repository.School.Implements;
using Backend.Repository.School.Interfaces;
using Backend.Services;
using FirstProjectWithMVC.Repository.School;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Backend.Middleware;
using Backend.Extensions;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Authorization;
using Backend.Configuration;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.Configure(options =>
{
    options.ActivityTrackingOptions = ActivityTrackingOptions.TraceId | ActivityTrackingOptions.SpanId;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 2L * 1024 * 1024 * 1024;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 2L * 1024 * 1024 * 1024;
});

builder.Services.AddControllers(options =>
{
    // Require authentication by default for all API controllers.
    // Use [AllowAnonymous] on specific controllers/actions that must be public.
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
})
.AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("SqlAdminConnection"),
        sql => sql.CommandTimeout(180)
    )
);

builder.Services.AddScoped<TenantInfo>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<SqlRestoreOptions>(builder.Configuration.GetSection(SqlRestoreOptions.SectionName));
builder.Services.AddScoped<SqlRestoreService>();

// Register TenantDbContext - connection string will be set dynamically from TenantInfo
// The configuration will be evaluated when the DbContext is resolved (after middleware runs)
// IMPORTANT: Never run DatabaseContext.Migrate() against a tenant database connection string.
// Only TenantDbContext (and Backend/Migrations/Tenant) belongs in tenant DBs.
builder.Services.AddDbContext<TenantDbContext>((serviceProvider, optionsBuilder) =>
{
    // Get TenantInfo from service provider (scoped, resolved per request)
    // The middleware sets ConnectionString before the controller/DbContext is resolved
    var tenantInfo = serviceProvider.GetRequiredService<TenantInfo>();
    
    // Get connection string - either from TenantInfo (set by middleware) or fallback to admin connection for ADMIN users
    string? connectionString = tenantInfo.ConnectionString;
    
    // If connection string is available (set by middleware), use it
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        optionsBuilder.UseTenantSqlServer(connectionString);
    }
    // If ConnectionString is not set, OnConfiguring will try to configure it
    // This ensures dynamic tenant resolution works correctly
});

builder.Services.AddAutoMapper(cfg => cfg.LicenseKey = "", typeof(MappingConfig));

builder.Services.AddSwaggerGen(swagger =>
{
    swagger.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "ASP.NET 8 Web API",
        Description =
            "Myschool tenant API. Teacher workflow is covered by: Attendance (daily rolls, bulk), " +
            "WeeklySchedule (class/term grid and CRUD), MonthlyGrades and TermlyGrade (fast grade entry), " +
            "Exams (sessions, scheduled exams, teacher mark entry, student/guardian views, reports), " +
            "Homework (tasks per course plan, submissions, grading, guardian read-only with published feedback, manager activity), " +
            "Report (monthly reports and templates), Notifications (inbox and send), and TeacherWorkspace " +
            "(class/student/subject summary plus recent course plans)."
    });
    swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your token."
    });
    swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Force Swagger UI to call the same host you opened it from (prevents localhost:5000 issues).
    swagger.DocumentFilter<SwaggerServerDocumentFilter>();
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<DatabaseContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

var key = Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"]!);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;   // dev only

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JWT:IssuerIP"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JWT:AudienceIP"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateLifetime = false,
            // Ensures JWT "role" / ClaimTypes.Role map correctly so IsInRole(...) works after bearer auth.
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(
                    JsonSerializer.Serialize(new
                    {
                        error = "Forbidden",
                        message = "You are not authenticated or your token is invalid."
                    })
                );
            }
            ,
            OnForbidden = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(
                    JsonSerializer.Serialize(new
                    {
                        error = "Forbidden",
                        message = "You do not have permission to access this resource."
                    })
                );
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DatabaseRestore", policy =>
    {
        policy.RequireAssertion(context =>
        {
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            if (context.User.IsInRole("ADMIN") || context.User.IsInRole("MANAGER"))
            {
                return true;
            }

            var userType = context.User.FindFirst("UserType")?.Value;
            return string.Equals(userType, "ADMIN", StringComparison.OrdinalIgnoreCase)
                || string.Equals(userType, "MANAGER", StringComparison.OrdinalIgnoreCase);
        });
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("MyPolicy", p => p
        .WithOrigins("http://localhost:4200")   // the Angular dev server
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());                   // ★ allow cookies
});

// Register Application Services and Repositories
builder.Services.AddApplicationServices();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    // Ensure the admin database exists and all migrations are applied before seeding roles.
    var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    db.Database.Migrate();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "ADMIN","GUARDIAN", "STUDENT", "TEACHER", "MANAGER" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

// Enable Swagger in all environments (or change to IsDevelopment() if you only want it in dev)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ASP.NET 8 Web API V1");
    c.RoutePrefix = "swagger";
});

// Serves wwwroot (e.g. wwwroot/uploads/School/...) at /uploads/... — school logos must live under uploads, not site root.
app.UseStaticFiles();
app.UseCors("MyPolicy");
app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>(); // Resolve tenant from JWT before authorization
app.UseAuthorization();

// Add a default route to redirect to Swagger
app.MapGet("/", async (HttpContext context) =>
{
    context.Response.Redirect("/swagger");
    await Task.CompletedTask;
});

app.MapControllers();
app.Run();
