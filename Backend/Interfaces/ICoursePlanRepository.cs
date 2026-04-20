using System.Threading;
using Backend.DTOS.School.CoursePlan;
using Backend.Models;

namespace Backend.Repository.School.Implements;

public interface ICoursePlanRepository
{
    Task<CoursePlanDTO> AddAsync(CoursePlanDTO dto);
    Task<List<CoursePlanReturnDTO>> GetAllAsync();
    Task<List<SubjectCoursePlanDTO>> GetAllSubjectsAsync();
    Task<CoursePlanDTO?> GetByIdAsync(int yearID, int teacherID, int classID, int divisionID, int subjectID, int termID);
    Task UpdateAsync(CoursePlanDTO dto, int oldYearID, int oldTeacherID, int oldClassID, int oldDivisionID, int oldSubjectID, int oldTermID);
    Task DeleteAsync(int yearID, int teacherID, int classID, int divisionID, int subjectID, int termID);

    Task<List<CoursePlan>> GetByClassTermYearForSchedulingAsync(
        int yearId,
        int classId,
        int termId,
        int? divisionId,
        CancellationToken cancellationToken = default);

    Task<Dictionary<int, int>> GetTeacherCoursePlanCountsAsync(
        int yearId,
        int termId,
        CancellationToken cancellationToken = default);
}
