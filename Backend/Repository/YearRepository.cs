using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS.School.Years;
using Backend.Models;
using Backend.Repository.School.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Repository.School.Classes;

public class YearRepository : IYearRepository
{
    private readonly TenantDbContext _context;
    private readonly IMapper _mapper;
    public YearRepository(TenantDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task Add(YearDTO yearDTO)
    {
        if (yearDTO == null)
            throw new ArgumentNullException(nameof(yearDTO), "The model cannot be null.");

        var newYear = _mapper.Map<Year>(yearDTO);

        await _context.Years.AddAsync(newYear);
        await _context.SaveChangesAsync();
        yearDTO.YearID = newYear.YearID;
        
        // Add the YearTermMonth entries
        var YearTermMonths = new List<YearTermMonth>(){
            new YearTermMonth(){
                YearID = newYear.YearID,
                TermID = 1,
                MonthID = 5
            },
            new YearTermMonth(){
                YearID = newYear.YearID,
                TermID = 1,
                MonthID = 6
            },
            new YearTermMonth(){
                YearID = newYear.YearID,
                TermID = 1,
                MonthID = 7
            },
            new YearTermMonth(){
                YearID = newYear.YearID,
                TermID = 1,
                MonthID = 8
            },
            new YearTermMonth(){
                YearID = newYear.YearID,
                TermID = 2,
                MonthID = 9
            },
            new YearTermMonth(){
                YearID = newYear.YearID,
                TermID = 2,
                MonthID = 10
            },
            new YearTermMonth(){
                YearID = newYear.YearID,
                TermID = 2,
                MonthID = 11
            },
            new YearTermMonth(){
                YearID = newYear.YearID,
                TermID = 2,
                MonthID = 12
            }
        };
        await _context.YearTermMonths.AddRangeAsync(YearTermMonths);
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

    public async Task<bool> UpdatePartial(int id, JsonPatchDocument<YearDTO> partialyear)
    {
     if (partialyear == null || id == 0)
                return false;

            // Retrieve the year entity by its ID
            var year = await _context.Years.SingleOrDefaultAsync(s => s.YearID == id);
            if (year == null)
                return false;

            // Map the year entity to the DTO (this will be modified)
            var yearDTO = _mapper.Map<YearDTO>(year);

            // Apply the patch to the DTO
            partialyear.ApplyTo(yearDTO);

            // Map the patched DTO back to the entity (year)
            _mapper.Map(yearDTO, year);

            // Mark the entity as modified and save changes
            _context.Entry(year).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;   
    }
}
