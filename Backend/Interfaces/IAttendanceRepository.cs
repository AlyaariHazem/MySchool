using Backend.DTOS.School.Attendance;

namespace Backend.Interfaces;

public interface IAttendanceRepository
{
    Task<int?> GetGuardianIdByUserIdAsync(string userId);

    Task<AttendanceDTO?> GetByIdAsync(Guid id);
    Task<List<AttendanceDTO>> GetByClassAndDateAsync(int classId, DateOnly date);
    Task<List<AttendanceDTO>> GetByStudentAsync(int studentId, DateOnly? from = null, DateOnly? to = null);
    Task<List<AttendanceDTO>> GetGuardianStudentsAttendanceAsync(int guardianId, DateOnly? from = null, DateOnly? to = null);
    Task<AttendanceDTO> CreateAsync(CreateAttendanceDTO dto, string recordedByUserId, int? tenantId);
    Task<AttendanceDTO?> UpdateAsync(Guid id, UpdateAttendanceDTO dto);
    Task<bool> DeleteAsync(Guid id);
    Task<int> BulkUpsertAsync(BulkAttendanceDTO dto, string recordedByUserId, int? tenantId);
}
