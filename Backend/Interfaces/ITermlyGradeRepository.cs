using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Common;
using Backend.DTOS.School.TermlyGrade;

namespace Backend.Interfaces;

public interface ITermlyGradeRepository
{
    Task<Result<TermlyGradeDTO>> AddAsync(TermlyGradeDTO termlyGrade);
    Task<Result<List<TermlyGradesReturnDTO>>> GetAllAsync(int term, int yearId, int classId, int subjectId, int pageNumber, int pageSize);
    Task<Result<TermlyGradeDTO>> GetByIdAsync(int id);
    Task<int> GetTotalMonthlyGradesCountAsync(int term, int yearId, int classId, int subjectId);
    Task<Result<bool>> UpdateAsync(IEnumerable<TermlyGradeDTO> termlyGrade);
    Task<Result<bool>> DeleteAsync(int id);
}
