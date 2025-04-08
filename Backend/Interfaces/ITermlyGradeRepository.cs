using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.TermlyGrade;

namespace Backend.Interfaces;

public interface ITermlyGradeRepository
{
    Task<TermlyGradeDTO> AddAsync(TermlyGradeDTO termlyGrade);
    Task<List<TermlyGradeDTO>> GetAllAsync(int termId, int classId);
    Task<bool> UpdateAsync(TermlyGradeDTO termlyGrade);
    Task<bool> DeleteAsync(int id);
}
