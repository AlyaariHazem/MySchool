using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

    public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
    {
        IQueryable<T> query = dbset;
        if (includeProperties != null)
        {
            foreach (var includeproperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeproperty);
            }
        }
        if (filter != null)
        {
            query = query.Where(filter);
        }
        return await query.ToListAsync();
    }
    public async Task<int> GetCount(Expression<Func<T, bool>>? filter = null)
    {
        IQueryable<T> query = dbset;
        if (filter != null)
        {
            return await query.Where(filter).CountAsync();


        }
        return await query.CountAsync();

    }

    public async Task<T> GetAsync(Expression<Func<T, bool>> filter = null, bool tracked = true, string includeProperties = null)
    {
        IQueryable<T> query = dbset;
        if (includeProperties != null)
        {
            foreach (var includeproperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeproperty);
            }
        }
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
    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _db.Database.BeginTransactionAsync();
    }
}
