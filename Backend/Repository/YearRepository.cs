using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS.School.Years;
using Backend.Models;
using Backend.Repository.School.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Repository.School.Classes;

public class YearRepository : IYearRepository
{
    private readonly DatabaseContext _context;
    private readonly IMapper _mapper;
    public YearRepository(DatabaseContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task Add(YearDTO yearDTO)
    {
        if (yearDTO == null)
            throw new ArgumentNullException(nameof(yearDTO), "The model cannot be null.");

        var newYear = new Year
        {
            YearDateStart = yearDTO.YearDateStart,
            YearDateEnd = yearDTO.YearDateEnd,
            HireDate = DateTime.Now,
            Active = yearDTO.Active,
            SchoolID = yearDTO.SchoolID
        };
        await _context.Years.AddAsync(newYear);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        if (id == 0)
            throw new ArgumentNullException(nameof(id), "The id cannot be null.");

        var year = await _context.Years.FirstOrDefaultAsync(x => x.YearID == id);
        if (year == null)
            throw new ArgumentNullException(nameof(year), "The year cannot be null.");

        _context.Years.Remove(year);
        await _context.SaveChangesAsync();
    }

    public async Task<List<YearDTO>> GetAll()
    {
        var years = await _context.Years.ToListAsync();
        var yearList = _mapper.Map<List<YearDTO>>(years);
        return yearList;
    }

    public async Task<YearDTO> GetByIdAsync(int id)
    {
        if (id == 0)
            throw new ArgumentNullException(nameof(id), "The id cannot be null.");

        var year = await _context.Years.FirstOrDefaultAsync(x => x.YearID == id);
        var yearDTO = _mapper.Map<YearDTO>(year);

        if (year == null)
            throw new ArgumentNullException(nameof(year), "The year cannot be null.");

        return yearDTO;
    }

    public async Task Update(YearDTO obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj), "The model cannot be null.");

        var existingYear = _context.Years.FirstOrDefault(x => x.YearID == obj.YearID);
        if (existingYear == null)
            throw new ArgumentNullException(nameof(existingYear), "The year cannot be null.");

        existingYear.YearDateStart = obj.YearDateStart;
        existingYear.YearDateEnd = obj.YearDateEnd;
        existingYear.Active = obj.Active;
        existingYear.SchoolID = obj.SchoolID;

        _context.Entry(existingYear).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

}
