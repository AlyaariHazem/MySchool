/** Mirrors backend `MeetingStatus`. */
export enum MeetingStatus {
  Draft = 0,
  Scheduled = 1,
  InProgress = 2,
  Completed = 3,
  Cancelled = 4,
}

/** Mirrors backend `MeetingAttendeeRole`. */
export enum MeetingAttendeeRole {
  Required = 0,
  Optional = 1,
}

/** Mirrors backend `MeetingAttendeeResponse`. */
export enum MeetingAttendeeResponse {
  Pending = 0,
  Accepted = 1,
  Declined = 2,
  Tentative = 3,
}

/** Mirrors backend `MeetingTaskStatus`. */
export enum MeetingTaskStatus {
  Open = 0,
  InProgress = 1,
  Done = 2,
  Cancelled = 3,
}

export interface MeetingFilterDto {
  schoolID?: number | null;
  academicYearID?: number | null;
  status?: number | null;
}

export interface MeetingListItemDto {
  meetingID: number;
  schoolID: number;
  academicYearID: number;
  title: string;
  status: number;
  startAtUtc: string;
  endAtUtc?: string | null;
  organizerEmployeeProfileID: number;
  organizerName: string;
  createdAtUtc: string;
  updatedAtUtc: string;
  attendeeCount: number;
}

export interface MeetingAttendeeReadDto {
  meetingAttendeeID: number;
  employeeProfileID: number;
  employeeName: string;
  role: number;
  response: number;
  notes?: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface MeetingMinutesReadDto {
  meetingMinutesID: number;
  meetingID: number;
  body: string;
  recordedByEmployeeProfileID: number;
  recordedByName: string;
  recordedAtUtc: string;
  updatedAtUtc: string;
  approvedByEmployeeProfileID?: number | null;
  approvedByName?: string | null;
  approvedAtUtc?: string | null;
}

export interface MeetingTaskFollowUpReadDto {
  meetingTaskFollowUpID: number;
  meetingTaskID: number;
  note: string;
  progressPercent?: number | null;
  authorEmployeeProfileID?: number | null;
  authorName?: string | null;
  createdAtUtc: string;
}

export interface MeetingTaskReadDto {
  meetingTaskID: number;
  meetingID: number;
  title: string;
  details?: string | null;
  assignedToEmployeeProfileID?: number | null;
  assignedToName?: string | null;
  dueAtUtc?: string | null;
  status: number;
  sortOrder: number;
  createdAtUtc: string;
  updatedAtUtc: string;
  followUps: MeetingTaskFollowUpReadDto[];
}

export interface MeetingDetailDto extends MeetingListItemDto {
  description?: string | null;
  location?: string | null;
  attendees: MeetingAttendeeReadDto[];
  minutes?: MeetingMinutesReadDto | null;
  tasks: MeetingTaskReadDto[];
}

export interface MeetingAttendeeWriteDto {
  employeeProfileID: number;
  role: number;
  response: number;
  notes?: string | null;
}

export interface MeetingWriteDto {
  schoolID: number;
  academicYearID?: number | null;
  organizerEmployeeProfileID: number;
  title: string;
  description?: string | null;
  location?: string | null;
  startAtUtc: string;
  endAtUtc?: string | null;
  status: number;
  attendees: MeetingAttendeeWriteDto[];
}

export interface MeetingMinutesWriteDto {
  body: string;
  recordedByEmployeeProfileID: number;
  approvedByEmployeeProfileID?: number | null;
  approvedAtUtc?: string | null;
}

export interface MeetingTaskFollowUpWriteDto {
  note: string;
  progressPercent?: number | null;
  authorEmployeeProfileID?: number | null;
}

export interface MeetingTaskWriteDto {
  title: string;
  details?: string | null;
  assignedToEmployeeProfileID?: number | null;
  dueAtUtc?: string | null;
  status: number;
  sortOrder: number;
  followUps: MeetingTaskFollowUpWriteDto[];
}

export function meetingFilterForApi(f: MeetingFilterDto): Record<string, unknown> {
  const o: Record<string, unknown> = {};
  if (f.schoolID != null && f.schoolID > 0) o['schoolID'] = f.schoolID;
  if (f.academicYearID != null && f.academicYearID > 0) o['academicYearID'] = f.academicYearID;
  if (f.status != null && f.status >= 0) o['status'] = f.status;
  return o;
}

export function meetingWriteForApi(d: MeetingWriteDto): Record<string, unknown> {
  const o: Record<string, unknown> = {
    schoolID: d.schoolID,
    organizerEmployeeProfileID: d.organizerEmployeeProfileID,
    title: d.title,
    startAtUtc: d.startAtUtc,
    status: d.status,
    attendees: (d.attendees ?? []).map((a) => ({
      employeeProfileID: a.employeeProfileID,
      role: a.role,
      response: a.response,
      notes: a.notes ?? null,
    })),
  };
  if (d.academicYearID != null && d.academicYearID > 0) o['academicYearID'] = d.academicYearID;
  if (d.description != null && d.description !== '') o['description'] = d.description;
  if (d.location != null && d.location !== '') o['location'] = d.location;
  if (d.endAtUtc != null && d.endAtUtc !== '') o['endAtUtc'] = d.endAtUtc;
  return o;
}

export function meetingMinutesWriteForApi(d: MeetingMinutesWriteDto): Record<string, unknown> {
  const o: Record<string, unknown> = {
    body: d.body,
    recordedByEmployeeProfileID: d.recordedByEmployeeProfileID,
  };
  if (d.approvedByEmployeeProfileID != null && d.approvedByEmployeeProfileID > 0)
    o['approvedByEmployeeProfileID'] = d.approvedByEmployeeProfileID;
  if (d.approvedAtUtc != null && d.approvedAtUtc !== '') o['approvedAtUtc'] = d.approvedAtUtc;
  return o;
}

export function meetingTasksWriteForApi(tasks: MeetingTaskWriteDto[]): unknown[] {
  return (tasks ?? []).map((t) => ({
    title: t.title,
    details: t.details ?? null,
    assignedToEmployeeProfileID: t.assignedToEmployeeProfileID ?? null,
    dueAtUtc: t.dueAtUtc ?? null,
    status: t.status,
    sortOrder: t.sortOrder,
    followUps: (t.followUps ?? []).map((f) => ({
      note: f.note,
      progressPercent: f.progressPercent ?? null,
      authorEmployeeProfileID: f.authorEmployeeProfileID ?? null,
    })),
  }));
}

export function isoUtcToDatetimeLocal(iso: string): string {
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return '';
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

export function datetimeLocalToIsoUtc(value: string): string {
  const d = new Date(value);
  return d.toISOString();
}
