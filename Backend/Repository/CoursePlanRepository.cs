using AutoMapper;
using Backend.Data;
using Backend.DTOS.School.CoursePlan;
using Backend.Models;
using Backend.Repository.School.Implements;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository;

public class CoursePlanRepository : ICoursePlanRepository
{
    private readonly DatabaseContext _context;
    private readonly IMapper _mapper;

    public CoursePlanRepository(DatabaseContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CoursePlanDTO> AddAsync(CoursePlanDTO dto)
    {
        var entity = _mapper.Map<CoursePlan>(dto);
        _context.CoursePlans.Add(entity);
        await _context.SaveChangesAsync();
        dto.CoursePlanID = entity.CoursePlanID;
        return dto;
    }

    public async Task<List<CoursePlanDTO>> GetAllAsync()
    {
        var list = await _context.CoursePlans
            .Include(p => p.Subject)
            .Include(p => p.Teacher)
            .Include(p => p.Class)
            .Include(p => p.Division)
            .Include(p => p.Term)
            .Include(p => p.Year)
            .ToListAsync();

        return _mapper.Map<List<CoursePlanDTO>>(list);
    }

    public async Task<CoursePlanDTO?> GetByIdAsync(int id)
    {
        var entity = await _context.CoursePlans.FindAsync(id);
        return entity == null ? null : _mapper.Map<CoursePlanDTO>(entity);
    }

    public async Task UpdateAsync(CoursePlanDTO dto)
    {
        var entity = await _context.CoursePlans.FindAsync(dto.CoursePlanID);
        if (entity == null)
            throw new KeyNotFoundException("Course plan not found.");

        _mapper.Map(dto, entity);
        _context.CoursePlans.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.CoursePlans.FindAsync(id);
        if (entity == null)
            throw new KeyNotFoundException("Course plan not found.");

        _context.CoursePlans.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
