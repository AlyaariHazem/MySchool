using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Common;
using Backend.DTOS.School.TermlyGrade;

namespace Backend.Interfaces;

public interface ITermlyGradeRepository
{
    Task<Result<TermlyGradeDTO>> AddAsync(TermlyGradeDTO termlyGrade);

    /// <summary>Rows are scoped to the active academic year.</summary>
    Task<Result<List<TermlyGradesReturnDTO>>> GetAllAsync(int term, int classId, int subjectId, int pageNumber, int pageSize);

    Task<Result<List<TermlyGradesReturnDTO>>> GetAllAsync(TermlyGradeQueryDTO query);

    Task<Result<TermlyGradeDTO>> GetByIdAsync(int id);

    /// <summary>Total rows after grouping by student + subject (matches paged list), not distinct students only.</summary>
    Task<int> GetTotalTermlyGradesCountAsync(int term, int classId, int subjectId);

    Task<int> GetTotalTermlyGradesCountAsync(TermlyGradeQueryDTO query);

    Task<Result<bool>> UpdateAsync(IEnumerable<TermlyGradeDTO> termlyGrade);
    Task<Result<bool>> DeleteAsync(int id);
}
