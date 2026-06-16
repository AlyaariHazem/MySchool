using System.Security.Claims;
using MySchool.IdentityService.Entities;

namespace MySchool.IdentityService.Services;

public interface IUserClaimsBuilder
{
    Task<List<Claim>> BuildBaseClaimsAsync(
        ApplicationUser user,
        IList<string> userRoles,
        int? tenantId,
        CancellationToken cancellationToken = default);
}
