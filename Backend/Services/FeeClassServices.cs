using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS.FeeClassesDTO;
using Backend.Models;
using Backend.Repository.IRepository;

using Backend.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class FeeClassServices : IFeeClassServices
    {


        private readonly IFeeClassRepository _feeClassRepository;

        public FeeClassServices(IFeeClassRepository feeClassRepository)
        {

            _feeClassRepository = feeClassRepository;
        }


        public async Task<List<FeeClassDTO>> GetAllAsync(Expression<Func<FeeClass, bool>> filter = null)
        {

            var ListFeeClass = await _feeClassRepository.GetAllAsync(filter, includeProperties: "Class,Fee");
            var ListFeeClassDTO = new List<FeeClassDTO>();
            foreach (var feeClass in ListFeeClass)
            {
                var feeClassDto = new FeeClassDTO
                {
                    FeeClassID = feeClass.FeeClassID,
                    FeeID = feeClass.FeeID,
                    ClassID = feeClass.ClassID,
                    Amount = feeClass.Amount,
                    Mandatory = feeClass.Mandatory,
                    FeeName = feeClass.Fee.FeeName,
                    ClassName = feeClass.Class.ClassName,
                    FeeNameAlis = feeClass.Fee.FeeNameAlis,
                    ClassYear = feeClass.Class.ClassYear
                };
                ListFeeClassDTO.Add(feeClassDto);
            }

            return ListFeeClassDTO;


        }

        public async Task<bool> checkIfExist(int feeClassID)
        {
            var FeeClass = await _feeClassRepository.GetAsync(fc => fc.FeeClassID == feeClassID);
            if (FeeClass == null)
                return false;
            return true;
        }

        public async Task<bool> AddAsync(AddFeeClassDTO feeClass)
        {
            var NewFeeClass = new FeeClass()
            {
                FeeID = feeClass.FeeID,
                ClassID = feeClass.ClassID,
                Amount = feeClass.Amount,
                Mandatory = feeClass.Mandatory
            };
            await _feeClassRepository.CreateAsync(NewFeeClass);
            return true;
        }

        public async Task<bool> DeleteAsync(int feeClassID)
        {
            var feeClass = await _feeClassRepository.GetAsync(fc => fc.FeeClassID == feeClassID);

            if (feeClass != null)
            {
                await _feeClassRepository.RemoveAsync(feeClass);
                return true;
            }
            return false;
        }



        public async Task<FeeClassDTO> GetAsync(Expression<Func<FeeClass, bool>> filter)
        {
            var FeeClass = await _feeClassRepository.GetAsync(filter, includeProperties: "Class,Fee");
            if (FeeClass == null)
            {
                return null;
            }
            var FeeClassDTO = new FeeClassDTO()
            {
                FeeClassID = FeeClass.FeeClassID,
                FeeID = FeeClass.FeeID,
                ClassID = FeeClass.ClassID,
                Amount = FeeClass.Amount,
                Mandatory = FeeClass.Mandatory,
                FeeName = FeeClass.Fee.FeeName,
                ClassName = FeeClass.Class.ClassName,
                FeeNameAlis = FeeClass.Fee.FeeNameAlis,
                ClassYear = FeeClass.Class.ClassYear
            };
            return FeeClassDTO;
        }



        public async Task<bool> UpdateAsync(int feeClassID, UpdateFeeClassDTO feeClass)
        {
            var existingFeeClass = await _feeClassRepository.GetAsync(fc => fc.FeeClassID == feeClassID);
            if (existingFeeClass != null)
            {
                existingFeeClass.FeeID = feeClass.FeeID;
                existingFeeClass.ClassID = feeClass.ClassID;
                existingFeeClass.Amount = feeClass.Amount;
                existingFeeClass.Mandatory = feeClass.Mandatory;
                await _feeClassRepository.UpdateAsync(existingFeeClass);
                return true;

            }
            return false;
        }



    }

}

