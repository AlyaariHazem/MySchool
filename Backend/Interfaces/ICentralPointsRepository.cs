using Backend.DTOS.School.CentralPoints;

namespace Backend.Interfaces;

public interface ICentralPointsRepository
{
    Task EnsureDefaultSourcesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PointsSourceDto>> ListSourcesAsync(bool activeOnly, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PointsRuleDto>> ListRulesAsync(PointsRuleFilterDto filter, CancellationToken cancellationToken = default);

    Task<int> CreateRuleAsync(PointsRuleWriteDto dto, CancellationToken cancellationToken = default);

    Task UpdateRuleAsync(int ruleId, PointsRuleWriteDto dto, int? managerSchoolIdOnly, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<PointsLedgerListItemDto> Items, int TotalCount)> ListLedgerAsync(
        PointsLedgerFilterDto filter,
        CancellationToken cancellationToken = default);

    Task<PointsBalanceDto?> GetBalanceAsync(int employeeProfileId, int schoolId, CancellationToken cancellationToken = default);

    Task<PostCentralPointsResultDto> PostAsync(PostCentralPointsDto dto, int? postedByEmployeeProfileId, CancellationToken cancellationToken = default);

    Task<int> RebuildBalanceSnapshotAsync(int employeeProfileId, int schoolId, CancellationToken cancellationToken = default);
}
