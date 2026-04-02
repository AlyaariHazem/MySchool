using Backend.Common;

namespace Backend.Interfaces;

/// <summary>
/// Generic CRUD repository interface with dynamic filtering, sorting, and pagination.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TKey">The primary key type (int, string, etc.).</typeparam>
public interface IGenericCrudRepository<TEntity, TKey> where TEntity : class
{
    /// <summary>Get a single entity by its primary key.</summary>
    Task<TEntity?> GetByIdAsync(TKey id);

    /// <summary>Get all entities with optional filtering and sorting.</summary>
    Task<List<TEntity>> GetAllAsync(GenericQueryRequest request);

    /// <summary>Get paged entities with optional filtering and sorting.</summary>
    Task<(List<TEntity> Items, int TotalCount)> GetPagedAsync(GenericQueryRequest request);

    /// <summary>Create a new entity.</summary>
    Task<TEntity> CreateAsync(TEntity entity);

    /// <summary>Update an existing entity.</summary>
    Task<TEntity> UpdateAsync(TEntity entity);

    /// <summary>Delete an entity by its primary key.</summary>
    Task<bool> DeleteAsync(TKey id);
}
