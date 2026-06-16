using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySchool.WebBff.Abstracts;
using MySchool.WebBff.Common.DTOs;
using MySchool.WebBff.Common.DTOs.Identity;
using MySchool.WebBff.GrpcServices;

namespace MySchool.WebBff.Controllers.Identity;

[Route("bff/users")]
[Authorize]
public sealed class UsersController : RestfulBffControllerBase<UserListItemDto, EmptyRequestDto, EmptyRequestDto>
{
    private readonly IIdentityGrpcGateway _identity;

    public UsersController(IIdentityGrpcGateway identity)
    {
        _identity = identity;
    }

    [HttpGet]
    public override async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        await base.GetAll(cancellationToken);

    protected override async Task<IEnumerable<UserListItemDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await _identity.GetUsersAsync(cancellationToken);
        return UserListItemMapper.FromGrpc(result);
    }
}
