using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Common;
using Backend.DTOS.School.Notifications;

namespace Backend.Interfaces;

public interface INotificationRepository
{
    /// <param name="senderMayUseTenantWideTargets">True for ADMIN or MANAGER.</param>
    /// <param name="senderTeacherId">Teacher row id when sender is a teacher (for scope checks).</param>
    Task<(bool Ok, NotificationSendResultDTO? Result, string? Error)> SendAsync(
        SendNotificationRequestDTO request,
        string senderUserId,
        bool senderMayUseTenantWideTargets,
        int? senderTeacherId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<NotificationInboxDto>> GetInboxAsync(
        string recipientUserId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(string recipientUserId, CancellationToken cancellationToken = default);

    Task<bool> MarkReadAsync(string recipientUserId, Guid deliveryId, CancellationToken cancellationToken = default);
}
