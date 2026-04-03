namespace Backend.Models;

/// <summary>Who the notification is addressed to (resolution happens in the repository).</summary>
public enum NotificationTargetKind : byte
{
    DirectUserIds = 0,
    ClassStudents = 1,
    ClassGuardians = 2,
    ClassTeachers = 3,
    AllGuardiansInTenant = 4,
    AllStudentsInTenant = 5,
    AllTeachersInTenant = 6,
    /// <summary>Everyone in the tenant with a linked user account (students, guardians, teachers).</summary>
    AllUsersInTenant = 7
}

/// <summary>Requested delivery channels on the message (stored for future processors). In-app rows are created only for <see cref="InApp"/>.</summary>
[System.Flags]
public enum NotificationChannelFlags
{
    None = 0,
    InApp = 1,
    Email = 2,
    Sms = 4,
    Push = 8
}

/// <summary>Per-recipient channel for a delivery row.</summary>
public enum NotificationDeliveryChannel : byte
{
    InApp = 0,
    Email = 1,
    Sms = 2,
    Push = 3
}
