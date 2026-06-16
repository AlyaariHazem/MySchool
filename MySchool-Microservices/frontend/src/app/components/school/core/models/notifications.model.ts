/** Mirrors backend `NotificationTargetKind`. */
export enum NotificationTargetKind {
  DirectUserIds = 0,
  ClassStudents = 1,
  ClassGuardians = 2,
  ClassTeachers = 3,
  AllGuardiansInTenant = 4,
  AllStudentsInTenant = 5,
  AllTeachersInTenant = 6,
  AllUsersInTenant = 7
}

/** Mirrors backend `NotificationChannelFlags`. */
export enum NotificationChannelFlags {
  None = 0,
  InApp = 1,
  Email = 2,
  Sms = 4,
  Push = 8
}

export interface NotificationInboxDto {
  deliveryId: string;
  notificationId: string;
  title: string;
  body: string;
  sentAtUtc: string;
  sentByUserId: string;
  isRead: boolean;
  targetKind: NotificationTargetKind;
  requestedChannels: NotificationChannelFlags;
}

export interface NotificationSendResultDto {
  notificationId: string;
  recipientCount: number;
}

export interface PagedResultDto<T> {
  data: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface SendNotificationRequest {
  title: string;
  body?: string | null;
  targetKind: NotificationTargetKind;
  classId?: number | null;
  directUserIds?: string[] | null;
  channels?: NotificationChannelFlags | null;
}
