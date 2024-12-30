using System.Text;
using Backend;
using Backend.Data;
using Backend.Models;
using Backend.Repository;
using Backend.Repository.School;

using Backend.Repository.School.Interfaces;

using Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Backend.Repository.IRepository;
using Backend.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.CommandTimeout(180) // Set timeout to 180 seconds (3 minutes)
    )
);
builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddSwaggerDocumentation();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().
AddEntityFrameworkStores<DatabaseContext>();
builder.Services.AddRepositoriesLayer();
builder.Services.AddServicesLayer();
builder.Services.AddCustomAuthentication(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyPolicy", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;

    // Customize error messages
    options.Password.RequireDigit = false;
    options.Password.RequiredUniqueChars = 1;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
});

var app = builder.Build();
// Seed roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "GUARDIAN", "STUDENT", "TEACHER", "MANAGER" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ASP.NET 8 Web API V1");
        c.RoutePrefix = "swagger"; // Access Swagger UI at https://localhost:7258/swagger
    });
}

app.UseStaticFiles();

app.UseCors("MyPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
