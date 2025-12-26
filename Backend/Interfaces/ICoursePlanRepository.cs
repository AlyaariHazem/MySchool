
using Backend.DTOS.School.CoursePlan;

namespace Backend.Repository.School.Implements;

public interface ICoursePlanRepository
{
    Task<CoursePlanDTO> AddAsync(CoursePlanDTO dto);
    Task<List<CoursePlanReturnDTO>> GetAllAsync();
    Task<List<SubjectCoursePlanDTO>> GetAllSubjectsAsync();
    Task<CoursePlanDTO?> GetByIdAsync(int yearID, int teacherID, int classID, int divisionID, int subjectID);
    Task UpdateAsync(CoursePlanDTO dto, int oldYearID, int oldTeacherID, int oldClassID, int oldDivisionID, int oldSubjectID);
    Task DeleteAsync(int yearID, int teacherID, int classID, int divisionID, int subjectID);
}
