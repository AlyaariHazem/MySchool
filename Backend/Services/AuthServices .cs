using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Backend.Data;
using Backend.DTOS;
using Backend.DTOS.IdentityDTO;
using Backend.Models;
using Backend.Repository.IRepository;

using Backend.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Services
{
    public class AuthServices : IAuthServices
    {


        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IUserServices _userServices;

        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthServices(IUserServices userServices, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userServices = userServices;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public Task<Accounts> AddAccountAsync(Accounts account)
        {
            throw new NotImplementedException();
        }

        public async Task<IdentityResponseDTO> Login(LoginDTO login)
        {
            try
            {
                IdentityResponseDTO loginResponseDTO = new()
                {
                    User = null,
                    Token = ""
                };
                var user = await _userServices.GetAsync(u => u.UserName.ToLower() == login.UserName.ToLower());
                var checkPassword = await _userManager.CheckPasswordAsync(user, login.Password);
                if (user == null || checkPassword == false)
                {
                    return loginResponseDTO;
                }
                loginResponseDTO.Token = await CreateToken(user);
                var userDTO = new UserDTO()
                {
                    UserName = user.UserName
                };
                loginResponseDTO.User = userDTO;
                return loginResponseDTO;
            }
            catch
            {
                return null;
            }


        }

        public async Task<IdentityResponseDTO> Register(RegisterDTO register)
        {
            try
            {

                ApplicationUser user = new ApplicationUser()
                {
                    UserName = register.UserName,
                    Email = register.UserName,
                    NormalizedEmail = register.UserName.ToUpper()

                };
                var IsUinque = await _userServices.IsUinque(user.Email);
                if (!IsUinque)
                {
                    return null;
                }

                var result = await _userManager.CreateAsync(user, register.Password);

                if (result.Succeeded)
                {
                    if (!_roleManager.RoleExistsAsync("MANAGER").GetAwaiter().GetResult())
                    {
                        await _roleManager.CreateAsync(new IdentityRole("MANAGER"));

                    }
                    await _userManager.AddToRoleAsync(user, "admin");

                    var UserDTO = new UserDTO()
                    {
                        UserName = user.UserName,
                    };
                    IdentityResponseDTO RegisterResponseDTO = new()
                    {
                        Token = await CreateToken(user),
                        User = UserDTO
                    };
                    return RegisterResponseDTO;
                }
                return null;
            }
            catch
            {
                return null;
            }


        }

        public async Task<string> CreateToken(ApplicationUser user)
        {
            var role = await _userManager.GetRolesAsync(user);

            var tokenhandeler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("YourVeryStrongSecretKeyOfAtLeast32Characters");

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Name, user.UserName.ToString()),
                    new Claim(ClaimTypes.Role, role.FirstOrDefault())

                }),
                Expires = DateTime.UtcNow.AddMinutes(7),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenhandeler.CreateToken(tokenDescriptor);

            return tokenhandeler.WriteToken(token);
        }


    }
}


