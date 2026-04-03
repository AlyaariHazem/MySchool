using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Common;
using Backend.Data;
using Backend.DTOS.School.Notifications;
using Backend.Interfaces;
using Backend.Models;
using Backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository;

public class NotificationRepository : INotificationRepository
{
    private const int MaxTitleLength = 500;
    private const int MaxBodyLength = 20000;
    private const int MaxPageSize = 100;

    private readonly TenantDbContext _db;
    private readonly HtmlSanitizationService _htmlSanitizer;

    public NotificationRepository(TenantDbContext db, HtmlSanitizationService htmlSanitizer)
    {
        _db = db;
        _htmlSanitizer = htmlSanitizer;
    }

    public async Task<(bool Ok, NotificationSendResultDTO? Result, string? Error)> SendAsync(
        SendNotificationRequestDTO request,
        string senderUserId,
        bool senderMayUseTenantWideTargets,
        int? senderTeacherId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(senderUserId))
            return (false, null, "Sender is not authenticated.");

        var title = (request.Title ?? string.Empty).Trim();
        if (title.Length == 0)
            return (false, null, "Title is required.");
        if (title.Length > MaxTitleLength)
            return (false, null, $"Title must be at most {MaxTitleLength} characters.");

        var rawBody = request.Body ?? string.Empty;
        var body = _htmlSanitizer.Sanitize(rawBody);
        if (body.Length > MaxBodyLength)
            return (false, null, $"Body must be at most {MaxBodyLength} characters after sanitization.");

        var requestedChannels = request.Channels is { } c && c != NotificationChannelFlags.None
            ? c
            : NotificationChannelFlags.InApp;

        if (!senderMayUseTenantWideTargets && senderTeacherId is null)
            return (false, null, "Teachers must be linked to a teacher profile to send notifications.");

        switch (request.TargetKind)
        {
            case NotificationTargetKind.DirectUserIds:
            case NotificationTargetKind.ClassStudents:
            case NotificationTargetKind.ClassGuardians:
            case NotificationTargetKind.ClassTeachers:
                break;
            case NotificationTargetKind.AllGuardiansInTenant:
            case NotificationTargetKind.AllStudentsInTenant:
            case NotificationTargetKind.AllTeachersInTenant:
            case NotificationTargetKind.AllUsersInTenant:
                if (!senderMayUseTenantWideTargets)
                    return (false, null, "Only administrators or managers can use tenant-wide notification targets.");
                break;
            default:
                return (false, null, "Unsupported notification target.");
        }

        var classId = request.ClassId;
        if (request.TargetKind is NotificationTargetKind.ClassStudents
            or NotificationTargetKind.ClassGuardians
            or NotificationTargetKind.ClassTeachers)
        {
            if (classId is null or < 1)
                return (false, null, "ClassId is required for class-scoped targets.");

            var classExists = await _db.Classes.AsNoTracking().AnyAsync(c => c.ClassID == classId, cancellationToken);
            if (!classExists)
                return (false, null, "Class was not found.");

            if (!senderMayUseTenantWideTargets)
            {
                var teaches = await _db.CoursePlans.AsNoTracking()
                    .AnyAsync(cp => cp.TeacherID == senderTeacherId!.Value && cp.ClassID == classId, cancellationToken);
                if (!teaches)
                    return (false, null, "You can only notify classes you teach.");
            }
        }
        else if (classId is not null)
            return (false, null, "ClassId should only be set for class-scoped targets.");

        HashSet<string> recipients;
        try
        {
            recipients = await ResolveRecipientsAsync(request, senderMayUseTenantWideTargets, senderTeacherId, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return (false, null, ex.Message);
        }

        recipients.Remove(senderUserId);
        recipients.RemoveWhere(string.IsNullOrWhiteSpace);

        if (recipients.Count == 0)
            return (false, null, "No recipients were resolved for this notification.");

        var createInAppDeliveries = requestedChannels.HasFlag(NotificationChannelFlags.InApp);

        var messageId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var message = new NotificationMessage
        {
            Id = messageId,
            Title = title,
            Body = body,
            CreatedAtUtc = now,
            SentByUserId = senderUserId,
            TargetKind = request.TargetKind,
            ClassId = classId,
            RequestedChannels = requestedChannels
        };

        _db.NotificationMessages.Add(message);

        if (createInAppDeliveries)
        {
            foreach (var uid in recipients)
            {
                _db.NotificationDeliveries.Add(new NotificationDelivery
                {
                    Id = Guid.NewGuid(),
                    NotificationMessageId = messageId,
                    RecipientUserId = uid,
                    Channel = NotificationDeliveryChannel.InApp,
                    CreatedAtUtc = now
                });
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        return (true, new NotificationSendResultDTO
        {
            NotificationId = messageId,
            RecipientCount = createInAppDeliveries ? recipients.Count : 0
        }, null);
    }

    public async Task<PagedResult<NotificationInboxDto>> GetInboxAsync(
        string recipientUserId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(recipientUserId))
            return new PagedResult<NotificationInboxDto>(Array.Empty<NotificationInboxDto>(), 1, pageSize, 0, 0);

        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var baseQuery = _db.NotificationDeliveries
            .AsNoTracking()
            .Include(d => d.NotificationMessage)
            .Where(d => d.RecipientUserId == recipientUserId && d.Channel == NotificationDeliveryChannel.InApp);

        var total = await baseQuery.CountAsync(cancellationToken);
        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);

        var rows = await baseQuery
            .OrderByDescending(d => d.NotificationMessage.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new NotificationInboxDto
            {
                DeliveryId = d.Id,
                NotificationId = d.NotificationMessageId,
                Title = d.NotificationMessage.Title,
                Body = d.NotificationMessage.Body,
                SentAtUtc = d.NotificationMessage.CreatedAtUtc,
                SentByUserId = d.NotificationMessage.SentByUserId,
                IsRead = d.ReadAtUtc != null,
                TargetKind = d.NotificationMessage.TargetKind,
                RequestedChannels = d.NotificationMessage.RequestedChannels
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<NotificationInboxDto>(rows, pageNumber, pageSize, total, totalPages);
    }

    public Task<int> GetUnreadCountAsync(string recipientUserId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(recipientUserId))
            return Task.FromResult(0);

        return _db.NotificationDeliveries.AsNoTracking()
            .CountAsync(
                d => d.RecipientUserId == recipientUserId
                     && d.Channel == NotificationDeliveryChannel.InApp
                     && d.ReadAtUtc == null,
                cancellationToken);
    }

    public async Task<bool> MarkReadAsync(string recipientUserId, Guid deliveryId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(recipientUserId))
            return false;

        var row = await _db.NotificationDeliveries
            .FirstOrDefaultAsync(
                d => d.Id == deliveryId && d.RecipientUserId == recipientUserId && d.Channel == NotificationDeliveryChannel.InApp,
                cancellationToken);

        if (row is null)
            return false;

        if (row.ReadAtUtc is null)
        {
            row.ReadAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    private async Task<HashSet<string>> ResolveRecipientsAsync(
        SendNotificationRequestDTO request,
        bool senderMayUseTenantWideTargets,
        int? senderTeacherId,
        CancellationToken cancellationToken)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);

        switch (request.TargetKind)
        {
            case NotificationTargetKind.DirectUserIds:
            {
                var ids = request.DirectUserIds ?? Array.Empty<string>();
                foreach (var id in ids)
                {
                    if (!string.IsNullOrWhiteSpace(id))
                        set.Add(id.Trim());
                }

                if (set.Count == 0)
                    throw new InvalidOperationException("DirectUserIds must contain at least one user id.");

                if (!senderMayUseTenantWideTargets)
                {
                    var allowed = await GetTeacherReachableUserIdsAsync(senderTeacherId!.Value, cancellationToken);
                    foreach (var uid in set)
                    {
                        if (!allowed.Contains(uid))
                            throw new InvalidOperationException("One or more user ids are outside the classes you teach.");
                    }
                }

                return set;
            }
            case NotificationTargetKind.ClassStudents:
                await AddClassStudentUserIdsAsync(set, request.ClassId!.Value, cancellationToken);
                return set;
            case NotificationTargetKind.ClassGuardians:
                await AddClassGuardianUserIdsAsync(set, request.ClassId!.Value, cancellationToken);
                return set;
            case NotificationTargetKind.ClassTeachers:
                await AddClassTeacherUserIdsAsync(set, request.ClassId!.Value, cancellationToken);
                return set;
            case NotificationTargetKind.AllGuardiansInTenant:
                foreach (var uid in await _db.Guardians.AsNoTracking()
                             .Where(g => g.UserID != null && g.UserID != "")
                             .Select(g => g.UserID!)
                             .ToListAsync(cancellationToken))
                    set.Add(uid);
                return set;
            case NotificationTargetKind.AllStudentsInTenant:
                foreach (var uid in await _db.Students.AsNoTracking()
                             .Where(s => s.UserID != null && s.UserID != "")
                             .Select(s => s.UserID!)
                             .ToListAsync(cancellationToken))
                    set.Add(uid);
                return set;
            case NotificationTargetKind.AllTeachersInTenant:
                foreach (var uid in await _db.Teachers.AsNoTracking()
                             .Where(t => t.UserID != null && t.UserID != "")
                             .Select(t => t.UserID!)
                             .ToListAsync(cancellationToken))
                    set.Add(uid);
                return set;
            case NotificationTargetKind.AllUsersInTenant:
                foreach (var uid in await _db.Students.AsNoTracking()
                             .Where(s => s.UserID != null && s.UserID != "")
                             .Select(s => s.UserID!)
                             .ToListAsync(cancellationToken))
                    set.Add(uid);
                foreach (var uid in await _db.Guardians.AsNoTracking()
                             .Where(g => g.UserID != null && g.UserID != "")
                             .Select(g => g.UserID!)
                             .ToListAsync(cancellationToken))
                    set.Add(uid);
                foreach (var uid in await _db.Teachers.AsNoTracking()
                             .Where(t => t.UserID != null && t.UserID != "")
                             .Select(t => t.UserID!)
                             .ToListAsync(cancellationToken))
                    set.Add(uid);
                return set;
            default:
                throw new InvalidOperationException("Unsupported notification target.");
        }
    }

    private async Task AddClassStudentUserIdsAsync(HashSet<string> set, int classId, CancellationToken cancellationToken)
    {
        var ids = await _db.Students.AsNoTracking()
            .Where(s => s.Division.ClassID == classId && s.UserID != null && s.UserID != "")
            .Select(s => s.UserID!)
            .ToListAsync(cancellationToken);
        foreach (var id in ids)
            set.Add(id);
    }

    private async Task AddClassGuardianUserIdsAsync(HashSet<string> set, int classId, CancellationToken cancellationToken)
    {
        var studentIds = await _db.Students.AsNoTracking()
            .Where(s => s.Division.ClassID == classId)
            .Select(s => s.StudentID)
            .ToListAsync(cancellationToken);

        var guardianIds = await _db.Students.AsNoTracking()
            .Where(s => s.Division.ClassID == classId)
            .Select(s => s.GuardianID)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var uid in await _db.Guardians.AsNoTracking()
                     .Where(g => guardianIds.Contains(g.GuardianID) && g.UserID != null && g.UserID != "")
                     .Select(g => g.UserID!)
                     .ToListAsync(cancellationToken))
            set.Add(uid);

        if (studentIds.Count > 0)
        {
            foreach (var uid in await _db.AccountStudentGuardians.AsNoTracking()
                         .Where(a => studentIds.Contains(a.StudentID))
                         .Join(
                             _db.Guardians.AsNoTracking(),
                             a => a.GuardianID,
                             g => g.GuardianID,
                             (_, g) => g.UserID)
                         .Where(uid => uid != null && uid != "")
                         .ToListAsync(cancellationToken))
                set.Add(uid!);
        }
    }

    private async Task AddClassTeacherUserIdsAsync(HashSet<string> set, int classId, CancellationToken cancellationToken)
    {
        var ids = await _db.CoursePlans.AsNoTracking()
            .Where(cp => cp.ClassID == classId)
            .Select(cp => cp.Teacher.UserID)
            .Where(uid => uid != null && uid != "")
            .Distinct()
            .ToListAsync(cancellationToken);
        foreach (var id in ids)
            set.Add(id!);
    }

    private async Task<HashSet<string>> GetTeacherReachableUserIdsAsync(int teacherId, CancellationToken cancellationToken)
    {
        var classIds = await _db.CoursePlans.AsNoTracking()
            .Where(cp => cp.TeacherID == teacherId)
            .Select(cp => cp.ClassID)
            .Distinct()
            .ToListAsync(cancellationToken);

        var set = new HashSet<string>(StringComparer.Ordinal);
        if (classIds.Count == 0)
            return set;

        foreach (var uid in await _db.Students.AsNoTracking()
                     .Where(s => classIds.Contains(s.Division.ClassID) && s.UserID != null && s.UserID != "")
                     .Select(s => s.UserID!)
                     .ToListAsync(cancellationToken))
            set.Add(uid);

        var studentIds = await _db.Students.AsNoTracking()
            .Where(s => classIds.Contains(s.Division.ClassID))
            .Select(s => s.StudentID)
            .ToListAsync(cancellationToken);

        var guardianIds = await _db.Students.AsNoTracking()
            .Where(s => classIds.Contains(s.Division.ClassID))
            .Select(s => s.GuardianID)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var uid in await _db.Guardians.AsNoTracking()
                     .Where(g => guardianIds.Contains(g.GuardianID) && g.UserID != null && g.UserID != "")
                     .Select(g => g.UserID!)
                     .ToListAsync(cancellationToken))
            set.Add(uid);

        if (studentIds.Count > 0)
        {
            foreach (var uid in await _db.AccountStudentGuardians.AsNoTracking()
                         .Where(a => studentIds.Contains(a.StudentID))
                         .Join(
                             _db.Guardians.AsNoTracking(),
                             a => a.GuardianID,
                             g => g.GuardianID,
                             (_, g) => g.UserID)
                         .Where(uid => uid != null && uid != "")
                         .ToListAsync(cancellationToken))
                set.Add(uid!);
        }

        foreach (var uid in await _db.CoursePlans.AsNoTracking()
                     .Where(cp => classIds.Contains(cp.ClassID))
                     .Select(cp => cp.Teacher.UserID)
                     .Where(uid => uid != null && uid != "")
                     .ToListAsync(cancellationToken))
            set.Add(uid!);

        return set;
    }
}
