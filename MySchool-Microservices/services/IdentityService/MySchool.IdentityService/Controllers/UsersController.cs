using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySchool.Contracts.Users;
using MySchool.IdentityService.Entities;
using MySchool.IdentityService.Mapping;

namespace MySchool.IdentityService.Controllers;

[Route("api/users")]
[ApiController]
public sealed class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<ActionResult<UserAccountDto>> CreateUser([FromBody] CreateUserApiRequest request)
    {
        var user = UserAccountMapper.ToEntity(request.User);
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { message = "Could not create user.", errors = result.Errors.Select(e => e.Description) });

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return BadRequest(new { message = "Could not assign role.", errors = roleResult.Errors.Select(e => e.Description) });
            }
        }

        return Ok(UserAccountMapper.ToDto(user));
    }

    [HttpPost("raw")]
    public async Task<IActionResult> AddRaw([FromBody] UserAccountDto dto)
    {
        var user = UserAccountMapper.ToEntity(dto);
        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
            return BadRequest(new { message = "Could not add user.", errors = result.Errors.Select(e => e.Description) });
        return Ok(UserAccountMapper.ToDto(user));
    }

    [HttpPost("approved-registration")]
    public async Task<ActionResult<UserAccountDto>> CreateApprovedRegistrationUser(
        [FromBody] ApprovedRegistrationUserRequest request)
    {
        var user = UserAccountMapper.ToEntity(request.User);
        var createResult = await _userManager.CreateAsync(user);
        if (!createResult.Succeeded)
            return BadRequest(new { message = "Could not create user.", errors = createResult.Errors.Select(e => e.Description) });

        user.PasswordHash = request.PasswordHash;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return BadRequest(new { message = "Could not set password.", errors = updateResult.Errors.Select(e => e.Description) });
        }

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return BadRequest(new { message = "Could not assign role.", errors = roleResult.Errors.Select(e => e.Description) });
            }
        }

        return Ok(UserAccountMapper.ToDto(user));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserAccountDto>>> GetAll()
    {
        var users = await _userManager.Users.AsNoTracking().ToListAsync();
        return Ok(users.Select(UserAccountMapper.ToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserAccountDto>> GetById(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();
        return Ok(UserAccountMapper.ToDto(user));
    }

    [HttpGet("by-id-or-name/{idOrName}")]
    public async Task<ActionResult<UserAccountDto>> GetByIdOrName(string idOrName)
    {
        var user = await _userManager.FindByIdAsync(idOrName)
            ?? await _userManager.FindByNameAsync(idOrName);
        if (user == null)
            return NotFound();
        return Ok(UserAccountMapper.ToDto(user));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UserAccountDto dto)
    {
        if (!string.Equals(id, dto.Id, StringComparison.Ordinal))
            return BadRequest(new { message = "Route id does not match body id." });

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        UserAccountMapper.ApplyToEntity(dto, user);
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(new { message = "Could not update user.", errors = result.Errors.Select(e => e.Description) });
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(new { message = "Could not delete user.", errors = result.Errors.Select(e => e.Description) });
        return NoContent();
    }

    [HttpGet("exists")]
    public async Task<ActionResult<ExistsResponse>> Exists(
        [FromQuery] string normalizedUserName,
        [FromQuery] string normalizedEmail)
    {
        var exists = await _userManager.Users.AnyAsync(u =>
            u.NormalizedUserName == normalizedUserName || u.NormalizedEmail == normalizedEmail);
        return Ok(new ExistsResponse { Exists = exists });
    }

    [HttpGet("exists-phone")]
    public async Task<ActionResult<ExistsResponse>> ExistsPhone([FromQuery] string normalizedPhone)
    {
        var exists = await _userManager.Users.AnyAsync(u => u.PhoneNumberNormalized == normalizedPhone);
        return Ok(new ExistsResponse { Exists = exists });
    }

    public sealed class CreateUserApiRequest
    {
        public UserAccountDto User { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string Role { get; set; } = default!;
    }

    public sealed class ApprovedRegistrationUserRequest
    {
        public UserAccountDto User { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public string Role { get; set; } = default!;
    }

    public sealed class ExistsResponse
    {
        public bool Exists { get; set; }
    }
}
