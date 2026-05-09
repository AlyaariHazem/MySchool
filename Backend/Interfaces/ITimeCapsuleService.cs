using Backend.DTOS.School.TimeCapsule;
using Backend.Models;

namespace Backend.Interfaces;

public interface ITimeCapsuleService
{
    Task EnsureCapsuleForEmployeeAsync(int employeeProfileId, int schoolId, CancellationToken cancellationToken = default);

    Task<TimeCapsuleStatusDto> GetCapsuleStatusAsync(int employeeProfileId, string? currentUserId, string? userType, bool isAdmin, CancellationToken cancellationToken = default);

    Task<ResignationRequestReadDto> RequestResignationAsync(ResignationRequestCreateDto dto, string currentUserId, string? userType, bool isAdmin, CancellationToken cancellationToken = default);

    Task<ResignationRequestReadDto> ApproveResignationAsync(int resignationRequestId, string approverUserId, string? userType, bool isAdmin, string? notes, CancellationToken cancellationToken = default);

    Task<ResignationRequestReadDto> RejectResignationAsync(int resignationRequestId, string approverUserId, string? userType, bool isAdmin, string? notes, CancellationToken cancellationToken = default);

    Task ApproveCapsuleUnlockAsync(int capsuleId, string approverUserId, string? userType, bool isAdmin, string? unlockReason, CancellationToken cancellationToken = default);

    Task RejectCapsuleUnlockAsync(int capsuleId, string approverUserId, string? userType, bool isAdmin, string? notes, CancellationToken cancellationToken = default);

    /// <summary>Builds immutable section snapshots and narrative; called internally when unlock is approved.</summary>
    Task GenerateCapsuleDataAsync(int timeCapsuleId, CancellationToken cancellationToken = default);

    Task<TimeCapsuleDetailDto?> GetCapsuleAsync(int employeeProfileId, string? currentUserId, string? userType, bool isAdmin, CancellationToken cancellationToken = default);

    Task LogAccessAsync(int timeCapsuleId, string userId, CapsuleAccessActionType action, string? notes, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CapsuleAccessLogReadDto>> GetAccessLogsAsync(int capsuleId, string? currentUserId, string? userType, bool isAdmin, CancellationToken cancellationToken = default);
}
