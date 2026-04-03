using System;
using System.Collections.Generic;

namespace Backend.Models;

public class NotificationMessage
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public string SentByUserId { get; set; } = string.Empty;

    public NotificationTargetKind TargetKind { get; set; }

    public int? ClassId { get; set; }

    /// <summary>Bit flags of channels the sender asked for; non-InApp channels are not written as deliveries yet.</summary>
    public NotificationChannelFlags RequestedChannels { get; set; }

    public ICollection<NotificationDelivery> Deliveries { get; set; } = new List<NotificationDelivery>();
}
