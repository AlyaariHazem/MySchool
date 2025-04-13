using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.Curriculms;

namespace Backend.Interfaces;

public interface ICurriculumRepository
{
    Task<Boolean> AddAsync(CurriculumDTO dto);
    Task<List<CurriculumReturnDTO>> GetAllAsync();
    Task<CurriculumReturnDTO?> GetByIdAsync(int subjectId, int classId);
    Task UpdateAsync(CurriculumDTO dto);
    Task DeleteAsync(int subjectId, int classId);
}

