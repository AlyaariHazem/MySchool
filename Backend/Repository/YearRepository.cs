using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Common;
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

        var existingYear = await _context.Years.FirstOrDefaultAsync(x => x.YearID == obj.YearID);
        if (existingYear == null)
            throw new ArgumentNullException(nameof(existingYear), "The year cannot be null.");

        // If setting this year to active, set all other years to inactive
        if (obj.Active)
        {
            var otherYears = await _context.Years
                .Where(y => y.YearID != obj.YearID && y.SchoolID == obj.SchoolID && y.Active)
                .ToListAsync();
            
            foreach (var year in otherYears)
            {
                year.Active = false;
                _context.Entry(year).State = EntityState.Modified;
            }
        }

        // Use AutoMapper to update all properties from DTO to entity
        _mapper.Map(obj, existingYear);

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
            var wasActive = yearDTO.Active;

            // Check if the patch modifies the Active property
            var activePropertyModified = partialyear.Operations
                .Any(op => op.path.Equals("/active", StringComparison.OrdinalIgnoreCase) || 
                          op.path.Equals("/Active", StringComparison.OrdinalIgnoreCase));

            // Apply the patch to the DTO
            partialyear.ApplyTo(yearDTO);

            // If Active property was modified and is now true, set all other years to inactive
            if (activePropertyModified && yearDTO.Active)
            {
                var otherYears = await _context.Years
                    .Where(y => y.YearID != id && y.SchoolID == year.SchoolID && y.Active)
                    .ToListAsync();
                
                foreach (var otherYear in otherYears)
                {
                    otherYear.Active = false;
                    _context.Entry(otherYear).State = EntityState.Modified;
                }
            }

            // Map the patched DTO back to the entity (year)
            _mapper.Map(yearDTO, year);

            // Mark the entity as modified and save changes
            _context.Entry(year).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;   
    }

    public async Task<(List<YearDTO> Items, int TotalCount)> GetYearsPageWithFiltersAsync(int pageNumber, int pageSize, Dictionary<string, FilterValue> filters, CancellationToken cancellationToken = default)
    {
        // Start with base query
        var query = _context.Years.AsQueryable();

        // Apply filters dynamically
        foreach (var filter in filters)
        {
            var columnName = filter.Key;
            var filterValue = filter.Value;

            query = columnName.ToLower() switch
            {
                "yearid" or "yearId" => filterValue.IntValue.HasValue
                    ? query.Where(y => y.YearID == filterValue.IntValue.Value)
                    : query,
                "schoolid" or "schoolId" => filterValue.IntValue.HasValue
                    ? query.Where(y => y.SchoolID == filterValue.IntValue.Value)
                    : query,
                "yeardatestart" or "yearDateStart" => filterValue.DateValue.HasValue
                    ? query.Where(y => y.YearDateStart.Date == filterValue.DateValue.Value.Date)
                    : query,
                "yeardateend" or "yearDateEnd" => filterValue.DateValue.HasValue
                    ? query.Where(y => y.YearDateEnd.HasValue && y.YearDateEnd.Value.Date == filterValue.DateValue.Value.Date)
                    : query,
                "hiredate" or "hireDate" => filterValue.DateValue.HasValue
                    ? query.Where(y => y.HireDate.Date == filterValue.DateValue.Value.Date)
                    : query,
                "active" => filterValue.BoolValue.HasValue
                    ? query.Where(y => y.Active == filterValue.BoolValue.Value)
                    : query,
                _ => query // Unknown filter, ignore it
            };
        }

        // Get total count with filters applied
        var totalCount = await query.CountAsync(cancellationToken);

        if (totalCount == 0)
            return (new List<YearDTO>(), 0);

        // Apply pagination
        var years = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        if (years == null || !years.Any())
            return (new List<YearDTO>(), totalCount);

        // Map to DTOs
        var items = _mapper.Map<List<YearDTO>>(years);
        return (items, totalCount);
    }
}
