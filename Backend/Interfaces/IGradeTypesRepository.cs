using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.GradeTypes;

namespace Backend.Interfaces;

public interface IGradeTypesRepository
{
    Task<GradeTypesDTO> AddAsync(GradeTypesDTO gradeType);
    Task<List<GradeTypesDTO>> GetAllAsync();
    Task<GradeTypesDTO?> GetByIdAsync(int id);
    Task UpdateAsync(GradeTypesDTO gradeType);
    Task DeleteAsync(int id);
}
