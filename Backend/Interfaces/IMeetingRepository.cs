using Backend.DTOS.School.Meeting;

namespace Backend.Interfaces;

public interface IMeetingRepository
{
    Task<IReadOnlyList<MeetingListItemDto>> ListMeetingsAsync(MeetingFilterDto filter, CancellationToken cancellationToken = default);

    Task<MeetingDetailDto?> GetMeetingByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<int> CreateMeetingAsync(MeetingWriteDto dto, CancellationToken cancellationToken = default);

    Task UpdateMeetingAsync(int id, MeetingWriteDto dto, CancellationToken cancellationToken = default);

    Task UpsertMinutesAsync(int meetingId, MeetingMinutesWriteDto dto, CancellationToken cancellationToken = default);

    Task ReplaceTasksAsync(int meetingId, IReadOnlyList<MeetingTaskWriteDto> tasks, CancellationToken cancellationToken = default);

    Task<int?> GetSchoolIdForMeetingAsync(int meetingId, CancellationToken cancellationToken = default);

    Task<int?> GetEmployeeProfileIdForUserInSchoolAsync(string? userId, int schoolId, CancellationToken cancellationToken = default);
}
