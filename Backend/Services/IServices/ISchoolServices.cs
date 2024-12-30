using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Backend.DTOS.SchoolsDTO;
using Backend.Models;

namespace Backend.Services.IServices
{
        public interface ISchoolServices
        {
                Task<bool> AddAsync(SchoolDTO school);
                Task<SchoolDTO> GetAsync(Expression<Func<School, bool>> filter);
                Task<List<SchoolDTO>> GetAllAsync(Expression<Func<School, bool>> filter = null);
                Task<bool> UpdateAsync(SchoolDTO school);
                Task<bool> DeleteAsync(int schoolId);
        }

}

