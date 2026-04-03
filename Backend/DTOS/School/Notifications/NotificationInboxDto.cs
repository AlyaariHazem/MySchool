using System;
using Backend.Models;

namespace Backend.DTOS.School.Notifications;

public class NotificationInboxDto
{
    public Guid DeliveryId { get; set; }
    public Guid NotificationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime SentAtUtc { get; set; }
    public string SentByUserId { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public NotificationTargetKind TargetKind { get; set; }
    public NotificationChannelFlags RequestedChannels { get; set; }
}
