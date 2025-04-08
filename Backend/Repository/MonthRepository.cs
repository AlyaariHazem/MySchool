using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS.School.Months;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Backend.Repository;

public class MonthRepository : IMonthRepository
{
    private readonly DatabaseContext _context;
    private readonly IMapper _mapper;
    public MonthRepository(DatabaseContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task AddMonthAsync(MonthDTO month)
    {
        if (month == null)
            throw new ArgumentNullException(nameof(month), $"Month is not found.");

        var monthEntity = _mapper.Map<Month>(month);
        await _context.Months.AddAsync(monthEntity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteMonthAsync(int id)
    {
        var month = await _context.Months.FirstOrDefaultAsync(m => m.MonthID == id);
        if (month == null)
            throw new ArgumentNullException(nameof(month), $"Month with ID {id} not found.");

        _context.Months.Remove(month);
        await _context.SaveChangesAsync();
    }

    public async Task<List<MonthDTO>> GetAllMonthsAsync()
    {
        var months = await _context.Months.ToListAsync();
        if (months == null)
            return new List<MonthDTO>();

        var monthDtos = _mapper.Map<List<MonthDTO>>(months);
        return monthDtos;
    }

    public async Task<MonthDTO> GetMonthByIdAsync(int id)
    {
        var month = await _context.Months.FirstOrDefaultAsync(m => m.MonthID == id);
        if (month == null)
            throw new ArgumentNullException(nameof(month), $"Month with ID {id} not found.");

        var monthDto = _mapper.Map<MonthDTO>(month);
        return monthDto;
    }

    public async Task UpdateMonthAsync(MonthDTO month)
    {
        var monthEntity = await _context.Months.FirstOrDefaultAsync(m => m.MonthID == month.MonthID);
        if (monthEntity == null)
            throw new ArgumentNullException(nameof(month), $"Month with ID {month.MonthID} not found.");
            
        var MonthEntity = _mapper.Map<Month>(month);
        _context.Months.Update(MonthEntity);
        await _context.SaveChangesAsync();
    }
}
