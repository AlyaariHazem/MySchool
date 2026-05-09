using Backend.DTOS.School.Analytics;

namespace Backend.Interfaces;

public interface IAnalyticsService
{
    Task<IReadOnlyList<KpiDefinitionListDto>> GetKpiDefinitionsAsync(
        AnalyticsListQueryDto query,
        CancellationToken cancellationToken = default);

    Task<AnalyticsDashboardResultDto> GetKpiSnapshotsDashboardAsync(
        AnalyticsListQueryDto query,
        CancellationToken cancellationToken = default);

    Task<AnalyticsGenerationResultDto> GenerateSnapshotsAsync(
        AnalyticsGenerateRequestDto request,
        CancellationToken cancellationToken = default);

    Task<AnalyticsDashboardResultDto> GetExecutiveDashboardAsync(
        AnalyticsDashboardQueryDto query,
        CancellationToken cancellationToken = default);

    Task<AnalyticsDashboardResultDto> GetEducationalSupervisorDashboardAsync(
        AnalyticsDashboardQueryDto query,
        CancellationToken cancellationToken = default);

    Task<AnalyticsDashboardResultDto> GetAdministrativeSupervisorDashboardAsync(
        AnalyticsDashboardQueryDto query,
        CancellationToken cancellationToken = default);

    Task<AnalyticsDashboardResultDto> GetEmployeeDashboardAsync(
        int employeeProfileId,
        AnalyticsDashboardQueryDto query,
        CancellationToken cancellationToken = default);

    Task<AnalyticsDashboardResultDto> GetSchoolDashboardAsync(
        AnalyticsDashboardQueryDto query,
        CancellationToken cancellationToken = default);

    Task<YearComparisonResultDto> GetYearOverYearComparisonAsync(
        YearComparisonQueryDto query,
        CancellationToken cancellationToken = default);
}
