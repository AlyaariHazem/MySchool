using Backend.DTOS.School.Employees;

namespace Backend.Interfaces;

public interface IEmployeeProfileService
{
    Task<EmployeeProfileReadDto> CreateAsync(EmployeeProfileCreateDto dto, CancellationToken cancellationToken = default);
    Task<EmployeeProfileReadDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployeeProfileReadDto>> GetAllAsync(EmployeeProfileListFilterDto? filter, CancellationToken cancellationToken = default);
    Task<EmployeeProfileReadDto> UpdateAsync(int id, EmployeeProfileUpdateDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeactivateAsync(int id, CancellationToken cancellationToken = default);
    Task<EmployeeProfileFullDto> GetFullProfileAsync(int id, CancellationToken cancellationToken = default);

    Task<EmployeeQualificationDto> AddQualificationAsync(int employeeProfileId, EmployeeQualificationDto dto, CancellationToken cancellationToken = default);
    Task<EmployeeSpecializationDto> AddSpecializationAsync(int employeeProfileId, EmployeeSpecializationDto dto, CancellationToken cancellationToken = default);
    Task<EmployeeHistoryDto> AddHistoryAsync(int employeeProfileId, EmployeeHistoryDto dto, CancellationToken cancellationToken = default);
    Task<EmployeeDocumentDto> AddDocumentAsync(int employeeProfileId, EmployeeDocumentDto dto, CancellationToken cancellationToken = default);
    Task<EmployeeLeaveDto> AddLeaveAsync(int employeeProfileId, EmployeeLeaveDto dto, CancellationToken cancellationToken = default);
    Task<EmployeePerformanceSummaryDto> AddPerformanceSummaryAsync(int employeeProfileId, EmployeePerformanceSummaryDto dto, CancellationToken cancellationToken = default);
}
