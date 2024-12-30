using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;

using Backend.Models;
using Backend.Repository.School.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Backend.Repository;

public class YearRepository : Repository<Year>, IYearRepository
{
    private readonly DatabaseContext _db;
    public YearRepository(DatabaseContext db) : base(db)
    {
        _db = db;
    }

    public Task UpdateAsync(Year obj)
    {
        _db.Years.Update(obj);
        return SaveAsync();
    }

}
