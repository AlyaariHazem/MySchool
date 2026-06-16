/** Mirrors backend `ActivityRequestStatus`. */
export enum ActivityRequestStatus {
  Draft = 0,
  Submitted = 1,
  InReview = 2,
  Approved = 3,
  Rejected = 4,
  InProgress = 5,
  Completed = 6,
  Cancelled = 7,
}

/** Mirrors backend `ActivityApprovalDecision`. */
export enum ActivityApprovalDecision {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
  Skipped = 3,
}

/** Mirrors backend `ActivityExecutionStatus`. */
export enum ActivityExecutionStatus {
  Pending = 0,
  InProgress = 1,
  WaitingExternal = 2,
  Completed = 3,
  Blocked = 4,
  Cancelled = 5,
}

export interface ActivityFilterDto {
  schoolID?: number | null;
  academicYearID?: number | null;
  status?: number | null;
  employeeProfileID?: number | null;
}

export interface ActivityListItemDto {
  activityRequestID: number;
  schoolID: number;
  academicYearID: number;
  employeeProfileID: number;
  employeeName: string;
  title: string;
  status: number;
  submittedAtUtc: string;
  updatedAtUtc: string;
  resolvedAtUtc?: string | null;
}

export interface ActivityApprovalReadDto {
  activityApprovalID: number;
  activityRequestID: number;
  approverEmployeeProfileID: number;
  approverName: string;
  sortOrder: number;
  decision: number;
  comment?: string | null;
  decidedAtUtc?: string | null;
  createdAtUtc: string;
}

export interface ActivityExecutionReadDto {
  activityExecutionID: number;
  activityRequestID: number;
  status: number;
  notes?: string | null;
  progressPercent: number;
  dueAtUtc?: string | null;
  executedAtUtc?: string | null;
  updatedAtUtc: string;
  responsibleEmployeeProfileID?: number | null;
  responsibleName?: string | null;
}

export interface ActivityEvaluationReadDto {
  activityEvaluationID: number;
  activityRequestID: number;
  evaluatorEmployeeProfileID: number;
  evaluatorName: string;
  score: number;
  feedback?: string | null;
  createdAtUtc: string;
}

export interface ActivityPointsReadDto {
  activityPointsID: number;
  activityRequestID: number;
  points: number;
  reason?: string | null;
  awardedByEmployeeProfileID: number;
  awardedByName: string;
  awardedAtUtc: string;
}

export interface ActivityDetailDto extends ActivityListItemDto {
  details?: string | null;
  approvals: ActivityApprovalReadDto[];
  executions: ActivityExecutionReadDto[];
  evaluations: ActivityEvaluationReadDto[];
  points: ActivityPointsReadDto[];
}

export interface ActivityApprovalWriteDto {
  approverEmployeeProfileID: number;
  sortOrder: number;
  decision: number;
  comment?: string | null;
  decidedAtUtc?: string | null;
}

export interface ActivityExecutionWriteDto {
  status: number;
  notes?: string | null;
  progressPercent: number;
  dueAtUtc?: string | null;
  executedAtUtc?: string | null;
  responsibleEmployeeProfileID?: number | null;
}

export interface ActivityEvaluationWriteDto {
  evaluatorEmployeeProfileID: number;
  score: number;
  feedback?: string | null;
}

export interface ActivityPointsWriteDto {
  points: number;
  reason?: string | null;
  awardedByEmployeeProfileID: number;
}

export interface ActivityRequestWriteDto {
  schoolID: number;
  academicYearID?: number | null;
  employeeProfileID: number;
  title: string;
  details?: string | null;
  status: number;
  approvals: ActivityApprovalWriteDto[];
  executions: ActivityExecutionWriteDto[];
  evaluations: ActivityEvaluationWriteDto[];
  points: ActivityPointsWriteDto[];
}

export function activityFilterForApi(f: ActivityFilterDto): Record<string, unknown> {
  const o: Record<string, unknown> = {};
  if (f.schoolID != null && f.schoolID > 0) o['schoolID'] = f.schoolID;
  if (f.academicYearID != null && f.academicYearID > 0) o['academicYearID'] = f.academicYearID;
  if (f.status != null && f.status >= 0) o['status'] = f.status;
  if (f.employeeProfileID != null && f.employeeProfileID > 0) o['employeeProfileID'] = f.employeeProfileID;
  return o;
}

export function activityWriteForApi(d: ActivityRequestWriteDto): Record<string, unknown> {
  const o: Record<string, unknown> = {
    schoolID: d.schoolID,
    employeeProfileID: d.employeeProfileID,
    title: d.title,
    status: d.status,
    approvals: (d.approvals ?? []).map((a) => ({
      approverEmployeeProfileID: a.approverEmployeeProfileID,
      sortOrder: a.sortOrder,
      decision: a.decision,
      comment: a.comment ?? null,
      decidedAtUtc: a.decidedAtUtc ?? null,
    })),
    executions: (d.executions ?? []).map((e) => ({
      status: e.status,
      notes: e.notes ?? null,
      progressPercent: e.progressPercent,
      dueAtUtc: e.dueAtUtc ?? null,
      executedAtUtc: e.executedAtUtc ?? null,
      responsibleEmployeeProfileID: e.responsibleEmployeeProfileID ?? null,
    })),
    evaluations: (d.evaluations ?? []).map((ev) => ({
      evaluatorEmployeeProfileID: ev.evaluatorEmployeeProfileID,
      score: ev.score,
      feedback: ev.feedback ?? null,
    })),
    points: (d.points ?? []).map((p) => ({
      points: p.points,
      reason: p.reason ?? null,
      awardedByEmployeeProfileID: p.awardedByEmployeeProfileID,
    })),
  };
  if (d.academicYearID != null && d.academicYearID > 0) o['academicYearID'] = d.academicYearID;
  if (d.details != null && d.details !== '') o['details'] = d.details;
  return o;
}
