using Backend.Models;
using Backend.Models.Master;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

/// <summary>
/// Master database: tenants, user↔tenant membership, subscriptions, registration requests.
/// School business entities live in <see cref="TenantDbContext"/> (per-tenant databases).
/// Identity users are owned by the Identity service.
/// </summary>
public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<UserTenant> UserTenants => Set<UserTenant>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<TenantSettings> TenantSettings => Set<TenantSettings>();
    public DbSet<RegistrationRequest> RegistrationRequests => Set<RegistrationRequest>();
    public DbSet<RegistrationRequestAttachment> RegistrationRequestAttachments => Set<RegistrationRequestAttachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(e => e.HasKey(t => t.TenantId));

        modelBuilder.Entity<UserTenant>(e =>
        {
            e.HasKey(x => x.UserTenantId);
            e.Property(x => x.TenantRole).HasConversion<int>();
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

        modelBuilder.Entity<RegistrationRequest>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.UserName).HasMaxLength(256);
            e.Property(x => x.NormalizedUserName).HasMaxLength(256);
            e.Property(x => x.PhoneNumber).HasMaxLength(32);
            e.Property(x => x.NormalizedPhone).HasMaxLength(32);
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.NormalizedEmail).HasMaxLength(256);
            e.Property(x => x.Gender).HasMaxLength(32);
            e.Property(x => x.RequestedRole).HasMaxLength(32);
            e.Property(x => x.RejectionReason).HasMaxLength(2000);
            e.Property(x => x.Status).HasConversion<int>();
            e.HasIndex(x => x.NormalizedUserName)
                .IsUnique()
                .HasFilter("[Status] = 0");
            e.HasIndex(x => x.NormalizedPhone)
                .IsUnique()
                .HasFilter("[Status] = 0");
            e.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RegistrationRequestAttachment>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.OriginalFileName).HasMaxLength(512);
            e.Property(x => x.RelativePath).HasMaxLength(1024);
            e.Property(x => x.ContentType).HasMaxLength(256);
            e.HasIndex(x => x.RegistrationRequestId);
            e.HasOne(x => x.RegistrationRequest)
                .WithMany(r => r.Attachments)
                .HasForeignKey(x => x.RegistrationRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
