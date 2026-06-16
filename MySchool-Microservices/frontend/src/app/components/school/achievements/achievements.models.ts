/** Mirrors backend enums (int). */
export enum AchievementRequestStatus {
  Draft = 0,
  Submitted = 1,
  InReview = 2,
  Approved = 3,
  Rejected = 4,
  Cancelled = 5,
}

export enum AchievementApprovalDecision {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
  Skipped = 3,
}

export interface AchievementRequestFilterDto {
  schoolID?: number | null;
  academicYearID?: number | null;
  employeeProfileID?: number | null;
  status?: number | null;
}

export interface AchievementCatalogItemDto {
  achievementID: number;
  code: string;
  title: string;
  defaultPoints: number;
  academicYearID?: number | null;
}

export interface AchievementCatalogFilterDto {
  schoolID: number;
  academicYearID?: number | null;
}

export interface AchievementRequestListItemDto {
  achievementRequestID: number;
  schoolID: number;
  academicYearID: number;
  employeeProfileID: number;
  employeeName: string;
  achievementID?: number | null;
  achievementTitle?: string | null;
  customTitle?: string | null;
  status: number;
  submittedAtUtc: string;
  resolvedAtUtc?: string | null;
}

export interface AchievementApprovalReadDto {
  achievementApprovalID: number;
  approverEmployeeProfileID: number;
  approverName: string;
  decision: number;
  comment?: string | null;
  sortOrder: number;
  decidedAtUtc?: string | null;
  createdAtUtc: string;
}

export interface AchievementAttachmentReadDto {
  achievementAttachmentID: number;
  fileName: string;
  contentType?: string | null;
  fileSizeBytes?: number | null;
  uploadedAtUtc: string;
}

export interface AchievementPointsLedgerReadDto {
  achievementPointsLedgerID: number;
  deltaPoints: number;
  reason: string;
  createdAtUtc: string;
}

export interface AchievementRequestDetailDto extends AchievementRequestListItemDto {
  notes?: string | null;
  updatedAtUtc: string;
  approvals: AchievementApprovalReadDto[];
  attachments: AchievementAttachmentReadDto[];
  ledgerEntries: AchievementPointsLedgerReadDto[];
}

export interface AchievementRequestWriteDto {
  schoolID: number;
  /** Omit or null: server uses the school active academic year. */
  academicYearID?: number | null;
  employeeProfileID: number;
  achievementID?: number | null;
  customTitle?: string | null;
  notes?: string | null;
  status: number;
}

export function achievementRequestFilterForApi(f: AchievementRequestFilterDto): Record<string, unknown> {
  return {
    schoolID: f.schoolID ?? null,
    academicYearID: f.academicYearID ?? null,
    employeeProfileID: f.employeeProfileID ?? null,
    status: f.status ?? null,
  };
}

export function achievementCatalogFilterForApi(f: AchievementCatalogFilterDto): Record<string, unknown> {
  const body: Record<string, unknown> = { schoolID: f.schoolID };
  if (f.academicYearID != null && f.academicYearID > 0) {
    body['academicYearID'] = f.academicYearID;
  }
  return body;
}

export function achievementRequestWriteForApi(d: AchievementRequestWriteDto): Record<string, unknown> {
  const body: Record<string, unknown> = {
    schoolID: d.schoolID,
    employeeProfileID: d.employeeProfileID,
    achievementID: d.achievementID != null && d.achievementID > 0 ? d.achievementID : null,
    customTitle: d.customTitle != null && String(d.customTitle).trim() !== '' ? String(d.customTitle).trim() : null,
    notes: d.notes != null && String(d.notes).trim() !== '' ? String(d.notes).trim() : null,
    status: d.status,
  };
  if (d.academicYearID != null && d.academicYearID > 0) {
    body['academicYearID'] = d.academicYearID;
  }
  return body;
}

export function displayAchievementTitle(row: Pick<AchievementRequestListItemDto, 'achievementTitle' | 'customTitle'>): string {
  const a = row.achievementTitle?.trim();
  if (a) return a;
  const c = row.customTitle?.trim();
  if (c) return c;
  return '';
}
