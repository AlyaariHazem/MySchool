using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.Services.IServices
{
    public interface IAccountServices
    {
        Task<bool> AddAsync(Accounts accounts);
    }
}