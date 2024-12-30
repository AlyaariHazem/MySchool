using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Models;
using Backend.Repository.IRepository;

namespace Backend.Repository
{
    public class AccountRepository : Repository<Accounts>, IAccountRepository
    {
        private readonly DatabaseContext _db;

        public AccountRepository(DatabaseContext db) : base(db)
        {
            _db = db;

        }

        public async Task UpdateAsync(Accounts obj)
        {
            _db.Accounts.Update(obj);
            await SaveAsync();
        }
    }
}