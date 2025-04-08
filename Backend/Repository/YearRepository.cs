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

        var newYear = _mapper.Map<Year>(yearDTO);

        await _context.Years.AddAsync(newYear);
        await _context.SaveChangesAsync();
        yearDTO.YearID = newYear.YearID;
        
        var term = new Term
        {
            TermID = 0,
            YearID = yearDTO.YearID ?? 1,
            Name = "الأول"
        };
        var term2 = new Term
        {
            TermID = 0,
            YearID = yearDTO.YearID ?? 1,
            Name = "الثاني"
        };
        _context.Terms.Add(term);
        _context.Terms.Add(term2);
        await _context.SaveChangesAsync();
        
        var month2 = new Month
        {
            MonthID = 0,
            TermID = term.TermID,
            Name = "يوليو"
        };
        var month3 = new Month
        {
            MonthID = 0,
            TermID = term.TermID,
            Name = "أغسطس"
        };
        var month4 = new Month
        {
            MonthID = 0,
            TermID = term.TermID,
            Name = "سبتمبر"
        };
        var month5 = new Month
        {
            MonthID = 0,
            TermID = term2.TermID,
            Name = "أكتوبر"
        };
        var month6 = new Month
        {
            MonthID = 0,
            TermID = term2.TermID,
            Name = "نوفمبر"
        };
        var month7 = new Month
        {
            MonthID = 0,
            TermID = term2.TermID,
            Name = "ديسمبر"
        };
        _context.Months.Add(month2);
        _context.Months.Add(month3);
        _context.Months.Add(month4);
        _context.Months.Add(month5);
        _context.Months.Add(month6);
        _context.Months.Add(month7);
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
