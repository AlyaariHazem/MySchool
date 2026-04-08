using Backend.Models;
using Backend.Models.Master;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

/// <summary>
/// Master database only: Identity, tenants, user↔tenant membership, subscriptions, refresh tokens.
/// School business entities live in <see cref="TenantDbContext"/> (per-tenant databases).
/// </summary>
public class DatabaseContext : IdentityDbContext<ApplicationUser>
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<UserTenant> UserTenants => Set<UserTenant>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<TenantSettings> TenantSettings => Set<TenantSettings>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(e => e.HasKey(t => t.TenantId));

        modelBuilder.Entity<UserTenant>(e =>
        {
            e.HasKey(x => x.UserTenantId);
            e.Property(x => x.TenantRole).HasConversion<int>();
            e.HasOne(x => x.User)
                .WithMany(u => u.UserTenants)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Tenant)
                .WithMany(t => t.UserTenants)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Subscription>(e =>
        {
            e.HasKey(x => x.SubscriptionId);
            e.HasOne(x => x.Tenant)
                .WithMany(t => t.Subscriptions)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TenantSettings>(e =>
        {
            e.HasKey(x => x.TenantSettingsId);
            e.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
            e.HasOne(x => x.Tenant)
                .WithMany(t => t.TenantSettings)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

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
                HireDate = DateTime.UtcNow,
                SecurityStamp = "f9a7b3c2d1e04a5b9c8d7e6f5a4b3c2d",
                ConcurrencyStamp = "a1b2c3d4e5f647899876543210fedcba"
            });
    }
}
