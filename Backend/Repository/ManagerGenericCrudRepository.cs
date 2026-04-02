using System.Reflection;
using Backend.Common;
using Backend.DTOS.School.Manager;
using Backend.Interfaces;
using Backend.Repository.School.Implements;

namespace Backend.Repository;

/// <summary>
/// Adapts <see cref="IManagerRepository"/> to <see cref="IGenericCrudRepository{GetManagerDTO,int}"/>.
/// Create uses <c>POST api/Manager/add</c> with <see cref="AddManagerDTO"/> instead.
/// </summary>
public class ManagerGenericCrudRepository : IGenericCrudRepository<GetManagerDTO, int>
{
    private readonly IManagerRepository _managers;

    public ManagerGenericCrudRepository(IManagerRepository managers)
    {
        _managers = managers;
    }

    public async Task<GetManagerDTO?> GetByIdAsync(int id)
    {
        return await _managers.GetManager(id);
    }

    public async Task<List<GetManagerDTO>> GetAllAsync(GenericQueryRequest request)
    {
        var list = await _managers.GetManagers();
        list = ApplyFilters(list, request.Filters);
        list = ApplyOrdering(list, request.Orders);
        return list;
    }

    public async Task<(List<GetManagerDTO> Items, int TotalCount)> GetPagedAsync(GenericQueryRequest request)
    {
        var list = await _managers.GetManagers();
        list = ApplyFilters(list, request.Filters);
        list = ApplyOrdering(list, request.Orders);
        var total = list.Count;
        var page = list
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .ToList();
        return (page, total);
    }

    public Task<GetManagerDTO> CreateAsync(GetManagerDTO entity) =>
        throw new NotSupportedException(
            "Manager creation uses POST api/Manager/add with AddManagerDTO.");

    public async Task<GetManagerDTO> UpdateAsync(GetManagerDTO entity)
    {
        if (entity.ManagerID is not int id)
            throw new ArgumentException("ManagerID is required for update.", nameof(entity));

        await _managers.UpdateManager(entity);
        var updated = await _managers.GetManager(id);
        if (updated == null)
            throw new KeyNotFoundException($"Manager with ID {id} was not found after update.");

        return updated;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _managers.GetManager(id);
        if (existing == null)
            return false;

        await _managers.DeleteManager(id);
        return true;
    }

    private static List<GetManagerDTO> ApplyFilters(List<GetManagerDTO> items, Dictionary<string, string>? filters)
    {
        if (filters == null || filters.Count == 0)
            return items;

        IEnumerable<GetManagerDTO> q = items;
        foreach (var filter in filters)
        {
            if (string.IsNullOrWhiteSpace(filter.Value))
                continue;

            var prop = FindProperty(filter.Key);
            if (prop == null)
                continue;

            q = q.Where(e => MatchesFilter(e, prop, filter.Value));
        }

        return q.ToList();
    }

    private static List<GetManagerDTO> ApplyOrdering(List<GetManagerDTO> items, Dictionary<string, int>? orders)
    {
        if (orders == null || orders.Count == 0)
            return items;

        IOrderedEnumerable<GetManagerDTO>? ordered = null;
        var first = true;

        foreach (var order in orders)
        {
            var prop = FindProperty(order.Key);
            if (prop == null)
                continue;

            if (first)
            {
                ordered = order.Value < 0
                    ? items.OrderByDescending(e => prop.GetValue(e), Comparer<object?>.Default)
                    : items.OrderBy(e => prop.GetValue(e), Comparer<object?>.Default);
                first = false;
            }
            else
            {
                ordered = order.Value < 0
                    ? ordered!.ThenByDescending(e => prop.GetValue(e), Comparer<object?>.Default)
                    : ordered!.ThenBy(e => prop.GetValue(e), Comparer<object?>.Default);
            }
        }

        return ordered?.ToList() ?? items;
    }

    private static PropertyInfo? FindProperty(string name) =>
        typeof(GetManagerDTO).GetProperty(
            name,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

    private static bool MatchesFilter(GetManagerDTO e, PropertyInfo prop, string value)
    {
        var raw = prop.GetValue(e);

        if (prop.PropertyType == typeof(string))
        {
            var s = raw as string;
            return s != null && s.Contains(value, StringComparison.OrdinalIgnoreCase);
        }

        if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?))
        {
            if (!int.TryParse(value, out var intVal))
                return true;

            if (prop.PropertyType == typeof(int?))
            {
                var ni = (int?)raw;
                return ni.HasValue && ni.Value == intVal;
            }

            return raw is int i && i == intVal;
        }

        if (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool?))
        {
            if (!bool.TryParse(value, out var boolVal))
                return true;

            if (prop.PropertyType == typeof(bool?))
            {
                var nb = (bool?)raw;
                return nb.HasValue && nb.Value == boolVal;
            }

            return raw is bool b && b == boolVal;
        }

        if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
        {
            if (!DateTime.TryParse(value, out var dateVal))
                return true;

            if (prop.PropertyType == typeof(DateTime?))
            {
                var nd = (DateTime?)raw;
                return nd.HasValue && nd.Value.Date == dateVal.Date;
            }

            return raw is DateTime dt && dt.Date == dateVal.Date;
        }

        if (prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?))
        {
            if (!decimal.TryParse(value, out var decVal))
                return true;

            if (prop.PropertyType == typeof(decimal?))
            {
                var nm = (decimal?)raw;
                return nm.HasValue && nm.Value == decVal;
            }

            return raw is decimal m && m == decVal;
        }

        return true;
    }
}
