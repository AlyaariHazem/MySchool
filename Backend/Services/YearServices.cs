using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Backend.DTOS.yearsDTO;

using Backend.Models;
using Backend.Repository.School.Interfaces;
using Backend.Services.IServices;

namespace Backend.Services
{
    public class YearServices : IYearServices
    {
        private readonly IYearRepository _yearRepository;

        public YearServices(IYearRepository yearRepository)
        {
            _yearRepository = yearRepository;
        }

        public async Task<bool> AddAsync(YearDTO obj)
        {
            var newYear = new Year
            {
                YearDateStart = obj.YearDateStart.ToDateTime(TimeOnly.MinValue),
                YearDateEnd = obj.YearDateEnd.ToDateTime(TimeOnly.MinValue),
                HireDate = obj.HireDate.ToDateTime(TimeOnly.MinValue),
            };

            await _yearRepository.CreateAsync(newYear);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var year = await _yearRepository.GetAsync(x => x.YearID == id);
            if (year == null)
            {
                return false;
            }
            await _yearRepository.RemoveAsync(year);
            return true;
        }

        public async Task<List<YearDTO>> GetAllAsync(Expression<Func<Year, bool>> filter = null)
        {
            var years = await _yearRepository.GetAllAsync(filter);
            var yearDTOs = years.Select(x => new YearDTO
            {
                YearID = x.YearID,
                YearDateStart = DateOnly.Parse(x.YearDateStart.ToString()),
                YearDateEnd = DateOnly.Parse(x.YearDateEnd.ToString()),
                HireDate = DateOnly.Parse(x.HireDate.ToString())
            }).ToList();
            return yearDTOs;
        }

        public async Task<YearDTO> GetAsync(Expression<Func<Year, bool>> filter)
        {
            var year = await _yearRepository.GetAsync(filter);
            var yearDTO = new YearDTO
            {
                YearID = year.YearID,
                YearDateStart = DateOnly.Parse(year.YearDateStart.ToString()),
                YearDateEnd = DateOnly.Parse(year.YearDateEnd.ToString()),
                HireDate = DateOnly.Parse(year.HireDate.ToString())
            };
            return yearDTO;
        }

        public async Task<bool> UpdateAsync(YearDTO obj)
        {
            var existingYear = await _yearRepository.GetAsync(x => x.YearID == obj.YearID);
            if (existingYear == null)
            {
                return false;
            }
            existingYear.YearDateStart = obj.YearDateStart.ToDateTime(TimeOnly.MinValue);
            existingYear.YearDateEnd = obj.YearDateEnd.ToDateTime(TimeOnly.MinValue);
            existingYear.HireDate = obj.HireDate.ToDateTime(TimeOnly.MinValue);
            await _yearRepository.UpdateAsync(existingYear);
            return true;

        }

    }
}


