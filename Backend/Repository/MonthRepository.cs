using AutoMapper;
using Backend.Common;
using Backend.Data;
using Backend.DTOS.School.Months;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository;

public class MonthRepository : IMonthRepository
{
    private readonly TenantDbContext _context;
    private readonly IMapper _mapper;

    public MonthRepository(TenantDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<bool>> AddMonthAsync(MonthDTO dto)
    {
        if (dto is null)
            return Result<bool>.Fail("Month payload is null.");

        bool exists = await _context.Months.AnyAsync(m => m.Name == dto.Name);
        if (exists)
            return Result<bool>.Fail($"Month '{dto.Name}' already exists.");

        await _context.Months.AddAsync(_mapper.Map<Month>(dto));

        try
        {
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
        catch (DbUpdateException ex)
        {
            return Result<bool>.Fail($"DB error: {ex.Message}");
        }
    }

    public async Task<Result<List<MonthDTO>>> GetAllMonthsAsync()
    {
        var list = await _context.Months
                                 .Select(m => _mapper.Map<MonthDTO>(m))
                                 .ToListAsync();

        return list.Count == 0
            ? Result<List<MonthDTO>>.Fail("No months found.")
            : Result<List<MonthDTO>>.Success(list);
    }

    public async Task<Result<MonthDTO>> GetMonthByIdAsync(int id)
    {
        var month = await _context.Months.FindAsync(id);

        return month is null
            ? Result<MonthDTO>.Fail($"Month {id} not found.")
            : Result<MonthDTO>.Success(_mapper.Map<MonthDTO>(month));
    }

    public async Task<Result<bool>> UpdateMonthAsync(MonthDTO dto)
    {
        var entity = await _context.Months.FindAsync(dto.MonthID);
        if (entity is null)
            return Result<bool>.Fail($"Month {dto.MonthID} not found.");

        _mapper.Map(dto, entity);              // update in-place
        try
        {
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
        catch (DbUpdateException ex)
        {
            return Result<bool>.Fail($"DB error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteMonthAsync(int id)
    {
        var entity = await _context.Months.FindAsync(id);
        if (entity is null)
            return Result<bool>.Fail($"Month {id} not found.");

        _context.Months.Remove(entity);

        try
        {
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
        catch (DbUpdateException ex)
        {
            return Result<bool>.Fail($"DB error: {ex.Message}");
        }
    }
}
