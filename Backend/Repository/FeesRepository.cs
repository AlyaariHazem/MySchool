using AutoMapper;
using Backend.Data;

using Backend.Models;
using Backend.Repository.IRepository;
using Backend.Repository.School.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository;

public class FeesRepository : Repository<Fee>, IFeesRepository
{
    private readonly DatabaseContext _db;

    public FeesRepository(DatabaseContext db, IMapper mapper) : base(db)
    {
        _db = db;

    }


    public async Task UpdateAsync(Fee obj)
    {
        _db.Fees.Update(obj);
        await SaveAsync();
    }


}
