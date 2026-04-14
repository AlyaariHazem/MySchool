namespace Backend.Interfaces;

/// <summary>Resolves <see cref="Common.SchoolUserRoleKeys"/> for a user in a tenant DB.</summary>
public interface ISchoolRoleResolver
{
    /// <returns>School role key (e.g. Teacher, Manager, EducationalSupervisor) or null if not found.</returns>
    Task<string?> ResolveSchoolRoleKeyAsync(string userId, int tenantId, CancellationToken cancellationToken = default);
}
