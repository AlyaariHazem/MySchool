using Backend.DTOS.School.Months;
using Backend.Common;

namespace Backend.Interfaces;

public interface IMonthRepository
{
    Task<Result<bool>> AddMonthAsync(MonthDTO month);
    Task<Result<List<MonthDTO>>> GetAllMonthsAsync();
    Task<Result<MonthDTO>> GetMonthByIdAsync(int id);
    Task<Result<bool>> UpdateMonthAsync(MonthDTO month);
    Task<Result<bool>> DeleteMonthAsync(int id);
}
