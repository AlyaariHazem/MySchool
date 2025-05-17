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
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration config;
        private readonly DatabaseContext _context;

        public AuthController(UserManager<ApplicationUser> UserManager, IConfiguration config, DatabaseContext context)
        {
            userManager = UserManager;
            this.config = config;
            _context = context;
        }

        [HttpPost("Register")] // POST api/auth/Register
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

        // ✅ في Login method:
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto userFromRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            ApplicationUser userFromDb = await userManager.FindByNameAsync(userFromRequest.UserName);
            if (userFromDb == null)
                return BadRequest(new { message = "Invalid username or password" });

            bool isPasswordValid = await userManager.CheckPasswordAsync(userFromDb, userFromRequest.Password);
            if (!isPasswordValid)
                return BadRequest(new { message = "Invalid username or password" });

            if (!string.Equals(userFromDb.UserType, userFromRequest.userType, StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Unauthorized: User type does not match." });

            // 🟩 Get actual role(s) from ASP.NET Identity
            var userRoles = await userManager.GetRolesAsync(userFromDb);

            List<Claim> userClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, userFromDb.Id),
                new Claim(ClaimTypes.Name, userFromDb.UserName!),
                new Claim("UserType", userFromDb.UserType)
            };

            // ⬅️ Add each role as a Role claim (important for [Authorize(Roles = "...")])
            foreach (var role in userRoles)
            {
                userClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var signInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:SecretKey"]!));
            var signingCred = new SigningCredentials(signInKey, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                audience: config["JWT:AudienceIP"],
                issuer: config["JWT:IssuerIP"],
                expires: DateTime.Now.AddMonths(1),
                claims: userClaims,
                signingCredentials: signingCred
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            dynamic? schoolData = null;
            string? managerName = null;
            int yearID = 1;
            string? userName = null;

            if (userFromDb.UserType != "ADMIN")
            {
                schoolData = await _context.Schools
                    .AsNoTracking()
                    .Include(s => s.Manager)
                    .Include(s => s.Years)
                    .Where(s => s.Manager.UserID == userFromDb.Id)
                    .Select(s => new
                    {
                        SchoolName = s.SchoolName,
                        ManagerName = s.Manager.FullName,
                        SchoolId = s.SchoolID,
                        yearID = s.Years.FirstOrDefault(y => y.Active)!.YearID
                    })
                    .FirstOrDefaultAsync();

                int? schoolId = schoolData?.SchoolId;
                yearID = schoolData?.yearID;

                managerName = $"{schoolData?.ManagerName.FirstName} {schoolData?.ManagerName.LastName}";
                userName = schoolData?.ManagerName.FirstName;
            }

            var accessToken = tokenString;
            var accessExpiry = token.ValidTo;

            var rawRefresh = CreateRandomToken();
            var hash = Hash(rawRefresh);
            var refresh = new RefreshToken
            {
                TokenHash = hash,
                Expires = DateTime.UtcNow.AddDays(7),
                UserId = userFromDb.Id
            };
            _context.RefreshTokens.Add(refresh);
            await _context.SaveChangesAsync();

            Response.Cookies.Append("refreshToken", rawRefresh, BuildCookieOptions(refresh.Expires));

            if (userFromDb.UserType == "ADMIN")
                return Ok(new
                {
                    userName = userFromDb.UserName,
                    token = accessToken,
                    expiration = accessExpiry
                });

            return Ok(new
            {
                schoolName = schoolData?.SchoolName,
                managerName,
                userName,
                schoolId = schoolData?.SchoolId,
                yearId = yearID,
                token = accessToken,
                expiration = accessExpiry
            });
        }

        private string BuildJwt(ApplicationUser user, TimeSpan? lifetime = null)
        {
            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim("UserType", user.UserType ?? string.Empty)
        };

            var key = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(config["JWT:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["JWT:IssuerIP"],
                audience: config["JWT:AudienceIP"],
                claims: claims,
                expires: DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromMinutes(15)),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private static string CreateRandomToken(int bytes = 64)
    => Convert.ToBase64String(RandomNumberGenerator.GetBytes(bytes));

        private static string Hash(string rawToken)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToHexString(bytes);          // easy to store / compare
        }

        private CookieOptions BuildCookieOptions(DateTime expires) => new()
        {
            HttpOnly = true,
            Secure = true,        // ➜ HTTPS only in prod
            SameSite = SameSiteMode.None,
            Expires = expires
        };
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var rawToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrWhiteSpace(rawToken)) return Unauthorized();

            var hash = Hash(rawToken);
            var token = await _context.RefreshTokens
                        .Include(t => t.User)
                        .FirstOrDefaultAsync(t => t.TokenHash == hash && t.Revoked == null);

            if (token is null || token.Expires < DateTime.UtcNow) return Unauthorized();

            /* Rotate (single-use) */
            token.Revoked = DateTime.UtcNow;

            var newRaw = CreateRandomToken();
            var newHash = Hash(newRaw);
            var newToken = new RefreshToken
            {
                TokenHash = newHash,
                Expires = DateTime.UtcNow.AddDays(7),
                UserId = token.UserId
            };
            _context.RefreshTokens.Add(newToken);
            await _context.SaveChangesAsync();

            Response.Cookies.Append("refreshToken", newRaw, BuildCookieOptions(newToken.Expires));

            var accessToken = BuildJwt(token.User);
            var accessExpiry = DateTime.UtcNow.AddMinutes(15);
            return Ok(new { token = accessToken, expiration = accessExpiry });
        }
        // ───────── NEW ENDPOINT ─────────
        
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // 2️⃣ never trust parameters – get the user ID from the JWT claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // 3️⃣ delete *all* refresh tokens for this user
            var tokens = await _context.RefreshTokens
                                       .Where(t => t.UserId == userId)
                                       .ToListAsync();

            if (tokens.Count > 0)
            {
                _context.RefreshTokens.RemoveRange(tokens);   // hard-delete
                await _context.SaveChangesAsync();
            }

            // 4️⃣ expire the cookie on the client
            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UnixEpoch
            });

            return Ok(new { message = "Logged out on all devices" });
        }

    }
}
