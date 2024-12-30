using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Backend.Models;

namespace Backend.Services.IServices
{
    public interface IUserServices
    {
        Task<ApplicationUser> AddAsync(ApplicationUser user, string password, string role);
        Task<ApplicationUser?> GetAsync(Expression<Func<ApplicationUser, bool>> filter);
        Task<List<ApplicationUser>> GetAllAsync(Expression<Func<ApplicationUser, bool>> filter = null);

        Task<bool> UpdateAsync(ApplicationUser user);
        Task<bool> IsUinque(string Email);
        Task<bool> DeleteAsync(string userId);
    }
}


