using Backend.DTOS.School.OrganizationalPlan;

namespace Backend.Interfaces;

public interface IOrganizationalPlanRepository
{
    Task<IReadOnlyList<StrategicGoalListItemDto>> ListStrategicGoalsAsync(StrategicGoalFilterDto filter, CancellationToken cancellationToken = default);
    Task<StrategicGoalDetailDto?> GetStrategicGoalByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateStrategicGoalAsync(StrategicGoalWriteDto dto, CancellationToken cancellationToken = default);
    Task UpdateStrategicGoalAsync(int id, StrategicGoalWriteDto dto, CancellationToken cancellationToken = default);
    Task<int?> GetSchoolIdForStrategicGoalAsync(int strategicGoalId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AnnualGoalListItemDto>> ListAnnualGoalsAsync(AnnualGoalFilterDto filter, CancellationToken cancellationToken = default);
    Task<AnnualGoalDetailDto?> GetAnnualGoalByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAnnualGoalAsync(AnnualGoalWriteDto dto, CancellationToken cancellationToken = default);
    Task UpdateAnnualGoalAsync(int id, AnnualGoalWriteDto dto, CancellationToken cancellationToken = default);
    Task<int?> GetSchoolIdForAnnualGoalAsync(int annualGoalId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DepartmentGoalListItemDto>> ListDepartmentGoalsAsync(DepartmentGoalFilterDto filter, CancellationToken cancellationToken = default);
    Task<DepartmentGoalDetailDto?> GetDepartmentGoalByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateDepartmentGoalAsync(DepartmentGoalWriteDto dto, CancellationToken cancellationToken = default);
    Task UpdateDepartmentGoalAsync(int id, DepartmentGoalWriteDto dto, CancellationToken cancellationToken = default);
    Task<int?> GetSchoolIdForDepartmentGoalAsync(int departmentGoalId, CancellationToken cancellationToken = default);
}
