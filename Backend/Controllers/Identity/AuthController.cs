using Backend.DTOS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;
using Backend.Models;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
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

            // Pre-load school / tenant info so we can issue TenantId claim
            dynamic? schoolData = null;
            string? managerName = null;
            int yearID = 1;
            string? userName = null;
            string? tenantDatabaseName = null;
            int? tenantId = null;

            if (userFromDb.UserType != "ADMIN")
            {
                Tenant? tenantEntity = null;

                var managerWithTenant = await _context.Managers
                    .Include(m => m.Tenant)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.UserID == userFromDb.Id);

                if (managerWithTenant?.Tenant != null)
                {
                    tenantEntity = managerWithTenant.Tenant;
                }
                else
                {
                    var resolvedTenantId = await ResolveTenantIdForTeacherStudentGuardianAsync(userFromDb.Id, userFromDb.UserType);
                    if (resolvedTenantId.HasValue)
                    {
                        tenantEntity = await _context.Tenants.AsNoTracking()
                            .FirstOrDefaultAsync(t => t.TenantId == resolvedTenantId.Value);
                    }
                }

                if (tenantEntity != null)
                {
                    tenantId = tenantEntity.TenantId;
                    try
                    {
                        var builder = new SqlConnectionStringBuilder(tenantEntity.ConnectionString);
                        tenantDatabaseName = builder.InitialCatalog;
                    }
                    catch
                    {
                        tenantDatabaseName = tenantEntity.ConnectionString;
                    }

                    var tenantInfo = new TenantInfo
                    {
                        TenantId = tenantId,
                        ConnectionString = tenantEntity.ConnectionString
                    };

                    var tenantOptions = new DbContextOptionsBuilder<TenantDbContext>()
                        .UseSqlServer(tenantEntity.ConnectionString, sql =>
                        {
                            sql.CommandTimeout(180);
                        })
                        .Options;

                    using var tenantContext = new TenantDbContext(tenantOptions, tenantInfo);

                    var manager = await tenantContext.Managers
                        .AsNoTracking()
                        .FirstOrDefaultAsync();

                    if (manager != null)
                    {
                        var school = await tenantContext.Schools
                            .AsNoTracking()
                            .Where(s => s.SchoolID == manager.SchoolID)
                            .FirstOrDefaultAsync();

                        if (school != null)
                        {
                            var activeYearId = await tenantContext.Years
                                .AsNoTracking()
                                .Where(y => y.SchoolID == school.SchoolID && y.Active == true)
                                .Select(y => (int?)y.YearID)
                                .FirstOrDefaultAsync();

                            schoolData = new
                            {
                                SchoolName = school.SchoolName,
                                SchoolId = school.SchoolID,
                                ManagerFirstName = manager.FullName?.FirstName,
                                ManagerLastName = manager.FullName?.LastName,
                                ActiveYearId = activeYearId
                            };
                        }
                    }

                    yearID = schoolData?.ActiveYearId ?? 1;
                    managerName = (schoolData?.ManagerFirstName + " " + schoolData?.ManagerLastName)?.Trim();
                    userName = schoolData?.ManagerFirstName;

                    if (string.Equals(userFromDb.UserType, "TEACHER", StringComparison.OrdinalIgnoreCase))
                    {
                        var teacher = await tenantContext.Teachers.AsNoTracking()
                            .FirstOrDefaultAsync(t => t.UserID == userFromDb.Id);
                        if (teacher?.FullName != null)
                        {
                            var fn = teacher.FullName;
                            var parts = new[] { fn.FirstName, fn.MiddleName, fn.LastName }
                                .Where(p => !string.IsNullOrWhiteSpace(p));
                            var display = string.Join(" ", parts).Trim();
                            if (display.Length > 0)
                            {
                                userName = fn.FirstName;
                                managerName = display;
                            }
                        }
                    }
                }
            }

            // 🟩 Get actual role(s) from ASP.NET Identity
            var userRoles = await userManager.GetRolesAsync(userFromDb);

            // Ensure ADMIN users have the ADMIN role assigned
            if (userFromDb.UserType == "ADMIN" && !userRoles.Contains("ADMIN"))
            {
                await userManager.AddToRoleAsync(userFromDb, "ADMIN");
                userRoles = await userManager.GetRolesAsync(userFromDb);
            }

            List<Claim> userClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, userFromDb.Id),
                new Claim(ClaimTypes.Name, userFromDb.UserName!),
                new Claim("UserType", userFromDb.UserType)
            };

            // ⬅️ Add each role as a Role claim (important for [Authorize(Roles = \"...\")])
            foreach (var role in userRoles)
            {
                userClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Fallback: If UserType is ADMIN but no ADMIN role claim exists, add it
            // This ensures [Authorize(Roles = "ADMIN")] works even if role assignment is missing
            if (userFromDb.UserType == "ADMIN" && !userClaims.Any(c => c.Type == ClaimTypes.Role && c.Value == "ADMIN"))
            {
                userClaims.Add(new Claim(ClaimTypes.Role, "ADMIN"));
            }

            if (string.Equals(userFromDb.UserType, "TEACHER", StringComparison.OrdinalIgnoreCase)
                && !userClaims.Any(c => c.Type == ClaimTypes.Role && string.Equals(c.Value, "TEACHER", StringComparison.OrdinalIgnoreCase)))
            {
                userClaims.Add(new Claim(ClaimTypes.Role, "TEACHER"));
            }

            if (tenantId.HasValue)
            {
                userClaims.Add(new Claim("TenantId", tenantId.Value.ToString()));
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
                tenantId,
                tenantDatabase = tenantDatabaseName,
                token = accessToken,
                expiration = accessExpiry
            });
        }

        /// <summary>Issue a short-lived access token (e.g. refresh) including roles and TenantId when applicable.</summary>
        private async Task<string> BuildJwtAsync(ApplicationUser user, TimeSpan? lifetime = null)
        {
            var userRoles = await userManager.GetRolesAsync(user);
            if (user.UserType == "ADMIN" && !userRoles.Contains("ADMIN"))
            {
                await userManager.AddToRoleAsync(user, "ADMIN");
                userRoles = await userManager.GetRolesAsync(user);
            }

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim("UserType", user.UserType ?? string.Empty)
            };

            foreach (var role in userRoles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            if (user.UserType == "ADMIN" && !claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "ADMIN"))
                claims.Add(new Claim(ClaimTypes.Role, "ADMIN"));

            if (string.Equals(user.UserType, "TEACHER", StringComparison.OrdinalIgnoreCase)
                && !claims.Any(c => c.Type == ClaimTypes.Role && string.Equals(c.Value, "TEACHER", StringComparison.OrdinalIgnoreCase)))
                claims.Add(new Claim(ClaimTypes.Role, "TEACHER"));

            var tid = await ResolveTenantIdForLoginAsync(user.Id, user.UserType);
            if (tid.HasValue)
                claims.Add(new Claim("TenantId", tid.Value.ToString()));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: config["JWT:IssuerIP"],
                audience: config["JWT:AudienceIP"],
                claims: claims,
                expires: DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromMinutes(15)),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>Manager row on admin DB, else teacher/student/guardian row in a tenant DB.</summary>
        private async Task<int?> ResolveTenantIdForLoginAsync(string userId, string? userType)
        {
            if (string.IsNullOrEmpty(userType) || userType.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
                return null;

            var managerWithTenant = await _context.Managers
                .Include(m => m.Tenant)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserID == userId);

            if (managerWithTenant?.Tenant != null)
                return managerWithTenant.Tenant.TenantId;

            return await ResolveTenantIdForTeacherStudentGuardianAsync(userId, userType);
        }

        private async Task<int?> ResolveTenantIdForTeacherStudentGuardianAsync(string userId, string userType)
        {
            if (userType.Equals("MANAGER", StringComparison.OrdinalIgnoreCase))
                return null;

            var tenants = await _context.Tenants.AsNoTracking()
                .Select(t => new { t.TenantId, t.ConnectionString })
                .ToListAsync();

            foreach (var row in tenants)
            {
                if (string.IsNullOrWhiteSpace(row.ConnectionString))
                    continue;

                try
                {
                    var tenantInfo = new TenantInfo { TenantId = row.TenantId, ConnectionString = row.ConnectionString };
                    var opts = new DbContextOptionsBuilder<TenantDbContext>()
                        .UseSqlServer(row.ConnectionString, sql => sql.CommandTimeout(180))
                        .Options;

                    await using var ctx = new TenantDbContext(opts, tenantInfo);

                    var match = false;
                    if (userType.Equals("TEACHER", StringComparison.OrdinalIgnoreCase))
                        match = await ctx.Teachers.AsNoTracking().AnyAsync(t => t.UserID == userId);
                    else if (userType.Equals("STUDENT", StringComparison.OrdinalIgnoreCase))
                        match = await ctx.Students.AsNoTracking().AnyAsync(s => s.UserID == userId);
                    else if (userType.Equals("GUARDIAN", StringComparison.OrdinalIgnoreCase))
                        match = await ctx.Guardians.AsNoTracking().AnyAsync(g => g.UserID == userId);

                    if (match)
                        return row.TenantId;
                }
                catch
                {
                    // Ignore unreachable or misconfigured tenant DBs
                }
            }

            return null;
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

            var accessToken = await BuildJwtAsync(token.User, TimeSpan.FromMinutes(15));
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
