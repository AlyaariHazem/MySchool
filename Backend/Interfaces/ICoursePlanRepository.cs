
using Backend.DTOS.School.CoursePlan;

namespace Backend.Repository.School.Implements;

public interface ICoursePlanRepository
{
    Task<CoursePlanDTO> AddAsync(CoursePlanDTO dto);
    Task<List<CoursePlanReturnDTO>> GetAllAsync();
    Task<List<SubjectCoursePlanDTO>> GetAllSubjectsAsync();
    Task<CoursePlanDTO?> GetByIdAsync(int id);
    Task UpdateAsync(CoursePlanDTO dto);
    Task DeleteAsync(int id);
}
