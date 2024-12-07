using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School;

namespace Backend.Repository.School.Implements;

public interface ISchoolRepository
{
        Task AddAsync(SchoolDTO school);
        Task<List<SchoolDTO>> GetByIdAsync();
        Task UpdateAsync(SchoolDTO school);
        Task DeleteAsync(int schoolId);
}
