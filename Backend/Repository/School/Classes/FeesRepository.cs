using AutoMapper;
using Backend.Data;
using Backend.DTOS.School.Fees;
using Backend.Models;
using Backend.Repository.School.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School.Classes;

public class FeesRepository : IFeesRepository
{
    private readonly DatabaseContext _db;
    private readonly IMapper _mapper;
    public FeesRepository(DatabaseContext db,IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<GetFeeDTO>> GetAllAsync()
    {
        var fees = await _db.Fees.ToListAsync();
        var feesDTO = _mapper.Map<List<GetFeeDTO>>(fees);
        return feesDTO;
    }

    public async Task<GetFeeDTO> GetByIdAsync(int id)
    {
       var fees = await _db.Fees
        .Include(f => f.FeeClasses)
        .FirstOrDefaultAsync(f => f.FeeID == id);
        var fee=_mapper.Map<GetFeeDTO>(fees);

        if (fees == null)
        throw new KeyNotFoundException($"Fee with ID {id} was not found.");

    return fee;
    }

    public async Task AddAsync(FeeDTO fee)
    {
        var NewFee=_mapper.Map<Fee>(fee);

        await _db.Fees.AddAsync(NewFee);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(FeeDTO fee)
    {
        var existingFee = await _db.Fees.FirstOrDefaultAsync(f=>f.FeeID==fee.FeeID);
        if (existingFee != null)
        {
            existingFee.FeeName=fee.FeeName;
            existingFee.Note=fee.Note;
            existingFee.FeeNameAlis=fee.FeeNameAlis;

            _db.Entry(existingFee).State = EntityState.Modified;
            await _db.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var fee = await _db.Fees.FindAsync(id);
        if (fee != null)
        {
            _db.Fees.Remove(fee);
            await _db.SaveChangesAsync();
        }
    }
}
