using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Backend.Models;
using Backend.Repository.IRepository;

using Backend.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class UsersServices : IUserServices
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserRepository _userRepository;
        public UsersServices(UserManager<ApplicationUser> userManager, IUserRepository userRepository)
        {
            _userManager = userManager;
            _userRepository = userRepository;
        }

        public async Task<bool> UpdateAsync(ApplicationUser user)
        {
            var existingUser = await _userRepository.GetAsync(u => u.Id == user.Id, tracked: false);
            if (existingUser != null)
            {
                existingUser.Address = user.Address;
                existingUser.Gender = user.Gender;
                existingUser.HireDate = user.HireDate;
                existingUser.UserType = user.UserType;
                await _userRepository.UpdateAsync(user);
                return true;

            }
            return false;
        }

        public async Task<bool> DeleteAsync(string userId)
        {
            var user = await _userRepository.GetAsync(u => u.Id == userId);
            if (user != null)
            {
                await _userRepository.RemoveAsync(user);
                return true;
            }
            return false;
        }

        public async Task<ApplicationUser> AddAsync(ApplicationUser user, string password, string role)
        {
            /*   var passwordHasher = new PasswordHasher<ApplicationUser>();
              string hashedPassword = passwordHasher.HashPassword(user, password);
              user.PasswordHash = hashedPassword;
              await _userRepository.CreateAsync(user);
              await _userManager.AddToRoleAsync(user, role); */
            var User = await _userRepository.CreateAsync(user, password, role);
            return User;
        }

        public async Task<ApplicationUser?> GetAsync(Expression<Func<ApplicationUser, bool>> filter)
        {
            var user = await _userRepository.GetAsync(filter, tracked: false);
            if (user != null)
            {
                return user;
            }
            else
            {
                return null;
            }
        }

        public async Task<List<ApplicationUser>> GetAllAsync(Expression<Func<ApplicationUser, bool>> filter = null)
        {
            var users = await _userRepository.GetAllAsync(filter);
            return users;
        }
        public async Task<bool> IsUinque(string Email)
        {
            var user = await _userRepository.GetAsync(u => u.Email == Email);
            if (user == null)
            {
                return true;
            }
            return false;
        }



    }

}

