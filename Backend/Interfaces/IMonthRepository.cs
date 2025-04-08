using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.Months;

namespace Backend.Interfaces;

public interface IMonthRepository
{
    Task AddMonthAsync(MonthDTO month);
    Task<List<MonthDTO>> GetAllMonthsAsync();
    Task<MonthDTO> GetMonthByIdAsync(int id);
    Task UpdateMonthAsync(MonthDTO month);
    Task DeleteMonthAsync(int id);
}
