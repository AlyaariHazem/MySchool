using System.Threading;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Models;
using Backend.Repository.School.Implements;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Backend.Services;

/// <summary>
/// Idempotent demo rows: manager (if missing), teacher "Hazem" with login user, and terms الأول / الثاني.
/// </summary>
public sealed class TenantDemoDataSeeder
{
    public const string DemoTeacherFirstName = "Hazem";
    /// <summary>Arabic term names (match typical dbo.Terms seed).</summary>
    public const string TermNameFirst = "الأول";
    public const string TermNameSecond = "الثاني";

    /// <summary>Identity login name for the seeded teacher (stable across runs).</summary>
    public const string DemoTeacherUserName = "demo_teacher_hazem";

    private readonly TenantDbContext _db;
    private readonly IUserRepository _users;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public TenantDemoDataSeeder(
        TenantDbContext db,
        IUserRepository users,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _db = db;
        _users = users;
        _userManager = userManager;
        _configuration = configuration;
    }

    public sealed record SeedResult(
        bool Success,
        string Message,
        int? TeacherId,
        string? DemoTeacherUserName = null);

    public async Task<SeedResult> SeedAsync(CancellationToken cancellationToken = default)
    {
        var school = await _db.Schools.AsNoTracking().OrderBy(s => s.SchoolID).FirstOrDefaultAsync(cancellationToken);
        if (school == null)
            return new SeedResult(false, "No school found in this tenant database. Create a school first.", null, null);

        var manager = await _db.Managers.FirstOrDefaultAsync(m => m.SchoolID == school.SchoolID, cancellationToken);
        if (manager == null)
        {
            manager = new Manager
            {
                SchoolID = school.SchoolID,
                UserID = null!,
                FullName = new Name
                {
                    FirstName = "Demo",
                    MiddleName = "",
                    LastName = "Manager"
                }
            };
            _db.Managers.Add(manager);
            await _db.SaveChangesAsync(cancellationToken);
        }

        var hazem = await _db.Teachers.FirstOrDefaultAsync(
            t => t.FullName.FirstName == DemoTeacherFirstName,
            cancellationToken);
        if (hazem == null)
        {
            hazem = new Teacher
            {
                ManagerID = manager.ManagerID,
                UserID = null!,
                FullName = new Name
                {
                    FirstName = DemoTeacherFirstName,
                    MiddleName = "",
                    LastName = "-"
                }
            };
            _db.Teachers.Add(hazem);
            await _db.SaveChangesAsync(cancellationToken);
        }

        var demoUserName = await EnsureDemoTeacherUserAsync(hazem, cancellationToken);

        await EnsureTermsAsync(cancellationToken);

        return new SeedResult(
            true,
            $"Seeded teacher '{DemoTeacherFirstName}', login user '{demoUserName}', and terms '{TermNameFirst}' / '{TermNameSecond}' (idempotent). Password: configure DemoTeacher:Password (default applies if unset).",
            hazem.TeacherID,
            demoUserName);
    }

    /// <summary>Ensures <see cref="TermNameFirst"/> and <see cref="TermNameSecond"/> exist; uses identity TermIDs.</summary>
    private async Task EnsureTermsAsync(CancellationToken cancellationToken)
    {
        if (!await _db.Terms.AnyAsync(t => t.Name == TermNameFirst, cancellationToken))
        {
            _db.Terms.Add(new Term { Name = TermNameFirst });
            await _db.SaveChangesAsync(cancellationToken);
        }

        if (!await _db.Terms.AnyAsync(t => t.Name == TermNameSecond, cancellationToken))
        {
            _db.Terms.Add(new Term { Name = TermNameSecond });
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>Links <paramref name="hazem"/> to an Identity user <see cref="DemoTeacherUserName"/>; creates user if missing.</summary>
    private async Task<string> EnsureDemoTeacherUserAsync(Teacher hazem, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(hazem.UserID))
        {
            var linked = await _users.GetUserByIdAsync(hazem.UserID);
            if (linked != null)
                return linked.UserName ?? DemoTeacherUserName;
        }

        var existingByName = await _userManager.FindByNameAsync(DemoTeacherUserName);
        if (existingByName != null)
        {
            hazem.UserID = existingByName.Id;
            _db.Teachers.Update(hazem);
            await _db.SaveChangesAsync(cancellationToken);
            return existingByName.UserName ?? DemoTeacherUserName;
        }

        var password = _configuration["DemoTeacher:Password"];
        if (string.IsNullOrWhiteSpace(password))
            password = "Teacher@123!";

        var email = _configuration["DemoTeacher:Email"];
        if (string.IsNullOrWhiteSpace(email))
            email = "hazem.demo@school.local";

        var user = new ApplicationUser
        {
            UserName = DemoTeacherUserName,
            Email = email,
            UserType = "TEACHER",
            EmailConfirmed = true,
            HireDate = System.DateTime.UtcNow
        };

        var created = await _users.CreateUserAsync(user, password, "TEACHER");
        hazem.UserID = created.Id;
        _db.Teachers.Update(hazem);
        await _db.SaveChangesAsync(cancellationToken);

        return created.UserName ?? DemoTeacherUserName;
    }
}
