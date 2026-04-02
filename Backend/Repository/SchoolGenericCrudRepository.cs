using System.Reflection;
using Backend.Common;
using Backend.DTOS.School;
using Backend.Interfaces;
using Backend.Repository.School.Implements;

namespace Backend.Repository;

/// <summary>
/// Adapts <see cref="ISchoolRepository"/> to <see cref="IGenericCrudRepository{SchoolDTO,int}"/>.
/// Filtering, sorting, and paging run in-memory on the DTO list.
/// Create is not supported here — new schools use tenant provisioning on the controller.
/// </summary>
public class SchoolGenericCrudRepository : IGenericCrudRepository<SchoolDTO, int>
{
    private readonly ISchoolRepository _schools;

    public SchoolGenericCrudRepository(ISchoolRepository schools)
    {
        _schools = schools;
    }

    public async Task<SchoolDTO?> GetByIdAsync(int id)
    {
        try
        {
            return await _schools.GetByIdAsync(id);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    public async Task<List<SchoolDTO>> GetAllAsync(GenericQueryRequest request)
    {
        var list = await _schools.GetAllAsync();
        list = ApplyFilters(list, request.Filters);
        list = ApplyOrdering(list, request.Orders);
        return list;
    }

    public async Task<(List<SchoolDTO> Items, int TotalCount)> GetPagedAsync(GenericQueryRequest request)
    {
        var list = await _schools.GetAllAsync();
        list = ApplyFilters(list, request.Filters);
        list = ApplyOrdering(list, request.Orders);
        var total = list.Count;
        var page = list
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .ToList();
        return (page, total);
    }

    public Task<SchoolDTO> CreateAsync(SchoolDTO entity) =>
        throw new NotSupportedException(
            "School creation uses tenant provisioning. POST api/School is handled by SchoolController.");

    public async Task<SchoolDTO> UpdateAsync(SchoolDTO entity)
    {
        if (entity.SchoolID is not int id)
            throw new ArgumentException("SchoolID is required for update.", nameof(entity));

        await _schools.UpdateAsync(entity);
        return await _schools.GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            await _schools.DeleteAsync(id);
            return true;
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }

    private static List<SchoolDTO> ApplyFilters(List<SchoolDTO> items, Dictionary<string, string>? filters)
    {
        if (filters == null || filters.Count == 0)
            return items;

        IEnumerable<SchoolDTO> q = items;
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

    private static List<SchoolDTO> ApplyOrdering(List<SchoolDTO> items, Dictionary<string, int>? orders)
    {
        if (orders == null || orders.Count == 0)
            return items;

        IOrderedEnumerable<SchoolDTO>? ordered = null;
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
        typeof(SchoolDTO).GetProperty(
            name,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

    private static bool MatchesFilter(SchoolDTO e, PropertyInfo prop, string value)
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
