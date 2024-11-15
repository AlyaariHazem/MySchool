using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.Repository;

public interface IRepository<T> where T : class
{
    Task<List<T>> GetAllAsync(Expression<Func<T,bool>>? filter = null);
    Task<T> GetAsync(Expression<Func<T,bool>> filter = null, bool tracked = true);
    Task CreateAsync(T entity);
    Task RemoveAsync(T entity);
    Task SaveAsync();
}
