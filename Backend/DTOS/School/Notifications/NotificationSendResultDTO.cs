using System;

namespace Backend.DTOS.School.Notifications;

public class NotificationSendResultDTO
{
    public Guid NotificationId { get; set; }
    public int RecipientCount { get; set; }
}
