using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Common;
using Backend.DTOS.School.MonthlyGrade;

namespace Backend.Interfaces;

public interface IMonthlyGradeRepository
{
    Task<int?> GetGuardianIdByUserIdAsync(string userId);

    Task<List<GuardianMonthlyGradeRowDto>> GetGuardianStudentsMonthlyGradesAsync(int guardianId, int? yearId, int? termId, int? monthId);

    Task<Result<MonthlyGradeDTO>> AddAsync(MonthlyGradeDTO dto);
    Task<Result<List<MonthlyGradesReternDTO>>> GetAllAsync(MonthlyGradesQueryDTO query);
    Task<Result<bool>> UpdateManyAsync(IEnumerable<MonthlyGradeDTO> dtos);
    Task<Result<bool>> DeleteAsync(int id);
    Task<int> GetTotalMonthlyGradesCountAsync(MonthlyGradesQueryDTO query);
}
