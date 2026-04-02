using System.Linq.Expressions;
using System.Reflection;
using Backend.Common;
using Backend.Data;
using Backend.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository;

/// <summary>
/// Generic CRUD repository with dynamic filtering, sorting, and pagination.
/// Works with any EF Core entity registered in TenantDbContext.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TKey">The primary key type.</typeparam>
public class GenericCrudRepository<TEntity, TKey> : IGenericCrudRepository<TEntity, TKey>
    where TEntity : class
{
    protected readonly TenantDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public GenericCrudRepository(TenantDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    // ──────────────── READ ────────────────

    public virtual async Task<TEntity?> GetByIdAsync(TKey id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<List<TEntity>> GetAllAsync(GenericQueryRequest request)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        query = ApplyFilters(query, request.Filters);
        query = ApplyOrdering(query, request.Orders);

        return await query.ToListAsync();
    }

    public virtual async Task<(List<TEntity> Items, int TotalCount)> GetPagedAsync(GenericQueryRequest request)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        query = ApplyFilters(query, request.Filters);

        var totalCount = await query.CountAsync();

        query = ApplyOrdering(query, request.Orders);
        query = query.Skip(request.PageIndex * request.PageSize)
                     .Take(request.PageSize);

        var items = await query.ToListAsync();
        return (items, totalCount);
    }

    // ──────────────── WRITE ────────────────

    public virtual async Task<TEntity> CreateAsync(TEntity entity)
    {
        _dbSet.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<TEntity> UpdateAsync(TEntity entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(TKey id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity == null) return false;

        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    // ──────────────── DYNAMIC FILTERING ────────────────

    /// <summary>
    /// Applies dynamic filters by matching dictionary keys to entity property names.
    /// String properties use Contains (case-insensitive); other types use exact match.
    /// </summary>
    protected virtual IQueryable<TEntity> ApplyFilters(IQueryable<TEntity> query, Dictionary<string, string> filters)
    {
        if (filters == null || filters.Count == 0)
            return query;

        foreach (var filter in filters)
        {
            if (string.IsNullOrWhiteSpace(filter.Value))
                continue;

            var property = FindProperty(filter.Key);
            if (property == null)
                continue; // Skip unknown columns

            query = ApplyFilter(query, property, filter.Value);
        }

        return query;
    }

    /// <summary>
    /// Builds an expression-based Where clause for a single filter.
    /// </summary>
    private IQueryable<TEntity> ApplyFilter(IQueryable<TEntity> query, PropertyInfo property, string value)
    {
        // Build: entity => entity.Property.Contains(value)  OR  entity => entity.Property == value
        var parameter = Expression.Parameter(typeof(TEntity), "e");
        Expression propertyAccess = Expression.Property(parameter, property);

        Expression body;

        if (property.PropertyType == typeof(string))
        {
            // String: use Contains (case-insensitive via EF.Functions.Like)
            var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;

            // e.Property != null && e.Property.ToLower().Contains(value.ToLower())
            var notNull = Expression.NotEqual(propertyAccess, Expression.Constant(null, typeof(string)));
            var toLower = Expression.Call(propertyAccess, toLowerMethod);
            var contains = Expression.Call(toLower, containsMethod, Expression.Constant(value.ToLower()));
            body = Expression.AndAlso(notNull, contains);
        }
        else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
        {
            if (int.TryParse(value, out var intVal))
            {
                var targetExpr = property.PropertyType == typeof(int?)
                    ? (Expression)Expression.Property(propertyAccess, "Value")
                    : propertyAccess;
                body = Expression.Equal(targetExpr, Expression.Constant(intVal));

                if (property.PropertyType == typeof(int?))
                {
                    var hasValue = Expression.Property(propertyAccess, "HasValue");
                    body = Expression.AndAlso(hasValue, body);
                }
            }
            else return query;
        }
        else if (property.PropertyType == typeof(decimal) || property.PropertyType == typeof(decimal?))
        {
            if (decimal.TryParse(value, out var decVal))
            {
                var targetExpr = property.PropertyType == typeof(decimal?)
                    ? (Expression)Expression.Property(propertyAccess, "Value")
                    : propertyAccess;
                body = Expression.Equal(targetExpr, Expression.Constant(decVal));

                if (property.PropertyType == typeof(decimal?))
                {
                    var hasValue = Expression.Property(propertyAccess, "HasValue");
                    body = Expression.AndAlso(hasValue, body);
                }
            }
            else return query;
        }
        else if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
        {
            if (bool.TryParse(value, out var boolVal))
            {
                var targetExpr = property.PropertyType == typeof(bool?)
                    ? (Expression)Expression.Property(propertyAccess, "Value")
                    : propertyAccess;
                body = Expression.Equal(targetExpr, Expression.Constant(boolVal));

                if (property.PropertyType == typeof(bool?))
                {
                    var hasValue = Expression.Property(propertyAccess, "HasValue");
                    body = Expression.AndAlso(hasValue, body);
                }
            }
            else return query;
        }
        else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
        {
            if (DateTime.TryParse(value, out var dateVal))
            {
                var targetExpr = property.PropertyType == typeof(DateTime?)
                    ? (Expression)Expression.Property(propertyAccess, "Value")
                    : propertyAccess;

                // Compare date only (ignore time)
                var dateProp = Expression.Property(targetExpr, "Date");
                body = Expression.Equal(dateProp, Expression.Constant(dateVal.Date));

                if (property.PropertyType == typeof(DateTime?))
                {
                    var hasValue = Expression.Property(propertyAccess, "HasValue");
                    body = Expression.AndAlso(hasValue, body);
                }
            }
            else return query;
        }
        else
        {
            // Fallback: try ToString().Contains() — won't translate to SQL for all types
            return query;
        }

        var lambda = Expression.Lambda<Func<TEntity, bool>>(body, parameter);
        return query.Where(lambda);
    }

    // ──────────────── DYNAMIC ORDERING ────────────────

    /// <summary>
    /// Applies dynamic ordering. Orders dictionary: key = column name, value = 1 (ASC) or -1 (DESC).
    /// </summary>
    protected virtual IQueryable<TEntity> ApplyOrdering(IQueryable<TEntity> query, Dictionary<string, int> orders)
    {
        if (orders == null || orders.Count == 0)
            return query;

        bool isFirst = true;

        foreach (var order in orders)
        {
            var property = FindProperty(order.Key);
            if (property == null)
                continue;

            var parameter = Expression.Parameter(typeof(TEntity), "e");
            var propertyAccess = Expression.Property(parameter, property);
            var lambda = Expression.Lambda(propertyAccess, parameter);

            string methodName;
            if (isFirst)
            {
                methodName = order.Value < 0 ? "OrderByDescending" : "OrderBy";
                isFirst = false;
            }
            else
            {
                methodName = order.Value < 0 ? "ThenByDescending" : "ThenBy";
            }

            var resultExpression = Expression.Call(
                typeof(Queryable),
                methodName,
                new[] { typeof(TEntity), property.PropertyType },
                query.Expression,
                Expression.Quote(lambda));

            query = query.Provider.CreateQuery<TEntity>(resultExpression);
        }

        return query;
    }

    // ──────────────── HELPERS ────────────────

    /// <summary>
    /// Finds a property on the entity type by name (case-insensitive).
    /// </summary>
    private PropertyInfo? FindProperty(string propertyName)
    {
        return typeof(TEntity).GetProperty(
            propertyName,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
    }
}
