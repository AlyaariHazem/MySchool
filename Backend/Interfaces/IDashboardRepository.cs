using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.DTOS.Dashboard;

namespace Backend.Interfaces;

public interface IDashboardRepository
{
    Task<DashboardSummaryDTO> GetDashboardSummaryAsync();
    Task<List<RecentExamDTO>> GetRecentExamsAsync();
    Task<List<RecentExamDTO>> GetAllExamsAsync();
    Task<List<StudentEnrollmentTrendDTO>> GetStudentEnrollmentTrendAsync();
    Task<TeacherWorkspaceDTO> GetTeacherWorkspaceAsync(int teacherId);

    /// <summary>Tenant-wide teaching snapshot for managers/admins (no teacher row required).</summary>
    Task<TeacherWorkspaceDTO> GetSchoolTeachingWorkspaceAsync();
}

