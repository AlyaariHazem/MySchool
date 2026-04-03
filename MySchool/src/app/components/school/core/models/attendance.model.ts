/** Matches Backend.Models.AttendanceStatus */
export enum AttendanceStatus {
  Present = 0,
  Absent = 1,
  Late = 2,
  Excused = 3
}

export interface AttendanceDto {
  attendanceId: string;
  studentID: number;
  studentName?: string;
  classID: number;
  className?: string;
  date: string;
  status: AttendanceStatus;
  remarks?: string;
  recordedBy: string;
  tenantId?: number | null;
  createdAt: string;
  updatedAt?: string | null;
}

export interface BulkAttendanceEntry {
  studentID: number;
  status: AttendanceStatus;
  remarks?: string | null;
}

export interface BulkAttendanceRequest {
  classID: number;
  date: string;
  entries: BulkAttendanceEntry[];
}
