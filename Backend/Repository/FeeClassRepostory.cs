using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS.School.Fees;
using Backend.Models;
using Backend.Repository.School.Implements;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School.Classes;

public class FeeClassRepostory:IFeeClassRepository
{
      private readonly DatabaseContext _db;
    private readonly IMapper _mapper;

        public FeeClassRepostory(DatabaseContext db,IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<FeeClassDTO>> GetAllAsync()
        {
            var ListFeeClass= await _db.FeeClass
                .Include(fc => fc.Class) // Include related Class
                .Include(fc => fc.Fee).Select(fc=>new FeeClassDTO{
                    FeeClassID=fc.FeeClassID,
                    ClassID=fc.ClassID,
                    FeeID=fc.FeeID,
                    Amount=fc.Amount,
                    Mandatory=fc.Mandatory,
                    ClassYear=fc.Class.Year.YearDateStart.ToString("yyyy-MM-dd"),
                    ClassName=fc.Class.ClassName,
                    FeeName=fc.Fee.FeeName
                })  // Include related Fee
                .ToListAsync();
            var FeeClassDTO=_mapper.Map<List<FeeClassDTO>>(ListFeeClass);
            return FeeClassDTO;
                
        }

        public async Task<FeeClassDTO> GetByIdAsync(int feeClassID)
        {
            var FeeClass= await _db.FeeClass
                .Include(fc => fc.Class) // Include related Class
                .Include(fc => fc.Fee)  // Include related Fee
                .FirstOrDefaultAsync(fc => fc.FeeClassID == feeClassID);
            var FeeClassDTO=_mapper.Map<FeeClassDTO>(FeeClass);
            if (FeeClassDTO == null)
            {
                return null!;
            }
            return FeeClassDTO;
        }

        public async Task<List<FeeClassDTO>> GetAllByClassIdAsync(int classId)
        {
            var FeeClass= await _db.FeeClass
                .Include(fc => fc.Class)
                .Include(fc => fc.Fee)
                .Where(fc => fc.ClassID == classId)
                .ToListAsync();
            var FeeClassDTO=_mapper.Map<List<FeeClassDTO>>(FeeClass);
            if (FeeClassDTO == null)
            {
                return null!;
            }
            return FeeClassDTO;
        }
        
        public async Task<bool> checkIfExist(int feeClassID){
            var FeeClass= await _db.FeeClass.FirstOrDefaultAsync(fc => fc.FeeClassID==feeClassID);
            if(FeeClass == null)
            return false;
        return true;
        }

        public async Task AddAsync(AddFeeClassDTO feeClass)
        {
            var NewFeeClass=_mapper.Map<FeeClass>(feeClass);
            await _db.FeeClass.AddAsync(NewFeeClass);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(int feeClassID,AddFeeClassDTO feeClass)
        {
            var existingFeeClass = await _db.FeeClass
                .FirstOrDefaultAsync(fc => fc.FeeClassID==feeClassID);

            if (existingFeeClass != null)
            {
                existingFeeClass.FeeID = feeClass.FeeID;
                existingFeeClass.ClassID = feeClass.ClassID;
                existingFeeClass.Amount = feeClass.Amount;
                existingFeeClass.Mandatory = feeClass.Mandatory;
                _db.Entry(existingFeeClass).State = EntityState.Modified;
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int feeClassID)
        {
            var feeClass = await _db.FeeClass
                .FirstOrDefaultAsync(fc => fc.FeeClassID == feeClassID);

            if (feeClass != null)
            {
                _db.FeeClass.Remove(feeClass);
                await _db.SaveChangesAsync();
            }
        }
}
