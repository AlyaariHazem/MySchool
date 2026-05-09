using Backend.DTOS.School.Analytics;

namespace Backend.Interfaces;

public interface IAnalyticsRepository
{
    Task<AnalyticsDashboardResultDto> GetDashboardAsync(
        AnalyticsDashboardQueryDto query,
        CancellationToken cancellationToken = default);
}
