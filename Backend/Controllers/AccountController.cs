using Backend.DTOS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Models;
using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration config;
        private readonly DatabaseContext _context;

        public AccountController(UserManager<ApplicationUser> UserManager, IConfiguration config, DatabaseContext context)
        {
            userManager = UserManager;
            this.config = config;
            _context = context;
        }

        [HttpPost("Register")] // POST api/Account/Register
        public async Task<IActionResult> Register(RegisterDto UserFromRequest)
        {
            if (ModelState.IsValid)
            {
                // Save to DB
                ApplicationUser user = new ApplicationUser
                {
                    UserName = UserFromRequest.UserName,
                    Email = UserFromRequest.Email,
                    UserType = UserFromRequest.UserType // Assign UserType from request
                };

                IdentityResult result =
                    await userManager.CreateAsync(user, UserFromRequest.Password);

                if (result.Succeeded)
                {
                    // Optionally assign to role based on UserType
                    if (!string.IsNullOrEmpty(UserFromRequest.UserType))
                    {
                        await userManager.AddToRoleAsync(user, UserFromRequest.UserType);
                    }

                    return Ok(new { message = "User created successfully." });
                }

                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("Password", item.Description);
                }
                return BadRequest(new { message = "Registration failed.", errors = result.Errors });
            }
            return BadRequest(new { message = "Invalid request data." });
        }

        [HttpPost("Login")] // POST api/Account/Login
        public async Task<IActionResult> Login(LoginDto userFromRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the user exists in the database
            ApplicationUser userFromDb = await userManager.FindByNameAsync(userFromRequest.UserName);
            if (userFromDb == null)
            {
                return BadRequest(new { message = "Invalid username or password" });
            }

            // Check if the password is correct
            bool isPasswordValid = await userManager.CheckPasswordAsync(userFromDb, userFromRequest.Password);
            if (!isPasswordValid)
            {
                return BadRequest(new { message = "Invalid username or password" });
            }

            // Validate userType from `ApplicationUser`
            if (!string.Equals(userFromDb.UserType, userFromRequest.userType, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Unauthorized: User type does not match." });
            }

            // Generate JWT token
            List<Claim> userClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, userFromDb.Id),
                new Claim(ClaimTypes.Name, userFromDb.UserName),
                new Claim("UserType", userFromDb.UserType) // Adding userType as a claim
            };

            var signInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:SecritKey"]));
            var signingCred = new SigningCredentials(signInKey, SecurityAlgorithms.HmacSha256);

            // Design token
            JwtSecurityToken token = new JwtSecurityToken(
                audience: config["JWT:AudienceIP"],
                issuer: config["JWT:IssuerIP"],
                expires: DateTime.Now.AddMonths(1),
                claims: userClaims,
                signingCredentials: signingCred
            );

            // Return the generated token
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });
        }

    }
}
