using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using school = Backend.Models.School;
using Microsoft.EntityFrameworkCore;
using Backend.Repository.IRepository;

namespace Backend.Repository
{
    public class SchoolRepository : Repository<school>, ISchoolRepository
    {
        private readonly DatabaseContext _db;


        public SchoolRepository(DatabaseContext db) : base(db)
        {
            _db = db;
        }
        public async Task UpdateAsync(school obj)
        {
            _db.Schools.Update(obj);
            await SaveAsync();
        }

    }
}
