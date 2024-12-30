using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Models;
using Backend.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository;

public class UsersRepository : Repository<ApplicationUser>, IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    private readonly RoleManager<IdentityRole> _roleManager;

    private readonly DatabaseContext _db;

    public UsersRepository(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, DatabaseContext db) : base(db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }


    public async Task<ApplicationUser> CreateAsync(ApplicationUser user, string password, string role)
    {
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        await _userManager.AddToRoleAsync(user, role);
        return user;
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
            // Update other properties if needed
            await _userManager.UpdateAsync(existingUser);
        }
    }




}
