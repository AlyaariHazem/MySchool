using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySchool.WebBff.Abstracts;
using MySchool.WebBff.Common.DTOs;
using MySchool.WebBff.Common.DTOs.Identity;
using MySchool.WebBff.GrpcServices;

namespace MySchool.WebBff.Controllers.Identity;

[Route("bff/roles")]
[Authorize(Roles = "ADMIN,MANAGER")]
public sealed class RolesController : RestfulBffControllerBase<RoleListItemDto, EmptyRequestDto, EmptyRequestDto>
{
    private readonly IIdentityGrpcGateway _identity;

    public RolesController(IIdentityGrpcGateway identity)
    {
        _identity = identity;
    }

    [HttpGet]
    public override async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        await base.GetAll(cancellationToken);

    protected override async Task<IEnumerable<RoleListItemDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await _identity.GetRolesAsync(cancellationToken);
        return RoleListItemMapper.FromGrpc(result);
    }
}
