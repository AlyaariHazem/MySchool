using System.Linq;
using System.Reflection;
using Backend.Common;
using Backend.DTOS.School.Employee;
using Backend.Interfaces;

namespace Backend.Repository;

/// <summary>
/// Adapts aggregate employee operations (teachers + managers) to <see cref="IGenericCrudRepository{TEntity,TKey}"/>.
/// Filtering, sorting, and paging run in-memory on the combined DTO list.
/// </summary>
public class EmployeeGenericCrudRepository : IGenericCrudRepository<EmployeeDTO, int>
{
    private readonly IEmployeeRepository _employees;

    public EmployeeGenericCrudRepository(IEmployeeRepository employees)
    {
        _employees = employees;
    }

    public async Task<EmployeeDTO?> GetByIdAsync(int id)
    {
        return await _employees.GetEmployeeByIdAsync(id);
    }

    public async Task<List<EmployeeDTO>> GetAllAsync(GenericQueryRequest request)
    {
        var list = await _employees.GetAllEmployeesAsync();
        list = ApplyFilters(list, request.Filters);
        list = ApplyOrdering(list, request.Orders);
        return list;
    }

    public async Task<(List<EmployeeDTO> Items, int TotalCount)> GetPagedAsync(GenericQueryRequest request)
    {
        var list = await _employees.GetAllEmployeesAsync();
        list = ApplyFilters(list, request.Filters);
        list = ApplyOrdering(list, request.Orders);
        var total = list.Count;
        var page = list
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .ToList();
        return (page, total);
    }

    public async Task<EmployeeDTO> CreateAsync(EmployeeDTO entity)
    {
        return await _employees.AddEmployeeAsync(entity);
    }

    public async Task<EmployeeDTO> UpdateAsync(EmployeeDTO entity)
    {
        if (entity.EmployeeID is not int id)
            throw new ArgumentException("EmployeeID is required for update.", nameof(entity));

        var updated = await _employees.UpdateEmployeeAsync(id, entity);
        if (updated == null)
            throw new KeyNotFoundException($"Employee with ID {id} was not found or could not be updated.");

        return updated;
    }

    public Task<bool> DeleteAsync(int id) =>
        throw new NotSupportedException(
            "Employee delete requires a job type. Use DELETE api/Employee/{id}?jobType=Teacher|Manager.");

    private static List<EmployeeDTO> ApplyFilters(List<EmployeeDTO> items, Dictionary<string, string>? filters)
    {
        if (filters == null || filters.Count == 0)
            return items;

        IEnumerable<EmployeeDTO> q = items;
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

    private static List<EmployeeDTO> ApplyOrdering(List<EmployeeDTO> items, Dictionary<string, int>? orders)
    {
        if (orders == null || orders.Count == 0)
            return items;

        IOrderedEnumerable<EmployeeDTO>? ordered = null;
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
        typeof(EmployeeDTO).GetProperty(
            name,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

    private static bool MatchesFilter(EmployeeDTO e, PropertyInfo prop, string value)
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
