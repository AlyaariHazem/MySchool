using Backend.Common;
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
using Backend.Models.Master;
using Backend.Data;
using Backend.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration config;
        private readonly DatabaseContext _context;
        private readonly ITenantMembershipService _tenantMembership;

        public AuthController(
            UserManager<ApplicationUser> UserManager,
            IConfiguration config,
            DatabaseContext context,
            ITenantMembershipService tenantMembership)
        {
            userManager = UserManager;
            this.config = config;
            _context = context;
            _tenantMembership = tenantMembership;
        }

        [HttpPost("Register")]
        [AllowAnonymous]
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
        [AllowAnonymous]
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
            TenantRole? membershipTenantRole = null;
            IReadOnlyList<UserTenantSummaryDto>? tenantChoices = null;

            if (userFromDb.UserType != "ADMIN")
            {
                tenantChoices = await _tenantMembership.GetTenantSummariesAsync(userFromDb.Id);

                if (tenantChoices.Count > 0)
                {
                    if (tenantChoices.Count == 1)
                    {
                        tenantId = tenantChoices[0].TenantId;
                        membershipTenantRole = tenantChoices[0].TenantRole;
                    }
                    else if (userFromRequest.TenantId is { } requestedTid && tenantChoices.Any(t => t.TenantId == requestedTid))
                    {
                        var picked = tenantChoices.First(t => t.TenantId == requestedTid);
                        tenantId = picked.TenantId;
                        membershipTenantRole = picked.TenantRole;
                    }
                    // else: multiple schools and no TenantId in login → tenantId stays null (client shows selector)
                }

                Tenant? tenantEntity = null;
                if (tenantId.HasValue)
                {
                    tenantEntity = await _context.Tenants.AsNoTracking()
                        .FirstOrDefaultAsync(t => t.TenantId == tenantId.Value);
                }

                // Stale membership: UserTenant pointed at a tenant row removed from master
                if (tenantEntity == null && tenantId.HasValue)
                {
                    tenantId = null;
                    membershipTenantRole = null;
                }

                // Legacy: no usable UserTenant yet — scan tenant DBs (Managers, Teachers, …).
                // When Count > 1 we require an explicit TenantId on login / select-tenant instead.
                if (tenantEntity == null && !tenantId.HasValue && tenantChoices.Count <= 1)
                {
                    var resolvedTenantId = await ResolveTenantIdForTeacherStudentGuardianAsync(userFromDb.Id, userFromDb.UserType);
                    if (resolvedTenantId.HasValue)
                    {
                        tenantEntity = await _context.Tenants.AsNoTracking()
                            .FirstOrDefaultAsync(t => t.TenantId == resolvedTenantId.Value);
                    }

                    if (tenantEntity != null)
                    {
                        tenantId = tenantEntity.TenantId;
                        var roleFromType = TenantRoleFromUserType(userFromDb.UserType);
                        if (roleFromType.HasValue)
                        {
                            membershipTenantRole = roleFromType;
                            await EnsureUserTenantIfMissingAsync(userFromDb.Id, tenantEntity.TenantId, roleFromType.Value);
                        }
                    }
                }

                if (tenantEntity != null)
                {
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
                        .UseTenantSqlServer(tenantEntity.ConnectionString)
                        .Options;

                    using var tenantContext = new TenantDbContext(tenantOptions, tenantInfo);

                    if (string.Equals(userFromDb.UserType, "MANAGER", StringComparison.OrdinalIgnoreCase))
                    {
                        var manager = await tenantContext.Managers
                            .AsNoTracking()
                            .FirstOrDefaultAsync(m => m.UserID == userFromDb.Id);

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

            if (string.Equals(userFromDb.UserType, "ADMIN", StringComparison.OrdinalIgnoreCase))
                userClaims.Add(new Claim(PlatformAdminHelper.TenantBypassClaimType, PlatformAdminHelper.TenantBypassClaimValue));

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

            if (membershipTenantRole.HasValue)
            {
                userClaims.Add(new Claim("TenantRole", ((int)membershipTenantRole.Value).ToString()));
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
                tenants = tenantChoices != null && tenantChoices.Count > 1 && !tenantId.HasValue ? tenantChoices : null,
                token = accessToken,
                expiration = accessExpiry
            });
        }

        /// <summary>Issue a short-lived access token (e.g. refresh) including roles and TenantId when applicable.</summary>
        private async Task<string> BuildJwtAsync(ApplicationUser user, TimeSpan? lifetime = null, int? forcedTenantId = null)
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

            if (string.Equals(user.UserType, "ADMIN", StringComparison.OrdinalIgnoreCase))
                claims.Add(new Claim(PlatformAdminHelper.TenantBypassClaimType, PlatformAdminHelper.TenantBypassClaimValue));

            foreach (var role in userRoles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            if (user.UserType == "ADMIN" && !claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "ADMIN"))
                claims.Add(new Claim(ClaimTypes.Role, "ADMIN"));

            if (string.Equals(user.UserType, "TEACHER", StringComparison.OrdinalIgnoreCase)
                && !claims.Any(c => c.Type == ClaimTypes.Role && string.Equals(c.Value, "TEACHER", StringComparison.OrdinalIgnoreCase)))
                claims.Add(new Claim(ClaimTypes.Role, "TEACHER"));

            var tid = forcedTenantId ?? await ResolveTenantIdForLoginAsync(user.Id, user.UserType);
            if (tid.HasValue)
            {
                claims.Add(new Claim("TenantId", tid.Value.ToString()));
                var mem = await _tenantMembership.GetMembershipAsync(user.Id, tid.Value);
                if (mem != null)
                    claims.Add(new Claim("TenantRole", ((int)mem.TenantRole).ToString()));
            }

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

            var fromMembership = await _tenantMembership.ResolveTenantIdForIssuedTokenAsync(userId);
            if (fromMembership.HasValue)
                return fromMembership.Value;

            return await ResolveTenantIdForTeacherStudentGuardianAsync(userId, userType);
        }

        /// <summary>
        /// Legacy: find which tenant DB holds this user when there is no <see cref="UserTenant"/> row yet.
        /// Managers were previously skipped here, so school logins could issue a token without TenantId and hit TenantRequired.
        /// </summary>
        private async Task<int?> ResolveTenantIdForTeacherStudentGuardianAsync(string userId, string userType)
        {
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
                        .UseTenantSqlServer(row.ConnectionString)
                        .Options;

                    await using var ctx = new TenantDbContext(opts, tenantInfo);

                    var match = false;
                    if (userType.Equals("MANAGER", StringComparison.OrdinalIgnoreCase))
                        match = await ctx.Managers.AsNoTracking().AnyAsync(m => m.UserID == userId);
                    else if (userType.Equals("TEACHER", StringComparison.OrdinalIgnoreCase))
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

        private static TenantRole? TenantRoleFromUserType(string userType)
        {
            if (string.Equals(userType, "MANAGER", StringComparison.OrdinalIgnoreCase))
                return TenantRole.SchoolAdmin;
            if (string.Equals(userType, "TEACHER", StringComparison.OrdinalIgnoreCase))
                return TenantRole.Teacher;
            if (string.Equals(userType, "STUDENT", StringComparison.OrdinalIgnoreCase))
                return TenantRole.Student;
            if (string.Equals(userType, "GUARDIAN", StringComparison.OrdinalIgnoreCase))
                return TenantRole.Parent;
            return null;
        }

        /// <summary>Backfill <see cref="UserTenant"/> after legacy DB scan so the next login uses membership instead of scanning.</summary>
        private async Task EnsureUserTenantIfMissingAsync(string userId, int tenantId, TenantRole role)
        {
            var exists = await _context.UserTenants.AnyAsync(ut => ut.UserId == userId && ut.TenantId == tenantId);
            if (exists)
                return;

            _context.UserTenants.Add(new UserTenant
            {
                UserId = userId,
                TenantId = tenantId,
                TenantRole = role,
                IsActive = true,
                LastAccessedUtc = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
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
        [AllowAnonymous]
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
        [AllowAnonymous]
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

        [HttpGet("my-tenants")]
        [Authorize]
        public async Task<IActionResult> MyTenants()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var list = await _tenantMembership.GetTenantSummariesAsync(userId);
            return Ok(list);
        }

        [HttpPost("select-tenant")]
        [Authorize]
        public async Task<IActionResult> SelectTenant([FromBody] SelectTenantDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var mem = await _tenantMembership.GetMembershipAsync(userId, dto.TenantId);
            if (mem == null)
                return Forbid();

            var row = await _context.UserTenants
                .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TenantId == dto.TenantId && ut.IsActive);
            if (row != null)
            {
                row.LastAccessedUtc = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized();

            var accessToken = await BuildJwtAsync(user, TimeSpan.FromMinutes(15), dto.TenantId);
            return Ok(new { token = accessToken, tenantId = dto.TenantId, expiration = DateTime.UtcNow.AddMinutes(15) });
        }
    }
}
