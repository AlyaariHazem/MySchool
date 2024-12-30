using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.Repository.IRepository;

public interface IUserRepository : IRepository<ApplicationUser>
{
    Task<ApplicationUser> CreateAsync(ApplicationUser user, string password, string role);

    Task UpdateAsync(ApplicationUser user);



}
