using AutoMapper;
using Backend.Common;
using Backend.Data;
using Backend.DTOS.School.FeeClass;
using Backend.DTOS.School.Fees;
using Backend.Models;
using Backend.Repository.School.Implements;
using Backend.Repository.School.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School.Classes;

public class FeeClassRepository : IFeeClassRepository
{
    private readonly TenantDbContext _db;
    private readonly IMapper _mapper;

    public FeeClassRepository(TenantDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    /*-----------------------------------------*/
    /*               READ METHODS              */
    /*-----------------------------------------*/

    public async Task<Result<List<FeeClassDTO>>> GetAllAsync()
    {
        try
        {
            var list = await _db.FeeClass
                                .Include(fc => fc.Class)
                                .Include(fc => fc.Fee)
                                .Select(fc => new FeeClassDTO
                                {
                                    FeeClassID = fc.FeeClassID,
                                    ClassID    = fc.ClassID,
                                    FeeID      = fc.FeeID,
                                    Amount     = fc.Amount,
                                    Mandatory  = fc.Mandatory,
                                    ClassYear  = fc.Class.Year.YearDateStart.ToString("yyyy-MM-dd"),
                                    ClassName  = fc.Class.ClassName,
                                    FeeName    = fc.Fee.FeeName
                                })
                                .ToListAsync();

            return Result<List<FeeClassDTO>>.Success(list);
        }
        catch (Exception ex)
        {
            return Result<List<FeeClassDTO>>.Fail($"Error fetching fee-classes: {ex.Message}");
        }
    }

    public async Task<Result<FeeClassDTO>> GetByIdAsync(int feeClassID)
    {
        try
        {
            var entity = await _db.FeeClass
                                  .Include(fc => fc.Class)
                                  .Include(fc => fc.Fee)
                                  .FirstOrDefaultAsync(fc => fc.FeeClassID == feeClassID);

            if (entity is null)
                return Result<FeeClassDTO>.Fail("Fee-class not found.");

            var dto = _mapper.Map<FeeClassDTO>(entity);
            return Result<FeeClassDTO>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<FeeClassDTO>.Fail($"Error retrieving fee-class: {ex.Message}");
        }
    }

    public async Task<Result<List<FeeClassDTO>>> GetAllByClassIdAsync(int classId)
    {
        try
        {
            var list = await _db.FeeClass
                                .Include(fc => fc.Class)
                                .Include(fc => fc.Fee)
                                .Where(fc => fc.ClassID == classId)
                                .Select(fc => _mapper.Map<FeeClassDTO>(fc))
                                .ToListAsync();

            return Result<List<FeeClassDTO>>.Success(list);
        }
        catch (Exception ex)
        {
            return Result<List<FeeClassDTO>>.Fail($"Error fetching fee-classes: {ex.Message}");
        }
    }

    public async Task<Result<bool>> AddAsync(AddFeeClassDTO feeClass)
    {
        try
        {
            var entity = _mapper.Map<FeeClass>(feeClass);
            await _db.FeeClass.AddAsync(entity);
            await _db.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail($"Error adding fee-class: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateAsync(int feeClassID, AddFeeClassDTO feeClass)
    {
        try
        {
            var existing = await _db.FeeClass.FirstOrDefaultAsync(fc => fc.FeeClassID == feeClassID);
            if (existing is null)
                return Result<bool>.Fail("Fee-class not found.");

            existing.FeeID     = feeClass.FeeID;
            existing.ClassID   = feeClass.ClassID;
            existing.Amount    = feeClass.Amount;
            existing.Mandatory = feeClass.Mandatory;

            _db.Entry(existing).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail($"Error updating fee-class: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteAsync(int feeClassID)
    {
        try
        {
            var entity = await _db.FeeClass.FirstOrDefaultAsync(fc => fc.FeeClassID == feeClassID);
            if (entity is null)
                return Result<bool>.Fail("Fee-class not found.");

            _db.FeeClass.Remove(entity);
            await _db.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail($"Error deleting fee-class: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdatePartial(int id, JsonPatchDocument<ChangeStateFeeClassDTO> partialClass)
    {
        var existingClass = await _db.FeeClass.FirstOrDefaultAsync(c => c.FeeClassID == id);
        if (existingClass != null)
        {
            var classToUpdate = _mapper.Map<ChangeStateFeeClassDTO>(existingClass);

            partialClass.ApplyTo(classToUpdate);
            _mapper.Map(classToUpdate, existingClass);
            _db.Entry(existingClass).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
        return Result<bool>.Fail("Fee-class not found.");
    }
}
