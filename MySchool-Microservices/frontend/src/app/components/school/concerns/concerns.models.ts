/** Mirrors backend `ConcernStatus`. */
export enum ConcernStatus {
  Draft = 0,
  Submitted = 1,
  UnderReview = 2,
  InProgress = 3,
  Resolved = 4,
  Rejected = 5,
  Closed = 6,
  Cancelled = 7,
}

/** Mirrors backend `ConcernCategoryKind` for category list filter. */
export enum ConcernCategoryKind {
  Complaint = 0,
  Suggestion = 1,
  Both = 2,
}

export type ConcernKind = 'complaint' | 'suggestion';

export interface ConcernFilterDto {
  schoolID?: number | null;
  academicYearID?: number | null;
  status?: number | null;
  submitterEmployeeProfileID?: number | null;
}

export interface ConcernCategoryListItemDto {
  concernCategoryID: number;
  schoolID: number;
  code: string;
  categoryKind: number;
  name: string;
  nameAr?: string | null;
  description?: string | null;
  isActive: boolean;
}

export interface ComplaintListItemDto {
  complaintID: number;
  schoolID: number;
  academicYearID: number;
  concernCategoryID: number;
  categoryCode: string;
  categoryName: string;
  categoryNameAr?: string | null;
  submitterEmployeeProfileID: number;
  submitterName: string;
  assignedToEmployeeProfileID?: number | null;
  assignedToName?: string | null;
  title: string;
  status: number;
  submittedAtUtc: string;
  updatedAtUtc: string;
  closedAtUtc?: string | null;
}

export interface SuggestionListItemDto {
  suggestionID: number;
  schoolID: number;
  academicYearID: number;
  concernCategoryID: number;
  categoryCode: string;
  categoryName: string;
  categoryNameAr?: string | null;
  submitterEmployeeProfileID: number;
  submitterName: string;
  assignedToEmployeeProfileID?: number | null;
  assignedToName?: string | null;
  title: string;
  status: number;
  submittedAtUtc: string;
  updatedAtUtc: string;
  closedAtUtc?: string | null;
}

export interface ConcernActionLogReadDto {
  concernActionLogID: number;
  actionKind: number;
  oldStatus?: number | null;
  newStatus?: number | null;
  comment?: string | null;
  actorEmployeeProfileID?: number | null;
  actorName?: string | null;
  createdAtUtc: string;
}

export interface ComplaintDetailDto extends ComplaintListItemDto {
  details?: string | null;
  actionLogs: ConcernActionLogReadDto[];
}

export interface SuggestionDetailDto extends SuggestionListItemDto {
  details?: string | null;
  actionLogs: ConcernActionLogReadDto[];
}

export interface ComplaintWriteDto {
  schoolID: number;
  academicYearID?: number | null;
  concernCategoryID: number;
  submitterEmployeeProfileID: number;
  assignedToEmployeeProfileID?: number | null;
  title: string;
  details?: string | null;
  status: number;
}

export interface SuggestionWriteDto {
  schoolID: number;
  academicYearID?: number | null;
  concernCategoryID: number;
  submitterEmployeeProfileID: number;
  assignedToEmployeeProfileID?: number | null;
  title: string;
  details?: string | null;
  status: number;
}

export function concernFilterForApi(f: ConcernFilterDto): Record<string, unknown> {
  const o: Record<string, unknown> = {};
  if (f.schoolID != null && f.schoolID > 0) o['schoolID'] = f.schoolID;
  if (f.academicYearID != null && f.academicYearID > 0) o['academicYearID'] = f.academicYearID;
  if (f.status != null && f.status >= 0) o['status'] = f.status;
  if (f.submitterEmployeeProfileID != null && f.submitterEmployeeProfileID > 0)
    o['submitterEmployeeProfileID'] = f.submitterEmployeeProfileID;
  return o;
}

export function concernCategoriesFilterForApi(schoolID: number, categoryKind?: number | null): Record<string, unknown> {
  const o: Record<string, unknown> = { schoolID };
  if (categoryKind != null && categoryKind >= 0) o['categoryKind'] = categoryKind;
  return o;
}

export function complaintWriteForApi(d: ComplaintWriteDto): Record<string, unknown> {
  const o: Record<string, unknown> = {
    schoolID: d.schoolID,
    concernCategoryID: d.concernCategoryID,
    submitterEmployeeProfileID: d.submitterEmployeeProfileID,
    title: d.title,
    status: d.status,
  };
  if (d.academicYearID != null && d.academicYearID > 0) o['academicYearID'] = d.academicYearID;
  if (d.details != null && d.details !== '') o['details'] = d.details;
  if (d.assignedToEmployeeProfileID != null && d.assignedToEmployeeProfileID > 0)
    o['assignedToEmployeeProfileID'] = d.assignedToEmployeeProfileID;
  return o;
}

export function suggestionWriteForApi(d: SuggestionWriteDto): Record<string, unknown> {
  const o: Record<string, unknown> = {
    schoolID: d.schoolID,
    concernCategoryID: d.concernCategoryID,
    submitterEmployeeProfileID: d.submitterEmployeeProfileID,
    title: d.title,
    status: d.status,
  };
  if (d.academicYearID != null && d.academicYearID > 0) o['academicYearID'] = d.academicYearID;
  if (d.details != null && d.details !== '') o['details'] = d.details;
  if (d.assignedToEmployeeProfileID != null && d.assignedToEmployeeProfileID > 0)
    o['assignedToEmployeeProfileID'] = d.assignedToEmployeeProfileID;
  return o;
}
