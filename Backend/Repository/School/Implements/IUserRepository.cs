using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.Repository.School.Implements;

public interface IUserRepository
{
    Task<ApplicationUser> CreateUserAsync(ApplicationUser user, string password, string role);
    Task<ApplicationUser?> GetUserByIdAsync(string userId);
}
