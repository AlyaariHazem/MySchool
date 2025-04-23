using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.MonthlyGrade;

namespace Backend.Interfaces;

public interface IMonthlyGradeRepository
{
    Task<MonthlyGradeDTO> AddAsync(MonthlyGradeDTO monthlyGrade);
    Task<List<MonthlyGradesReternDTO>> GetAllAsync(int Trem, int monthId, int classId, int subjectId);
    
    Task<bool> UpdateManyAsync(IEnumerable<MonthlyGradeDTO> monthlyGrades);
    Task<bool> DeleteAsync(int id);
}
