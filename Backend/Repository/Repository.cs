using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly DatabaseContext _db;
    internal DbSet<T> dbset;
    public Repository(DatabaseContext db)
    {
        _db = db;
        this.dbset = _db.Set<T>();
    }

    public async Task CreateAsync(T entity)
    {
        await dbset.AddAsync(entity);
        await SaveAsync();
    }

    public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null)
    {
        IQueryable<T> query = dbset;

        if (filter != null)
        {
            query = query.Where(filter);
        }
        return await query.ToListAsync();
    }
    

    public async Task<T> GetAsync(Expression<Func<T, bool>> filter = null, bool tracked = true)
    {
        IQueryable<T> query = dbset;
        if (!tracked)
        {
            query = query.AsNoTracking();
        }
        if (filter != null)
        {
            query = query.Where(filter);
        }
        return await query.FirstOrDefaultAsync();
    }

    public async Task RemoveAsync(T entity)
    {
        dbset.Remove(entity);
        await SaveAsync();
    }

    public async Task SaveAsync()
    {
        await _db.SaveChangesAsync();
    }
}
