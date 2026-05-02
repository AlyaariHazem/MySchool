using Backend.Data;
using Backend.DTOS.School.Meeting;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School;

public class MeetingRepository : IMeetingRepository
{
    private readonly TenantDbContext _db;

    public MeetingRepository(TenantDbContext db)
    {
        _db = db;
    }

    private static string FormatPersonName(Name? n)
    {
        if (n == null) return string.Empty;
        return string.Join(" ", new[] { n.FirstName, n.MiddleName, n.LastName }
            .Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
    }

    private async Task<int?> GetActiveYearIdForSchoolAsync(int schoolId, CancellationToken cancellationToken = default)
    {
        var yid = await _db.Years.AsNoTracking()
            .Where(y => y.SchoolID == schoolId && y.Active)
            .OrderBy(y => y.YearID)
            .Select(y => (int?)y.YearID)
            .FirstOrDefaultAsync(cancellationToken);
        if (yid is > 0)
            return yid;
        return await _db.Years.AsNoTracking()
            .Where(y => y.SchoolID == schoolId)
            .OrderByDescending(y => y.YearID)
            .Select(y => (int?)y.YearID)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<int?> GetEmployeeProfileIdForUserInSchoolAsync(string? userId, int schoolId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            return Task.FromResult<int?>(null);
        return _db.EmployeeProfiles.AsNoTracking()
            .Where(e => e.UserId == userId && e.SchoolID == schoolId && e.IsActive)
            .OrderBy(e => e.EmployeeProfileID)
            .Select(e => (int?)e.EmployeeProfileID)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MeetingListItemDto>> ListMeetingsAsync(MeetingFilterDto filter, CancellationToken cancellationToken = default)
    {
        filter ??= new MeetingFilterDto();
        var q = _db.Meetings.AsNoTracking().AsQueryable();

        if (filter.SchoolID is > 0)
        {
            q = q.Where(m => m.SchoolID == filter.SchoolID);
            if (filter.AcademicYearID is not > 0)
            {
                var activeYear = await GetActiveYearIdForSchoolAsync(filter.SchoolID.Value, cancellationToken);
                if (activeYear is > 0)
                    q = q.Where(m => m.AcademicYearID == activeYear.Value);
            }
        }

        if (filter.AcademicYearID is > 0)
            q = q.Where(m => m.AcademicYearID == filter.AcademicYearID);
        if (filter.Status is >= 0)
            q = q.Where(m => (int)m.Status == filter.Status);

        var raw = await q
            .OrderByDescending(m => m.StartAtUtc)
            .ThenByDescending(m => m.MeetingID)
            .Select(m => new
            {
                m.MeetingID,
                m.SchoolID,
                m.AcademicYearID,
                m.Title,
                St = (int)m.Status,
                m.StartAtUtc,
                m.EndAtUtc,
                m.OrganizerEmployeeProfileID,
                OFirst = m.OrganizerEmployeeProfile.FullName.FirstName,
                OMid = m.OrganizerEmployeeProfile.FullName.MiddleName,
                OLast = m.OrganizerEmployeeProfile.FullName.LastName,
                m.CreatedAtUtc,
                m.UpdatedAtUtc,
                AttendeeCount = m.Attendees.Count,
            })
            .ToListAsync(cancellationToken);

        return raw.Select(m => new MeetingListItemDto
        {
            MeetingID = m.MeetingID,
            SchoolID = m.SchoolID,
            AcademicYearID = m.AcademicYearID,
            Title = m.Title,
            Status = m.St,
            StartAtUtc = m.StartAtUtc,
            EndAtUtc = m.EndAtUtc,
            OrganizerEmployeeProfileID = m.OrganizerEmployeeProfileID,
            OrganizerName = FormatPersonName(new Name { FirstName = m.OFirst, MiddleName = m.OMid, LastName = m.OLast }),
            CreatedAtUtc = m.CreatedAtUtc,
            UpdatedAtUtc = m.UpdatedAtUtc,
            AttendeeCount = m.AttendeeCount,
        }).ToList();
    }

    public async Task<MeetingDetailDto?> GetMeetingByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var m = await _db.Meetings.AsNoTracking()
            .Include(x => x.OrganizerEmployeeProfile)
            .Include(x => x.Attendees).ThenInclude(a => a.EmployeeProfile)
            .Include(x => x.Minutes)!.ThenInclude(mm => mm!.RecordedByEmployeeProfile)
            .Include(x => x.Minutes)!.ThenInclude(mm => mm!.ApprovedByEmployeeProfile)
            .Include(x => x.Tasks).ThenInclude(t => t.AssignedToEmployeeProfile)
            .Include(x => x.Tasks).ThenInclude(t => t.FollowUps).ThenInclude(f => f.AuthorEmployeeProfile)
            .FirstOrDefaultAsync(x => x.MeetingID == id, cancellationToken);

        if (m == null) return null;

        MeetingMinutesReadDto? minutesDto = null;
        if (m.Minutes != null)
        {
            var mm = m.Minutes;
            minutesDto = new MeetingMinutesReadDto
            {
                MeetingMinutesID = mm.MeetingMinutesID,
                MeetingID = mm.MeetingID,
                Body = mm.Body,
                RecordedByEmployeeProfileID = mm.RecordedByEmployeeProfileID,
                RecordedByName = FormatPersonName(mm.RecordedByEmployeeProfile.FullName),
                RecordedAtUtc = mm.RecordedAtUtc,
                UpdatedAtUtc = mm.UpdatedAtUtc,
                ApprovedByEmployeeProfileID = mm.ApprovedByEmployeeProfileID,
                ApprovedByName = mm.ApprovedByEmployeeProfile != null
                    ? FormatPersonName(mm.ApprovedByEmployeeProfile.FullName)
                    : null,
                ApprovedAtUtc = mm.ApprovedAtUtc,
            };
        }

        return new MeetingDetailDto
        {
            MeetingID = m.MeetingID,
            SchoolID = m.SchoolID,
            AcademicYearID = m.AcademicYearID,
            Title = m.Title,
            Status = (int)m.Status,
            StartAtUtc = m.StartAtUtc,
            EndAtUtc = m.EndAtUtc,
            OrganizerEmployeeProfileID = m.OrganizerEmployeeProfileID,
            OrganizerName = FormatPersonName(m.OrganizerEmployeeProfile.FullName),
            CreatedAtUtc = m.CreatedAtUtc,
            UpdatedAtUtc = m.UpdatedAtUtc,
            AttendeeCount = m.Attendees.Count,
            Description = m.Description,
            Location = m.Location,
            Attendees = m.Attendees
                .OrderBy(a => a.MeetingAttendeeID)
                .Select(a => new MeetingAttendeeReadDto
                {
                    MeetingAttendeeID = a.MeetingAttendeeID,
                    EmployeeProfileID = a.EmployeeProfileID,
                    EmployeeName = FormatPersonName(a.EmployeeProfile.FullName),
                    Role = (int)a.Role,
                    Response = (int)a.Response,
                    Notes = a.Notes,
                    CreatedAtUtc = a.CreatedAtUtc,
                    UpdatedAtUtc = a.UpdatedAtUtc,
                })
                .ToList(),
            Minutes = minutesDto,
            Tasks = m.Tasks
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.MeetingTaskID)
                .Select(t => new MeetingTaskReadDto
                {
                    MeetingTaskID = t.MeetingTaskID,
                    MeetingID = t.MeetingID,
                    Title = t.Title,
                    Details = t.Details,
                    AssignedToEmployeeProfileID = t.AssignedToEmployeeProfileID,
                    AssignedToName = t.AssignedToEmployeeProfile != null
                        ? FormatPersonName(t.AssignedToEmployeeProfile.FullName)
                        : null,
                    DueAtUtc = t.DueAtUtc,
                    Status = (int)t.Status,
                    SortOrder = t.SortOrder,
                    CreatedAtUtc = t.CreatedAtUtc,
                    UpdatedAtUtc = t.UpdatedAtUtc,
                    FollowUps = t.FollowUps
                        .OrderBy(f => f.CreatedAtUtc)
                        .ThenBy(f => f.MeetingTaskFollowUpID)
                        .Select(f => new MeetingTaskFollowUpReadDto
                        {
                            MeetingTaskFollowUpID = f.MeetingTaskFollowUpID,
                            MeetingTaskID = f.MeetingTaskID,
                            Note = f.Note,
                            ProgressPercent = f.ProgressPercent,
                            AuthorEmployeeProfileID = f.AuthorEmployeeProfileID,
                            AuthorName = f.AuthorEmployeeProfile != null
                                ? FormatPersonName(f.AuthorEmployeeProfile.FullName)
                                : null,
                            CreatedAtUtc = f.CreatedAtUtc,
                        })
                        .ToList(),
                })
                .ToList(),
        };
    }

    private async Task ValidateEmployeesInSchoolAsync(int schoolId, IEnumerable<int> employeeProfileIds, CancellationToken cancellationToken)
    {
        var ids = employeeProfileIds.Where(id => id > 0).Distinct().ToList();
        if (ids.Count == 0) return;
        var count = await _db.EmployeeProfiles.AsNoTracking()
            .CountAsync(e => ids.Contains(e.EmployeeProfileID) && e.SchoolID == schoolId, cancellationToken);
        if (count != ids.Count)
            throw new InvalidOperationException("One or more employee profiles were not found for this school.");
    }

    public async Task<int> CreateMeetingAsync(MeetingWriteDto dto, CancellationToken cancellationToken = default)
    {
        var orgOk = await _db.EmployeeProfiles.AsNoTracking()
            .AnyAsync(e => e.EmployeeProfileID == dto.OrganizerEmployeeProfileID && e.SchoolID == dto.SchoolID, cancellationToken);
        if (!orgOk)
            throw new InvalidOperationException("Organizer employee profile was not found for this school.");

        var attendeeIds = dto.Attendees.Select(a => a.EmployeeProfileID).Prepend(dto.OrganizerEmployeeProfileID);
        await ValidateEmployeesInSchoolAsync(dto.SchoolID, attendeeIds, cancellationToken);

        var yearId = dto.AcademicYearID is > 0
            ? dto.AcademicYearID!.Value
            : await GetActiveYearIdForSchoolAsync(dto.SchoolID, cancellationToken)
              ?? throw new InvalidOperationException("No academic year is configured for this school.");

        var yearOk = await _db.Years.AsNoTracking()
            .AnyAsync(y => y.YearID == yearId && y.SchoolID == dto.SchoolID, cancellationToken);
        if (!yearOk)
            throw new InvalidOperationException("Academic year does not belong to this school.");

        var now = DateTime.UtcNow;
        var entity = new Meeting
        {
            SchoolID = dto.SchoolID,
            AcademicYearID = yearId,
            OrganizerEmployeeProfileID = dto.OrganizerEmployeeProfileID,
            Title = dto.Title,
            Description = dto.Description,
            Location = dto.Location,
            StartAtUtc = dto.StartAtUtc,
            EndAtUtc = dto.EndAtUtc,
            Status = (MeetingStatus)dto.Status,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        _db.Meetings.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        foreach (var a in dto.Attendees.DistinctBy(x => x.EmployeeProfileID))
        {
            _db.MeetingAttendees.Add(new MeetingAttendee
            {
                MeetingID = entity.MeetingID,
                EmployeeProfileID = a.EmployeeProfileID,
                Role = (MeetingAttendeeRole)a.Role,
                Response = (MeetingAttendeeResponse)a.Response,
                Notes = a.Notes,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        return entity.MeetingID;
    }

    public async Task UpdateMeetingAsync(int id, MeetingWriteDto dto, CancellationToken cancellationToken = default)
    {
        var m = await _db.Meetings
            .Include(x => x.Attendees)
            .FirstOrDefaultAsync(x => x.MeetingID == id, cancellationToken)
            ?? throw new InvalidOperationException("Meeting was not found.");

        if (m.SchoolID != dto.SchoolID)
            throw new InvalidOperationException("School mismatch for this meeting.");

        var orgOk = await _db.EmployeeProfiles.AsNoTracking()
            .AnyAsync(e => e.EmployeeProfileID == dto.OrganizerEmployeeProfileID && e.SchoolID == dto.SchoolID, cancellationToken);
        if (!orgOk)
            throw new InvalidOperationException("Organizer employee profile was not found for this school.");

        var attendeeIds = dto.Attendees.Select(a => a.EmployeeProfileID).Prepend(dto.OrganizerEmployeeProfileID);
        await ValidateEmployeesInSchoolAsync(dto.SchoolID, attendeeIds, cancellationToken);

        var yearId = dto.AcademicYearID is > 0
            ? dto.AcademicYearID!.Value
            : m.AcademicYearID;

        var yearOk = await _db.Years.AsNoTracking()
            .AnyAsync(y => y.YearID == yearId && y.SchoolID == dto.SchoolID, cancellationToken);
        if (!yearOk)
            throw new InvalidOperationException("Academic year does not belong to this school.");

        var now = DateTime.UtcNow;
        m.AcademicYearID = yearId;
        m.OrganizerEmployeeProfileID = dto.OrganizerEmployeeProfileID;
        m.Title = dto.Title;
        m.Description = dto.Description;
        m.Location = dto.Location;
        m.StartAtUtc = dto.StartAtUtc;
        m.EndAtUtc = dto.EndAtUtc;
        m.Status = (MeetingStatus)dto.Status;
        m.UpdatedAtUtc = now;

        _db.MeetingAttendees.RemoveRange(m.Attendees);
        foreach (var a in dto.Attendees.DistinctBy(x => x.EmployeeProfileID))
        {
            _db.MeetingAttendees.Add(new MeetingAttendee
            {
                MeetingID = m.MeetingID,
                EmployeeProfileID = a.EmployeeProfileID,
                Role = (MeetingAttendeeRole)a.Role,
                Response = (MeetingAttendeeResponse)a.Response,
                Notes = a.Notes,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpsertMinutesAsync(int meetingId, MeetingMinutesWriteDto dto, CancellationToken cancellationToken = default)
    {
        var meeting = await _db.Meetings.AsNoTracking()
            .FirstOrDefaultAsync(m => m.MeetingID == meetingId, cancellationToken)
            ?? throw new InvalidOperationException("Meeting was not found.");

        var recOk = await _db.EmployeeProfiles.AsNoTracking()
            .AnyAsync(e => e.EmployeeProfileID == dto.RecordedByEmployeeProfileID && e.SchoolID == meeting.SchoolID, cancellationToken);
        if (!recOk)
            throw new InvalidOperationException("Recorder employee profile was not found for this school.");

        if (dto.ApprovedByEmployeeProfileID is > 0)
        {
            var apOk = await _db.EmployeeProfiles.AsNoTracking()
                .AnyAsync(e => e.EmployeeProfileID == dto.ApprovedByEmployeeProfileID && e.SchoolID == meeting.SchoolID, cancellationToken);
            if (!apOk)
                throw new InvalidOperationException("Approver employee profile was not found for this school.");
        }

        var now = DateTime.UtcNow;
        var existing = await _db.MeetingMinutes.FirstOrDefaultAsync(mm => mm.MeetingID == meetingId, cancellationToken);
        if (existing == null)
        {
            _db.MeetingMinutes.Add(new MeetingMinutes
            {
                MeetingID = meetingId,
                Body = dto.Body,
                RecordedByEmployeeProfileID = dto.RecordedByEmployeeProfileID,
                RecordedAtUtc = now,
                UpdatedAtUtc = now,
                ApprovedByEmployeeProfileID = dto.ApprovedByEmployeeProfileID is > 0 ? dto.ApprovedByEmployeeProfileID : null,
                ApprovedAtUtc = dto.ApprovedAtUtc,
            });
        }
        else
        {
            existing.Body = dto.Body;
            existing.RecordedByEmployeeProfileID = dto.RecordedByEmployeeProfileID;
            existing.UpdatedAtUtc = now;
            existing.ApprovedByEmployeeProfileID = dto.ApprovedByEmployeeProfileID is > 0 ? dto.ApprovedByEmployeeProfileID : null;
            existing.ApprovedAtUtc = dto.ApprovedAtUtc;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task ReplaceTasksAsync(int meetingId, IReadOnlyList<MeetingTaskWriteDto> tasks, CancellationToken cancellationToken = default)
    {
        var meeting = await _db.Meetings.AsNoTracking()
            .FirstOrDefaultAsync(m => m.MeetingID == meetingId, cancellationToken)
            ?? throw new InvalidOperationException("Meeting was not found.");

        var assigneeIds = tasks
            .Select(t => t.AssignedToEmployeeProfileID)
            .Where(id => id is > 0)
            .Cast<int>();
        var followAuthorIds = tasks.SelectMany(t => t.FollowUps.Select(f => f.AuthorEmployeeProfileID))
            .Where(id => id is > 0)
            .Cast<int>();
        await ValidateEmployeesInSchoolAsync(meeting.SchoolID, assigneeIds.Concat(followAuthorIds), cancellationToken);

        var existingTasks = await _db.MeetingTasks.Where(t => t.MeetingID == meetingId).ToListAsync(cancellationToken);
        _db.MeetingTasks.RemoveRange(existingTasks);
        await _db.SaveChangesAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var t in tasks)
        {
            var task = new MeetingTask
            {
                MeetingID = meetingId,
                Title = t.Title,
                Details = t.Details,
                AssignedToEmployeeProfileID = t.AssignedToEmployeeProfileID is > 0 ? t.AssignedToEmployeeProfileID : null,
                DueAtUtc = t.DueAtUtc,
                Status = (MeetingTaskStatus)t.Status,
                SortOrder = t.SortOrder,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            };
            _db.MeetingTasks.Add(task);
            await _db.SaveChangesAsync(cancellationToken);

            foreach (var f in t.FollowUps)
            {
                _db.MeetingTaskFollowUps.Add(new MeetingTaskFollowUp
                {
                    MeetingTaskID = task.MeetingTaskID,
                    Note = f.Note,
                    ProgressPercent = f.ProgressPercent,
                    AuthorEmployeeProfileID = f.AuthorEmployeeProfileID is > 0 ? f.AuthorEmployeeProfileID : null,
                    CreatedAtUtc = now,
                });
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<int?> GetSchoolIdForMeetingAsync(int meetingId, CancellationToken cancellationToken = default)
    {
        return _db.Meetings.AsNoTracking()
            .Where(x => x.MeetingID == meetingId)
            .Select(x => (int?)x.SchoolID)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
