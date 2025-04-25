using AutoMapper;
using Backend.Common;
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

    public FeesRepository(DatabaseContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<Result<List<GetFeeDTO>>> GetAllAsync()
    {
        try
        {
            var fees = await _db.Fees.ToListAsync();
            var feesDTO = _mapper.Map<List<GetFeeDTO>>(fees);
            return Result<List<GetFeeDTO>>.Success(feesDTO);
        }
        catch (Exception ex)
        {
            return Result<List<GetFeeDTO>>.Fail($"Error fetching fees: {ex.Message}");
        }
    }

    public async Task<Result<GetFeeDTO>> GetByIdAsync(int id)
    {
        try
        {
            var fee = await _db.Fees
                               .Include(f => f.FeeClasses)
                               .FirstOrDefaultAsync(f => f.FeeID == id);

            if (fee is null)
                return Result<GetFeeDTO>.Fail("Fee not found.");

            var feeDTO = _mapper.Map<GetFeeDTO>(fee);
            return Result<GetFeeDTO>.Success(feeDTO);
        }
        catch (Exception ex)
        {
            return Result<GetFeeDTO>.Fail($"Error retrieving fee: {ex.Message}");
        }
    }

    public async Task<Result<bool>> AddAsync(FeeDTO fee)
    {
        try
        {
            var newFee = _mapper.Map<Fee>(fee);
            await _db.Fees.AddAsync(newFee);
            await _db.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail($"Error adding fee: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateAsync(FeeDTO fee)
    {
        try
        {
            var existingFee = await _db.Fees.FirstOrDefaultAsync(f => f.FeeID == fee.FeeID);
            if (existingFee is null)
                return Result<bool>.Fail("Fee not found.");

            existingFee.FeeName = fee.FeeName;
            existingFee.Note = fee.Note;
            existingFee.FeeNameAlis = fee.FeeNameAlis;

            _db.Entry(existingFee).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail($"Error updating fee: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        try
        {
            var fee = await _db.Fees.FindAsync(id);
            if (fee is null)
                return Result<bool>.Fail("Fee not found.");

            _db.Fees.Remove(fee);
            await _db.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail($"Error deleting fee: {ex.Message}");
        }
    }
}
