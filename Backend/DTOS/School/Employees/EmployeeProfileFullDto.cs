namespace Backend.DTOS.School.Employees;

public class EmployeeProfileFullDto
{
    public EmployeeProfileReadDto Profile { get; set; } = null!;
    public IReadOnlyList<EmployeeQualificationDto> Qualifications { get; set; } = Array.Empty<EmployeeQualificationDto>();
    public IReadOnlyList<EmployeeSpecializationDto> Specializations { get; set; } = Array.Empty<EmployeeSpecializationDto>();
    public IReadOnlyList<EmployeeHistoryDto> HistoryRecords { get; set; } = Array.Empty<EmployeeHistoryDto>();
    public IReadOnlyList<EmployeeDocumentDto> Documents { get; set; } = Array.Empty<EmployeeDocumentDto>();
    public IReadOnlyList<EmployeeLeaveDto> Leaves { get; set; } = Array.Empty<EmployeeLeaveDto>();
    public IReadOnlyList<EmployeePerformanceSummaryDto> PerformanceSummaries { get; set; } = Array.Empty<EmployeePerformanceSummaryDto>();
}
