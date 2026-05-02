export enum StrategicGoalStatus {
  Draft = 0,
  Active = 1,
  Achieved = 2,
  Superseded = 3,
  Archived = 4,
  Cancelled = 5,
}

export enum AnnualGoalStatus {
  Draft = 0,
  Active = 1,
  Completed = 2,
  Cancelled = 3,
}

export enum OperationalPlanStatus {
  Draft = 0,
  Active = 1,
  OnHold = 2,
  Completed = 3,
  Cancelled = 4,
}

export enum PlanTaskStatus {
  NotStarted = 0,
  InProgress = 1,
  Blocked = 2,
  Completed = 3,
  Cancelled = 4,
}

export enum DepartmentGoalStatus {
  Draft = 0,
  Active = 1,
  Achieved = 2,
  Cancelled = 3,
}

export interface StrategicGoalFilterDto {
  schoolID?: number | null;
  status?: number | null;
}

export interface StrategicGoalListItemDto {
  strategicGoalID: number;
  schoolID: number;
  referenceCode?: string | null;
  title: string;
  status: number;
  sortOrder: number;
  effectiveFromUtc?: string | null;
  effectiveToUtc?: string | null;
  updatedAtUtc: string;
}

export interface StrategicGoalDetailDto extends StrategicGoalListItemDto {
  details?: string | null;
  createdAtUtc: string;
}

export interface StrategicGoalWriteDto {
  schoolID: number;
  referenceCode?: string | null;
  title: string;
  details?: string | null;
  status: number;
  sortOrder: number;
  effectiveFromUtc?: string | null;
  effectiveToUtc?: string | null;
}

export interface AnnualGoalFilterDto {
  schoolID?: number | null;
  academicYearID?: number | null;
  strategicGoalID?: number | null;
  status?: number | null;
}

export interface AnnualGoalListItemDto {
  annualGoalID: number;
  schoolID: number;
  academicYearID: number;
  strategicGoalID?: number | null;
  strategicGoalTitle?: string | null;
  title: string;
  status: number;
  sortOrder: number;
  operationalPlanCount: number;
  updatedAtUtc: string;
}

export interface PlanProgressUpdateReadDto {
  planProgressUpdateID: number;
  planTaskID: number;
  note?: string | null;
  progressPercent?: number | null;
  authorEmployeeProfileID?: number | null;
  authorName?: string | null;
  createdAtUtc: string;
}

export interface PlanTaskReadDto {
  planTaskID: number;
  operationalPlanID: number;
  title: string;
  details?: string | null;
  status: number;
  sortOrder: number;
  progressPercent: number;
  dueAtUtc?: string | null;
  assignedToEmployeeProfileID?: number | null;
  assignedToName?: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
  progressUpdates: PlanProgressUpdateReadDto[];
}

export interface OperationalPlanReadDto {
  operationalPlanID: number;
  annualGoalID: number;
  title: string;
  details?: string | null;
  status: number;
  sortOrder: number;
  startDateUtc?: string | null;
  endDateUtc?: string | null;
  ownerEmployeeProfileID?: number | null;
  ownerName?: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
  tasks: PlanTaskReadDto[];
}

export interface AnnualGoalDetailDto extends AnnualGoalListItemDto {
  details?: string | null;
  createdAtUtc: string;
  operationalPlans: OperationalPlanReadDto[];
}

export interface PlanProgressUpdateWriteDto {
  note?: string | null;
  progressPercent?: number | null;
  authorEmployeeProfileID?: number | null;
}

export interface PlanTaskWriteDto {
  title: string;
  details?: string | null;
  status: number;
  sortOrder: number;
  progressPercent: number;
  dueAtUtc?: string | null;
  assignedToEmployeeProfileID?: number | null;
  progressUpdates: PlanProgressUpdateWriteDto[];
}

export interface OperationalPlanWriteDto {
  title: string;
  details?: string | null;
  status: number;
  sortOrder: number;
  startDateUtc?: string | null;
  endDateUtc?: string | null;
  ownerEmployeeProfileID?: number | null;
  tasks: PlanTaskWriteDto[];
}

export interface AnnualGoalWriteDto {
  schoolID: number;
  academicYearID: number;
  strategicGoalID?: number | null;
  title: string;
  details?: string | null;
  status: number;
  sortOrder: number;
  operationalPlans: OperationalPlanWriteDto[];
}

export interface DepartmentGoalFilterDto {
  schoolID?: number | null;
  academicYearID?: number | null;
  status?: number | null;
}

export interface DepartmentGoalListItemDto {
  departmentGoalID: number;
  schoolID: number;
  academicYearID?: number | null;
  strategicGoalID?: number | null;
  annualGoalID?: number | null;
  departmentName: string;
  title: string;
  status: number;
  sortOrder: number;
  updatedAtUtc: string;
}

export interface DepartmentGoalDetailDto extends DepartmentGoalListItemDto {
  details?: string | null;
  ownerEmployeeProfileID?: number | null;
  ownerName?: string | null;
  createdAtUtc: string;
}

export interface DepartmentGoalWriteDto {
  schoolID: number;
  academicYearID?: number | null;
  strategicGoalID?: number | null;
  annualGoalID?: number | null;
  departmentName: string;
  title: string;
  details?: string | null;
  status: number;
  sortOrder: number;
  ownerEmployeeProfileID?: number | null;
}

export function strategicGoalFilterForApi(f: StrategicGoalFilterDto): Record<string, unknown> {
  const o: Record<string, unknown> = {};
  if (f.schoolID != null && f.schoolID > 0) o['schoolID'] = f.schoolID;
  if (f.status != null && f.status >= 0) o['status'] = f.status;
  return o;
}

export function annualGoalFilterForApi(f: AnnualGoalFilterDto): Record<string, unknown> {
  const o: Record<string, unknown> = {};
  if (f.schoolID != null && f.schoolID > 0) o['schoolID'] = f.schoolID;
  if (f.academicYearID != null && f.academicYearID > 0) o['academicYearID'] = f.academicYearID;
  if (f.strategicGoalID != null && f.strategicGoalID > 0) o['strategicGoalID'] = f.strategicGoalID;
  if (f.status != null && f.status >= 0) o['status'] = f.status;
  return o;
}

export function departmentGoalFilterForApi(f: DepartmentGoalFilterDto): Record<string, unknown> {
  const o: Record<string, unknown> = {};
  if (f.schoolID != null && f.schoolID > 0) o['schoolID'] = f.schoolID;
  if (f.academicYearID != null && f.academicYearID > 0) o['academicYearID'] = f.academicYearID;
  if (f.status != null && f.status >= 0) o['status'] = f.status;
  return o;
}

export function strategicGoalWriteForApi(d: StrategicGoalWriteDto): Record<string, unknown> {
  const o: Record<string, unknown> = {
    schoolID: d.schoolID,
    title: d.title,
    status: d.status,
    sortOrder: d.sortOrder,
  };
  if (d.referenceCode != null && d.referenceCode !== '') o['referenceCode'] = d.referenceCode;
  if (d.details != null && d.details !== '') o['details'] = d.details;
  if (d.effectiveFromUtc != null && d.effectiveFromUtc !== '') o['effectiveFromUtc'] = d.effectiveFromUtc;
  if (d.effectiveToUtc != null && d.effectiveToUtc !== '') o['effectiveToUtc'] = d.effectiveToUtc;
  return o;
}

export function annualGoalWriteForApi(d: AnnualGoalWriteDto): Record<string, unknown> {
  return {
    schoolID: d.schoolID,
    academicYearID: d.academicYearID,
    strategicGoalID: d.strategicGoalID != null && d.strategicGoalID > 0 ? d.strategicGoalID : null,
    title: d.title,
    details: d.details ?? null,
    status: d.status,
    sortOrder: d.sortOrder,
    operationalPlans: (d.operationalPlans ?? []).map((p) => ({
      title: p.title,
      details: p.details ?? null,
      status: p.status,
      sortOrder: p.sortOrder,
      startDateUtc: p.startDateUtc ?? null,
      endDateUtc: p.endDateUtc ?? null,
      ownerEmployeeProfileID: p.ownerEmployeeProfileID ?? null,
      tasks: (p.tasks ?? []).map((t) => ({
        title: t.title,
        details: t.details ?? null,
        status: t.status,
        sortOrder: t.sortOrder,
        progressPercent: t.progressPercent,
        dueAtUtc: t.dueAtUtc ?? null,
        assignedToEmployeeProfileID: t.assignedToEmployeeProfileID ?? null,
        progressUpdates: (t.progressUpdates ?? []).map((u) => ({
          note: u.note ?? null,
          progressPercent: u.progressPercent ?? null,
          authorEmployeeProfileID: u.authorEmployeeProfileID ?? null,
        })),
      })),
    })),
  };
}

export function departmentGoalWriteForApi(d: DepartmentGoalWriteDto): Record<string, unknown> {
  return {
    schoolID: d.schoolID,
    academicYearID: d.academicYearID != null && d.academicYearID > 0 ? d.academicYearID : null,
    strategicGoalID: d.strategicGoalID != null && d.strategicGoalID > 0 ? d.strategicGoalID : null,
    annualGoalID: d.annualGoalID != null && d.annualGoalID > 0 ? d.annualGoalID : null,
    departmentName: d.departmentName,
    title: d.title,
    details: d.details ?? null,
    status: d.status,
    sortOrder: d.sortOrder,
    ownerEmployeeProfileID: d.ownerEmployeeProfileID ?? null,
  };
}
