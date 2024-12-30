using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;
using Backend.Services.IServices;

namespace Backend.Services
{
    public class AccountServices : IAccountServices
    {
        public AccountServices()
        {



        }

        public Task<bool> AddAsync(Accounts accounts)
        {
            throw new NotImplementedException();
        }
    }
}