using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS.School.Terms;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository;

public class TermRepository : ITermRepository
{
    private readonly DatabaseContext _context;
    private readonly IMapper _mapper;
    public TermRepository(DatabaseContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task AddTermAsync(TermDTO term)
    {
        if (term.Equals(null))
            throw new ArgumentNullException(nameof(term));
        var termEntity = _mapper.Map<Term>(term);
        await _context.Terms.AddAsync(termEntity);
        await _context.SaveChangesAsync();
    }

    public async Task<List<TermDTO>> GetAllTermsAsync()
    {
        var terms = await _context.Terms.ToListAsync();
        if (terms == null)
            return new List<TermDTO>();

        var termDtos = _mapper.Map<List<TermDTO>>(terms);
        return termDtos;
    }

    public async Task<TermDTO> GetTermByIdAsync(int id)
    {
        var term = await _context.Terms.FirstOrDefaultAsync(t => t.TermID == id);
        if (term == null)
            throw new ArgumentNullException(nameof(term), $"Term with ID {id} not found.");

        // Map Entity → DTO
        var termDto = _mapper.Map<TermDTO>(term);
        return termDto;
    }

    public async Task UpdateTermAsync(TermDTO term)
    {
        if (term.Equals(null))
            throw new ArgumentNullException(nameof(term));
        var termEntity = _mapper.Map<Term>(term);

        _context.Terms.Update(termEntity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteTermAsync(int id)
    {
        var term = await _context.Terms.FirstOrDefaultAsync(t => t.TermID == id);
        if (term == null)
            throw new ArgumentNullException(nameof(term), $"Term with ID {id} not found.");

        _context.Terms.Remove(term);
        await _context.SaveChangesAsync();
    }

}
