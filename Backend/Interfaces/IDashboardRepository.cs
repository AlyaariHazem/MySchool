using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.DTOS.Dashboard;

namespace Backend.Interfaces;

public interface IDashboardRepository
{
    Task<DashboardSummaryDTO> GetDashboardSummaryAsync();
    Task<List<RecentExamDTO>> GetRecentExamsAsync();
    Task<List<StudentEnrollmentTrendDTO>> GetStudentEnrollmentTrendAsync();
}

