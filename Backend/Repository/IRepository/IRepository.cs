using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Backend.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Backend.Repository;

public interface IRepository<T> where T : class
{
    Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
    Task<T> GetAsync(Expression<Func<T, bool>> filter = null, bool tracked = true, string? includeProperties = null);
    Task CreateAsync(T entity);
    Task<int> GetCount(Expression<Func<T, bool>>? filter = null);
    Task RemoveAsync(T entity);
    Task SaveAsync();

    Task<IDbContextTransaction> BeginTransactionAsync();
}
