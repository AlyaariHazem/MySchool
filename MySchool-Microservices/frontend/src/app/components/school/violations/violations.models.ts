/** Mirrors backend `ViolationStatus` (int). */
export enum ViolationStatus {
  Draft = 0,
  Open = 1,
  InProgress = 2,
  Resolved = 3,
  Closed = 4,
  Cancelled = 5,
}

/** Mirrors backend `ViolationKind` (int). */
export enum ViolationKind {
  Clarification = 0,
  WrittenWarning = 1,
  AttentionNotice = 2,
  FinalWarning = 3,
}

/** Mirrors backend `ViolationActionCategory` (int). */
export enum ViolationActionCategory {
  GeneralNote = 0,
  MeetingHeld = 1,
  FormalDocumentation = 2,
  Other = 9,
}

export interface ViolationFilterDto {
  schoolID?: number | null;
  academicYearID?: number | null;
  subjectEmployeeProfileID?: number | null;
  status?: number | null;
}

export interface ViolationTypeListItemDto {
  violationTypeID: number;
  schoolID: number;
  kind: number;
  name: string;
  description?: string | null;
  sortOrder: number;
  isActive: boolean;
}

export interface ViolationListItemDto {
  violationID: number;
  schoolID: number;
  academicYearID?: number | null;
  violationTypeID: number;
  violationTypeKind: number;
  violationTypeName: string;
  subjectEmployeeProfileID: number;
  subjectEmployeeName: string;
  openedByEmployeeProfileID?: number | null;
  openedByName?: string | null;
  title: string;
  status: number;
  openedAtUtc: string;
  updatedAtUtc: string;
  closedAtUtc?: string | null;
}

export interface ViolationResponseReadDto {
  violationResponseID: number;
  violationID: number;
  authorEmployeeProfileID?: number | null;
  authorName?: string | null;
  body: string;
  createdAtUtc: string;
}

export interface ViolationActionReadDto {
  violationActionID: number;
  violationID: number;
  category: number;
  title: string;
  notes?: string | null;
  performedByEmployeeProfileID: number;
  performedByName: string;
  performedAtUtc: string;
}

export interface ViolationEscalationHistoryReadDto {
  violationEscalationHistoryID: number;
  violationID: number;
  previousViolationTypeID?: number | null;
  previousKind?: number | null;
  previousTypeName?: string | null;
  newViolationTypeID: number;
  newKind: number;
  newTypeName: string;
  reason?: string | null;
  changedByEmployeeProfileID: number;
  changedByName: string;
  changedAtUtc: string;
}

export interface ViolationDetailDto extends ViolationListItemDto {
  details?: string | null;
  responses: ViolationResponseReadDto[];
  actions: ViolationActionReadDto[];
  escalationHistory: ViolationEscalationHistoryReadDto[];
}

export interface ViolationWriteDto {
  schoolID: number;
  academicYearID?: number | null;
  subjectEmployeeProfileID: number;
  openedByEmployeeProfileID?: number | null;
  /** 0 = server picks Clarification for the school (create only). */
  violationTypeID: number;
  title: string;
  details?: string | null;
  status: number;
}

export interface ViolationResponseWriteDto {
  body: string;
  authorEmployeeProfileID?: number | null;
}

export interface ViolationActionWriteDto {
  category: number;
  title: string;
  notes?: string | null;
  performedByEmployeeProfileID: number;
}

export interface ViolationEscalateDto {
  newViolationTypeID: number;
  changedByEmployeeProfileID: number;
  reason?: string | null;
}

export function violationFilterForApi(f: ViolationFilterDto): Record<string, unknown> {
  return {
    schoolID: f.schoolID ?? null,
    academicYearID: f.academicYearID ?? null,
    subjectEmployeeProfileID: f.subjectEmployeeProfileID ?? null,
    status: f.status ?? null,
  };
}

export function violationTypesFilterForApi(schoolID: number): Record<string, unknown> {
  return { schoolID };
}

export function violationWriteForApi(d: ViolationWriteDto): Record<string, unknown> {
  const body: Record<string, unknown> = {
    schoolID: d.schoolID,
    subjectEmployeeProfileID: d.subjectEmployeeProfileID,
    openedByEmployeeProfileID:
      d.openedByEmployeeProfileID != null && d.openedByEmployeeProfileID > 0 ? d.openedByEmployeeProfileID : null,
    violationTypeID: d.violationTypeID > 0 ? d.violationTypeID : 0,
    title: String(d.title ?? '').trim(),
    details: d.details != null && String(d.details).trim() !== '' ? String(d.details).trim() : null,
    status: d.status,
  };
  if (d.academicYearID != null && d.academicYearID > 0) {
    body['academicYearID'] = d.academicYearID;
  }
  return body;
}

export function violationResponseWriteForApi(d: ViolationResponseWriteDto): Record<string, unknown> {
  return {
    body: String(d.body ?? '').trim(),
    authorEmployeeProfileID:
      d.authorEmployeeProfileID != null && d.authorEmployeeProfileID > 0 ? d.authorEmployeeProfileID : null,
  };
}

export function violationActionWriteForApi(d: ViolationActionWriteDto): Record<string, unknown> {
  return {
    category: d.category,
    title: String(d.title ?? '').trim(),
    notes: d.notes != null && String(d.notes).trim() !== '' ? String(d.notes).trim() : null,
    performedByEmployeeProfileID: d.performedByEmployeeProfileID,
  };
}

export function violationEscalateForApi(d: ViolationEscalateDto): Record<string, unknown> {
  return {
    newViolationTypeID: d.newViolationTypeID,
    changedByEmployeeProfileID: d.changedByEmployeeProfileID,
    reason: d.reason != null && String(d.reason).trim() !== '' ? String(d.reason).trim() : null,
  };
}
