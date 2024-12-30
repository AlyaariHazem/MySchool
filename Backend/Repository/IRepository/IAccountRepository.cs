using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.Repository.IRepository
{
    public interface IAccountRepository : IRepository<Accounts>
    {
        Task UpdateAsync(Accounts obj);
    }
}