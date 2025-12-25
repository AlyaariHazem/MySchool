using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;
using Backend.Repository.School.Implements;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
    public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task AddAsync(ApplicationUser user)
        {
            await _userManager.CreateAsync(user);
        }

        public async Task UpdateAsync(ApplicationUser user)
        {
            var existingUser = await _userManager.FindByIdAsync(user.Id);
            if (existingUser != null)
            {
                existingUser.Address = user.Address;
                existingUser.Gender = user.Gender;
                existingUser.HireDate = user.HireDate;
                existingUser.UserType = user.UserType;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.UserName = user.UserName;
                // Update other properties if needed
                await _userManager.UpdateAsync(existingUser);
            }
        }

        public async Task DeleteAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
        }
}
