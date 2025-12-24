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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson(options =>
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

// Register TenantDbContext - connection string will be set dynamically from TenantInfo
// The configuration will be evaluated when the DbContext is resolved (after middleware runs)
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
        optionsBuilder.UseSqlServer(connectionString, sql =>
        {
            sql.CommandTimeout(180);
        });
    }
    // If ConnectionString is not set, OnConfiguring will try to configure it
    // This ensures dynamic tenant resolution works correctly
});

builder.Services.AddAutoMapper(typeof(MappingConfig));

builder.Services.AddSwaggerGen(swagger =>
{
    swagger.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "ASP.NET 8 Web API", Description = "Myschool Project" });
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
        context.Response.StatusCode = 401;
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
            ValidateLifetime = false
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync("{\"error\":\"Unauthorized\"}");
            }
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("MyPolicy", p => p
        .WithOrigins("http://localhost:4200")   // the Angular dev server
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());                   // ★ allow cookies
});
builder.Services.AddScoped<StudentManagementService>();
builder.Services.AddScoped<mangeFilesService>();
builder.Services.AddScoped<StudentClassFeesRepository>();
builder.Services.AddScoped<TenantProvisioningService>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IGuardianRepository, GuardianRepository>();
builder.Services.AddScoped<ISubjectsRepository, SubjectRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IClassesRepository, ClassesRepository>();
builder.Services.AddScoped<IDivisionRepository, DivisionRepository>();
builder.Services.AddScoped<IUserRepository, UsersRepository>();
builder.Services.AddScoped<IStagesRepository, StagesRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IFeeClassRepository, FeeClassRepository>();
builder.Services.AddScoped<IFeesRepository, FeesRepository>();
builder.Services.AddScoped<IManagerRepository, ManagerRepository>();
builder.Services.AddScoped<ISchoolRepository, SchoolRepository>();
builder.Services.AddScoped<IStudentClassFeeRepository, StudentClassFeeRepository>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IYearRepository, YearRepository>();
builder.Services.AddScoped<IVoucherRepository, VoucherRepository>();
builder.Services.AddScoped<IAttachmentRepository, AttachmentsRepository>();
builder.Services.AddScoped<ICurriculumRepository, CurriculumRepository>();
builder.Services.AddScoped<ICoursePlanRepository, CoursePlanRepository>();
builder.Services.AddScoped<IGradeTypesRepository, GradeTypesRepository>();
builder.Services.AddScoped<IMonthlyGradeRepository, MonthlyGradeRepository>();
builder.Services.AddScoped<ITermlyGradeRepository, TermlyGradeRepository>();
builder.Services.AddScoped<ITermRepository, TermRepository>();
builder.Services.AddScoped<IMonthRepository, MonthRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IAccountStudentGuardianRepository, AccountStudentGuardianRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
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
