using System;

namespace Backend.Models;

public class NotificationDelivery
{
    public Guid Id { get; set; }

    public Guid NotificationMessageId { get; set; }

    public NotificationMessage NotificationMessage { get; set; } = null!;

    public string RecipientUserId { get; set; } = string.Empty;

    public NotificationDeliveryChannel Channel { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ReadAtUtc { get; set; }
}
