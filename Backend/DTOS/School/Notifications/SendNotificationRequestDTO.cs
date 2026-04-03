using System.Collections.Generic;
using Backend.Models;

namespace Backend.DTOS.School.Notifications;

public class SendNotificationRequestDTO
{
    public string Title { get; set; } = string.Empty;

    public string? Body { get; set; }

    public NotificationTargetKind TargetKind { get; set; }

    public int? ClassId { get; set; }

    /// <summary>Used when <see cref="NotificationTargetKind.DirectUserIds"/>.</summary>
    public IReadOnlyList<string>? DirectUserIds { get; set; }

    /// <summary>When null or <see cref="NotificationChannelFlags.None"/>, defaults to in-app only.</summary>
    public NotificationChannelFlags? Channels { get; set; }
}
