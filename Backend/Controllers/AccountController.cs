using Backend.DTOS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Models;

namespace WebAPIDotNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration config;

        public AccountController(UserManager<ApplicationUser> UserManager, IConfiguration config)
        {
            userManager = UserManager;
            this.config = config;
        }


        [HttpPost("Register")]//Post api/Account/Register
        public async Task<IActionResult> Register(RegisterDto UserFromRequest)
        {
            if (ModelState.IsValid)
            {
                //save DB
                ApplicationUser user = new ApplicationUser();
                user.UserName = UserFromRequest.UserName;
                user.Email = UserFromRequest.Email;
                IdentityResult result =
                    await userManager.CreateAsync(user, UserFromRequest.Password);
                if (result.Succeeded)
                {
                    return Ok("Create");
                }
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("PAssword", item.Description);
                }
            }
            return BadRequest(ModelState);
        }


        [HttpPost("Login")]//Post api/Account/login
        public async Task<IActionResult> Login(LoginDto userFRomRequest)
        {
            if (ModelState.IsValid)
            {
                //check
                ApplicationUser userFromDb =
                    await userManager.FindByNameAsync(userFRomRequest.UserName);
                if (userFromDb != null)
                {
                    bool found =
                        await userManager.CheckPasswordAsync(userFromDb, userFRomRequest.Password);
                    if (found == true)
                    {
                        //generate token<==

                        List<Claim> UserClaims = new List<Claim>();

                        //Token Genrated id change (JWT Predefind Claims )
                        UserClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                        UserClaims.Add(new Claim(ClaimTypes.NameIdentifier, userFromDb.Id));
                        UserClaims.Add(new Claim(ClaimTypes.Name, userFromDb.UserName));

                        var UserRoles = await userManager.GetRolesAsync(userFromDb);

                        foreach (var roleNAme in UserRoles)
                        {
                            UserClaims.Add(new Claim(ClaimTypes.Role, roleNAme));
                        }

                        var SignInKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                                config["JWT:SecritKey"]));

                        SigningCredentials signingCred =
                            new SigningCredentials
                            (SignInKey, SecurityAlgorithms.HmacSha256);

                        //design token
                        JwtSecurityToken mytoken = new JwtSecurityToken(
                            audience: config["JWT:AudienceIP"], //localhost for SPA(Angualr)
                            issuer: config["JWT:IssuerIP"],
                            expires: DateTime.Now.AddMonths(1),
                            claims: UserClaims,
                            signingCredentials: signingCred

                            );
                        //generate token response

                        return Ok(new
                        {
                            //it can be DTO
                            token = new JwtSecurityTokenHandler().WriteToken(mytoken),
                            expiration = DateTime.Now.AddMonths(1)//mytoken.ValidTo
                            
                        });
                    }
                }
                ModelState.AddModelError("Username", "Username OR Password  Invalid");
            }
            return BadRequest(ModelState);

        }
    }
}
