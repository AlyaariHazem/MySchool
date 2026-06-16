using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MySchool.IdentityService.Entities;

namespace MySchool.IdentityService.Data;

/// <summary>
/// Master database identity slice: users, roles, refresh tokens, permissions, user↔tenant links.
/// </summary>
public class IdentityDbContext : IdentityDbContext<ApplicationUser>
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasIndex(t => t.TokenHash).IsUnique();
            e.Property(t => t.TokenHash).HasMaxLength(88);
            e.HasOne(t => t.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.PhoneNumberNormalized).HasMaxLength(32);
            e.HasIndex(u => u.PhoneNumberNormalized)
                .IsUnique()
                .HasFilter("[PhoneNumberNormalized] IS NOT NULL");
        });

        modelBuilder.Entity<Permission>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<RolePermission>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.RoleName, x.PermissionId }).IsUnique();
            e.HasOne(x => x.Permission)
                .WithMany()
                .HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = "1", Name = "ADMIN", NormalizedName = "ADMIN" },
            new IdentityRole { Id = "2", Name = "MANAGER", NormalizedName = "MANAGER" },
            new IdentityRole { Id = "3", Name = "STUDENT", NormalizedName = "STUDENT" },
            new IdentityRole { Id = "4", Name = "TEACHER", NormalizedName = "TEACHER" },
            new IdentityRole { Id = "5", Name = "GUARDIAN", NormalizedName = "GUARDIAN" });

        var passwordHasher = new PasswordHasher<ApplicationUser>();
        var hashedPassword = passwordHasher.HashPassword(null!, "ADMIN");
        modelBuilder.Entity<ApplicationUser>().HasData(
            new ApplicationUser
            {
                Id = "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                UserName = "ADMIN",
                NormalizedUserName = "ADMIN",
                Email = "ADMIN@GMAIL.COM",
                NormalizedEmail = "ADMIN@GMAIL.COM",
                PasswordHash = hashedPassword,
                EmailConfirmed = true,
                UserType = "ADMIN",
                HireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                SecurityStamp = "f9a7b3c2d1e04a5b9c8d7e6f5a4b3c2d",
                ConcurrencyStamp = "a1b2c3d4e5f647899876543210fedcba"
            });
    }
}
