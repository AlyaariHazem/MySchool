/** Mirrors backend `EmployeeRequestCategory` (int). */
export enum EmployeeRequestCategory {
  Tools = 0,
  Advance = 1,
  Support = 2,
}

/** Mirrors backend `EmployeeRequestStatus` (int). */
export enum EmployeeRequestStatus {
  Draft = 0,
  Submitted = 1,
  InApproval = 2,
  Approved = 3,
  Rejected = 4,
  InExecution = 5,
  Completed = 6,
  Cancelled = 7,
}

/** Mirrors backend `RequestApprovalDecision` (int). */
export enum RequestApprovalDecision {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
  Skipped = 3,
}

/** Mirrors backend `RequestExecutionStatus` (int). */
export enum RequestExecutionStatus {
  Pending = 0,
  InProgress = 1,
  WaitingExternal = 2,
  Completed = 3,
  Blocked = 4,
  Cancelled = 5,
}

export interface EmployeeRequestFilterDto {
  schoolID?: number | null;
  academicYearID?: number | null;
  employeeProfileID?: number | null;
  status?: number | null;
}

export interface EmployeeRequestTypeListItemDto {
  requestTypeID: number;
  schoolID: number;
  code: string;
  category: number;
  name: string;
  nameAr?: string | null;
  description?: string | null;
  isActive: boolean;
}

export interface EmployeeRequestListItemDto {
  employeeRequestID: number;
  schoolID: number;
  academicYearID: number;
  employeeProfileID: number;
  employeeName: string;
  requestTypeID: number;
  requestTypeCode: string;
  requestTypeCategory: number;
  requestTypeName: string;
  requestTypeNameAr?: string | null;
  title: string;
  status: number;
  requestedAmount?: number | null;
  submittedAtUtc: string;
  updatedAtUtc: string;
  resolvedAtUtc?: string | null;
}

export interface EmployeeRequestApprovalReadDto {
  requestApprovalStepID: number;
  employeeRequestID: number;
  approverEmployeeProfileID: number;
  approverName: string;
  stepOrder: number;
  decision: number;
  comment?: string | null;
  decidedAtUtc?: string | null;
  createdAtUtc: string;
}

export interface EmployeeRequestExecutionReadDto {
  requestExecutionID: number;
  employeeRequestID: number;
  status: number;
  notes?: string | null;
  progressPercent: number;
  dueAtUtc?: string | null;
  executedAtUtc?: string | null;
  updatedAtUtc: string;
  responsibleEmployeeProfileID?: number | null;
  responsibleName?: string | null;
}

export interface EmployeeRequestDailySummaryReadDto {
  requestDailySummaryID: number;
  employeeRequestID: number;
  summaryDate: string;
  summary: string;
  progressPercent?: number | null;
  isFinalForDay: boolean;
  createdByEmployeeProfileID?: number | null;
  createdByName?: string | null;
  createdAtUtc: string;
}

export interface EmployeeRequestDetailDto extends EmployeeRequestListItemDto {
  details?: string | null;
  approvalSteps: EmployeeRequestApprovalReadDto[];
  executions: EmployeeRequestExecutionReadDto[];
  dailySummaries: EmployeeRequestDailySummaryReadDto[];
}

export interface EmployeeRequestWriteDto {
  schoolID: number;
  academicYearID?: number | null;
  employeeProfileID: number;
  requestTypeID: number;
  title: string;
  details?: string | null;
  requestedAmount?: number | null;
  status: number;
}

export interface EmployeeRequestExecutionWriteDto {
  status: number;
  notes?: string | null;
  progressPercent: number;
  dueAtUtc?: string | null;
  responsibleEmployeeProfileID?: number | null;
}

export interface EmployeeRequestDailySummaryWriteDto {
  summaryDate: string;
  summary: string;
  progressPercent?: number | null;
  isFinalForDay: boolean;
  createdByEmployeeProfileID?: number | null;
}

export interface EmployeeRequestApprovalStepWriteDto {
  approverEmployeeProfileID: number;
  stepOrder: number;
}

export interface EmployeeRequestApprovalDecideDto {
  decision: number;
  comment?: string | null;
}

export function employeeRequestFilterForApi(f: EmployeeRequestFilterDto): Record<string, unknown> {
  return {
    schoolID: f.schoolID ?? null,
    academicYearID: f.academicYearID ?? null,
    employeeProfileID: f.employeeProfileID ?? null,
    status: f.status ?? null,
  };
}

export function employeeRequestTypesFilterForApi(schoolID: number): Record<string, unknown> {
  return { schoolID };
}

export function employeeRequestWriteForApi(d: EmployeeRequestWriteDto): Record<string, unknown> {
  const body: Record<string, unknown> = {
    schoolID: d.schoolID,
    employeeProfileID: d.employeeProfileID,
    requestTypeID: d.requestTypeID,
    title: String(d.title ?? '').trim(),
    details: d.details != null && String(d.details).trim() !== '' ? String(d.details).trim() : null,
    status: d.status,
  };
  if (d.academicYearID != null && d.academicYearID > 0) {
    body['academicYearID'] = d.academicYearID;
  }
  if (d.requestedAmount != null && Number.isFinite(Number(d.requestedAmount))) {
    body['requestedAmount'] = d.requestedAmount;
  } else {
    body['requestedAmount'] = null;
  }
  return body;
}

export function employeeRequestExecutionWriteForApi(d: EmployeeRequestExecutionWriteDto): Record<string, unknown> {
  return {
    status: d.status,
    notes: d.notes != null && String(d.notes).trim() !== '' ? String(d.notes).trim() : null,
    progressPercent: d.progressPercent,
    dueAtUtc: d.dueAtUtc != null && String(d.dueAtUtc).trim() !== '' ? d.dueAtUtc : null,
    responsibleEmployeeProfileID:
      d.responsibleEmployeeProfileID != null && d.responsibleEmployeeProfileID > 0
        ? d.responsibleEmployeeProfileID
        : null,
  };
}

export function employeeRequestDailySummaryWriteForApi(d: EmployeeRequestDailySummaryWriteDto): Record<string, unknown> {
  return {
    summaryDate: d.summaryDate,
    summary: String(d.summary ?? '').trim(),
    progressPercent: d.progressPercent != null && d.progressPercent >= 0 ? d.progressPercent : null,
    isFinalForDay: Boolean(d.isFinalForDay),
    createdByEmployeeProfileID:
      d.createdByEmployeeProfileID != null && d.createdByEmployeeProfileID > 0
        ? d.createdByEmployeeProfileID
        : null,
  };
}

export function employeeRequestApprovalStepWriteForApi(d: EmployeeRequestApprovalStepWriteDto): Record<string, unknown> {
  return {
    approverEmployeeProfileID: d.approverEmployeeProfileID,
    stepOrder: d.stepOrder > 0 ? d.stepOrder : 0,
  };
}

export function employeeRequestApprovalDecideForApi(d: EmployeeRequestApprovalDecideDto): Record<string, unknown> {
  return {
    decision: d.decision,
    comment: d.comment != null && String(d.comment).trim() !== '' ? String(d.comment).trim() : null,
  };
}
