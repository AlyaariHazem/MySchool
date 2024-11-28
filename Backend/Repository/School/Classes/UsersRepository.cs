using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;
using Backend.Repository.School.Implements;
using Microsoft.AspNetCore.Identity;

namespace Backend.Repository.School.Classes;

public class UsersRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersRepository(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ApplicationUser> CreateUserAsync(ApplicationUser user, string password, string role)
    {
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        await _userManager.AddToRoleAsync(user, role);
        return user;
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId);
    }
}
