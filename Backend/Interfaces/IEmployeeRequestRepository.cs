using Backend.DTOS.School.EmployeeRequest;

namespace Backend.Interfaces;

public interface IEmployeeRequestRepository
{
    Task<IReadOnlyList<EmployeeRequestTypeListItemDto>> ListTypesAsync(int schoolId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmployeeRequestListItemDto>> ListAsync(EmployeeRequestFilterDto filter, CancellationToken cancellationToken = default);

    Task<EmployeeRequestDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<int> CreateAsync(EmployeeRequestWriteDto dto, CancellationToken cancellationToken = default);

    Task UpdateAsync(int id, EmployeeRequestWriteDto dto, CancellationToken cancellationToken = default);

    Task<int> AddExecutionAsync(int employeeRequestId, EmployeeRequestExecutionWriteDto dto, CancellationToken cancellationToken = default);

    Task<int> AddDailySummaryAsync(int employeeRequestId, EmployeeRequestDailySummaryWriteDto dto, CancellationToken cancellationToken = default);

    Task<int> AddApprovalStepAsync(int employeeRequestId, EmployeeRequestApprovalStepWriteDto dto, CancellationToken cancellationToken = default);

    Task DecideApprovalStepAsync(int employeeRequestId, int stepId, EmployeeRequestApprovalDecideDto dto, CancellationToken cancellationToken = default);

    Task<int?> GetSchoolIdForRequestAsync(int employeeRequestId, CancellationToken cancellationToken = default);
}
