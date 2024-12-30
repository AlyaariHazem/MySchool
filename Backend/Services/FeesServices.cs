using System.Linq.Expressions;
using AutoMapper;
using Backend.Data;
using Backend.DTOS.FeesDTO;
using Backend.Models;
using Backend.Repository.IRepository;
using Backend.Repository.School.Interfaces;
using Backend.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class FeesServices : IFeesServices
    {

        private readonly IFeesRepository _feesRepository;
        public FeesServices(IFeesRepository feesRepository)
        {

            _feesRepository = feesRepository;
        }

        public async Task<List<FeeDTO>> GetAllAsync(Expression<Func<Fee, bool>> filter = null)
        {
            var Listfees = await _feesRepository.GetAllAsync(filter);
            var ListfeesDTO = new List<FeeDTO>();
            foreach (var fee in Listfees)
            {
                var feeDTO = new FeeDTO
                {
                    FeeID = fee.FeeID,
                    FeeName = fee.FeeName,
                    Note = fee.Note,
                    FeeNameAlis = fee.FeeNameAlis,
                    State = fee.State,

                };
                ListfeesDTO.Add(feeDTO);
            }
            return ListfeesDTO;
        }

        public async Task<FeeDTO> GetAsync(Expression<Func<Fee, bool>> filter)
        {
            var fee = await _feesRepository.GetAsync(filter);
            if (fee == null)
            {
                return null;
            }
            var feeDTO = new FeeDTO()
            {
                FeeID = fee.FeeID,
                FeeName = fee.FeeName,
                Note = fee.Note,
                FeeNameAlis = fee.FeeNameAlis,
                State = fee.State
            };
            return feeDTO;
        }

        public async Task<bool> AddAsync(AddFeeDTO fee)
        {
            var NewFee = new Fee()
            {
                FeeName = fee.FeeName,
                Note = fee.Note,
                FeeNameAlis = fee.FeeNameAlis,

            };
            await _feesRepository.CreateAsync(NewFee);
            return true;

        }

        public async Task<bool> UpdateAsync(UpdateFeeDTO fee)
        {
            var existingFee = await _feesRepository.GetAsync(f => f.FeeID == fee.FeeID, tracked: false);
            if (existingFee != null)
            {
                existingFee.FeeName = fee.FeeName;
                existingFee.Note = fee.Note;
                existingFee.FeeNameAlis = fee.FeeNameAlis;

                await _feesRepository.UpdateAsync(existingFee);
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var fee = await _feesRepository.GetAsync(f => f.FeeID == id);
            if (fee != null)
            {
                await _feesRepository.RemoveAsync(fee);
                return true;
            }
            return false;
        }

    }

}

