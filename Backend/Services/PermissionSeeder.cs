using Backend.Data;

namespace Backend.Services;

/// <summary>Permission seeding moved to the Identity service.</summary>
public static class PermissionSeeder
{
    public static Task SeedAsync(DatabaseContext db, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
