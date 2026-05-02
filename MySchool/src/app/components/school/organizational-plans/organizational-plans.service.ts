import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map } from 'rxjs/operators';
import type { Observable } from 'rxjs';

import { BackendAspService } from 'app/ASP.NET/backend-asp.service';
import { ApiResponse } from 'app/core/models/response.model';

import {
  AnnualGoalDetailDto,
  AnnualGoalFilterDto,
  AnnualGoalListItemDto,
  AnnualGoalWriteDto,
  annualGoalFilterForApi,
  annualGoalWriteForApi,
  DepartmentGoalDetailDto,
  DepartmentGoalFilterDto,
  DepartmentGoalListItemDto,
  DepartmentGoalWriteDto,
  departmentGoalFilterForApi,
  departmentGoalWriteForApi,
  OperationalPlanReadDto,
  PlanProgressUpdateReadDto,
  PlanTaskReadDto,
  StrategicGoalDetailDto,
  StrategicGoalFilterDto,
  StrategicGoalListItemDto,
  StrategicGoalWriteDto,
  strategicGoalFilterForApi,
  strategicGoalWriteForApi,
} from './organizational-plans.models';

function unwrap<T>(body: ApiResponse<T> | Record<string, unknown>): T {
  const b = body as Record<string, unknown>;
  const ok = (b['isSuccess'] ?? b['IsSuccess']) !== false;
  const errs = (b['errorMasseges'] ?? b['ErrorMasseges']) as string[] | undefined;
  if (!ok && errs?.length) {
    throw new Error(errs.join('; '));
  }
  return (b['result'] ?? b['Result']) as T;
}

export function readOrganizationalPlanHttpError(err: unknown): string {
  const e = err as HttpErrorResponse;
  const msgs = e?.error?.errorMasseges ?? e?.error?.ErrorMasseges;
  if (Array.isArray(msgs) && msgs.length) return msgs.join('; ');
  if (typeof e?.error?.message === 'string') return e.error.message;
  if (typeof e?.message === 'string') return e.message;
  return 'Request failed';
}

function num(raw: Record<string, unknown>, ...keys: string[]): number {
  for (const k of keys) {
    if (raw[k] != null) return Number(raw[k]);
  }
  return 0;
}

function str(raw: Record<string, unknown>, ...keys: string[]): string {
  for (const k of keys) {
    if (raw[k] != null) return String(raw[k]);
  }
  return '';
}

function normStrategicListItem(raw: Record<string, unknown>): StrategicGoalListItemDto {
  return {
    strategicGoalID: num(raw, 'strategicGoalID', 'StrategicGoalID'),
    schoolID: num(raw, 'schoolID', 'SchoolID'),
    referenceCode: (raw['referenceCode'] ?? raw['ReferenceCode']) as string | null | undefined,
    title: str(raw, 'title', 'Title'),
    status: num(raw, 'status', 'Status'),
    sortOrder: num(raw, 'sortOrder', 'SortOrder'),
    effectiveFromUtc: raw['effectiveFromUtc'] != null ? String(raw['effectiveFromUtc'] ?? raw['EffectiveFromUtc']) : null,
    effectiveToUtc: raw['effectiveToUtc'] != null ? String(raw['effectiveToUtc'] ?? raw['EffectiveToUtc']) : null,
    updatedAtUtc: str(raw, 'updatedAtUtc', 'UpdatedAtUtc'),
  };
}

function normStrategicDetail(raw: Record<string, unknown>): StrategicGoalDetailDto {
  const base = normStrategicListItem(raw);
  return {
    ...base,
    details: (raw['details'] ?? raw['Details']) as string | null | undefined,
    createdAtUtc: str(raw, 'createdAtUtc', 'CreatedAtUtc'),
  };
}

function normProgressUpdate(raw: Record<string, unknown>): PlanProgressUpdateReadDto {
  return {
    planProgressUpdateID: num(raw, 'planProgressUpdateID', 'PlanProgressUpdateID'),
    planTaskID: num(raw, 'planTaskID', 'PlanTaskID'),
    note: (raw['note'] ?? raw['Note']) as string | null | undefined,
    progressPercent:
      raw['progressPercent'] != null || raw['ProgressPercent'] != null
        ? Number(raw['progressPercent'] ?? raw['ProgressPercent'])
        : null,
    authorEmployeeProfileID:
      raw['authorEmployeeProfileID'] != null ? num(raw, 'authorEmployeeProfileID', 'AuthorEmployeeProfileID') : null,
    authorName: (raw['authorName'] ?? raw['AuthorName']) as string | null | undefined,
    createdAtUtc: str(raw, 'createdAtUtc', 'CreatedAtUtc'),
  };
}

function normTask(raw: Record<string, unknown>): PlanTaskReadDto {
  const pus = (raw['progressUpdates'] ?? raw['ProgressUpdates']) as Record<string, unknown>[] | undefined;
  return {
    planTaskID: num(raw, 'planTaskID', 'PlanTaskID'),
    operationalPlanID: num(raw, 'operationalPlanID', 'OperationalPlanID'),
    title: str(raw, 'title', 'Title'),
    details: (raw['details'] ?? raw['Details']) as string | null | undefined,
    status: num(raw, 'status', 'Status'),
    sortOrder: num(raw, 'sortOrder', 'SortOrder'),
    progressPercent: num(raw, 'progressPercent', 'ProgressPercent'),
    dueAtUtc: raw['dueAtUtc'] != null ? String(raw['dueAtUtc'] ?? raw['DueAtUtc']) : null,
    assignedToEmployeeProfileID:
      raw['assignedToEmployeeProfileID'] != null ? num(raw, 'assignedToEmployeeProfileID', 'AssignedToEmployeeProfileID') : null,
    assignedToName: (raw['assignedToName'] ?? raw['AssignedToName']) as string | null | undefined,
    createdAtUtc: str(raw, 'createdAtUtc', 'CreatedAtUtc'),
    updatedAtUtc: str(raw, 'updatedAtUtc', 'UpdatedAtUtc'),
    progressUpdates: Array.isArray(pus) ? pus.map((u) => normProgressUpdate(u as Record<string, unknown>)) : [],
  };
}

function normOperationalPlan(raw: Record<string, unknown>): OperationalPlanReadDto {
  const ts = (raw['tasks'] ?? raw['Tasks']) as Record<string, unknown>[] | undefined;
  return {
    operationalPlanID: num(raw, 'operationalPlanID', 'OperationalPlanID'),
    annualGoalID: num(raw, 'annualGoalID', 'AnnualGoalID'),
    title: str(raw, 'title', 'Title'),
    details: (raw['details'] ?? raw['Details']) as string | null | undefined,
    status: num(raw, 'status', 'Status'),
    sortOrder: num(raw, 'sortOrder', 'SortOrder'),
    startDateUtc: raw['startDateUtc'] != null ? String(raw['startDateUtc'] ?? raw['StartDateUtc']) : null,
    endDateUtc: raw['endDateUtc'] != null ? String(raw['endDateUtc'] ?? raw['EndDateUtc']) : null,
    ownerEmployeeProfileID: raw['ownerEmployeeProfileID'] != null ? num(raw, 'ownerEmployeeProfileID', 'OwnerEmployeeProfileID') : null,
    ownerName: (raw['ownerName'] ?? raw['OwnerName']) as string | null | undefined,
    createdAtUtc: str(raw, 'createdAtUtc', 'CreatedAtUtc'),
    updatedAtUtc: str(raw, 'updatedAtUtc', 'UpdatedAtUtc'),
    tasks: Array.isArray(ts) ? ts.map((t) => normTask(t as Record<string, unknown>)) : [],
  };
}

function normAnnualListItem(raw: Record<string, unknown>): AnnualGoalListItemDto {
  return {
    annualGoalID: num(raw, 'annualGoalID', 'AnnualGoalID'),
    schoolID: num(raw, 'schoolID', 'SchoolID'),
    academicYearID: num(raw, 'academicYearID', 'AcademicYearID'),
    strategicGoalID: raw['strategicGoalID'] != null ? num(raw, 'strategicGoalID', 'StrategicGoalID') : null,
    strategicGoalTitle: (raw['strategicGoalTitle'] ?? raw['StrategicGoalTitle']) as string | null | undefined,
    title: str(raw, 'title', 'Title'),
    status: num(raw, 'status', 'Status'),
    sortOrder: num(raw, 'sortOrder', 'SortOrder'),
    operationalPlanCount: num(raw, 'operationalPlanCount', 'OperationalPlanCount'),
    updatedAtUtc: str(raw, 'updatedAtUtc', 'UpdatedAtUtc'),
  };
}

function normAnnualDetail(raw: Record<string, unknown>): AnnualGoalDetailDto {
  const base = normAnnualListItem(raw);
  const ops = (raw['operationalPlans'] ?? raw['OperationalPlans']) as Record<string, unknown>[] | undefined;
  return {
    ...base,
    details: (raw['details'] ?? raw['Details']) as string | null | undefined,
    createdAtUtc: str(raw, 'createdAtUtc', 'CreatedAtUtc'),
    operationalPlans: Array.isArray(ops) ? ops.map((o) => normOperationalPlan(o as Record<string, unknown>)) : [],
  };
}

function normDeptListItem(raw: Record<string, unknown>): DepartmentGoalListItemDto {
  return {
    departmentGoalID: num(raw, 'departmentGoalID', 'DepartmentGoalID'),
    schoolID: num(raw, 'schoolID', 'SchoolID'),
    academicYearID: raw['academicYearID'] != null ? num(raw, 'academicYearID', 'AcademicYearID') : null,
    strategicGoalID: raw['strategicGoalID'] != null ? num(raw, 'strategicGoalID', 'StrategicGoalID') : null,
    annualGoalID: raw['annualGoalID'] != null ? num(raw, 'annualGoalID', 'AnnualGoalID') : null,
    departmentName: str(raw, 'departmentName', 'DepartmentName'),
    title: str(raw, 'title', 'Title'),
    status: num(raw, 'status', 'Status'),
    sortOrder: num(raw, 'sortOrder', 'SortOrder'),
    updatedAtUtc: str(raw, 'updatedAtUtc', 'UpdatedAtUtc'),
  };
}

function normDeptDetail(raw: Record<string, unknown>): DepartmentGoalDetailDto {
  const base = normDeptListItem(raw);
  return {
    ...base,
    details: (raw['details'] ?? raw['Details']) as string | null | undefined,
    ownerEmployeeProfileID: raw['ownerEmployeeProfileID'] != null ? num(raw, 'ownerEmployeeProfileID', 'OwnerEmployeeProfileID') : null,
    ownerName: (raw['ownerName'] ?? raw['OwnerName']) as string | null | undefined,
    createdAtUtc: str(raw, 'createdAtUtc', 'CreatedAtUtc'),
  };
}

@Injectable({ providedIn: 'root' })
export class OrganizationalPlansService {
  private readonly http = inject(HttpClient);
  private readonly api = inject(BackendAspService);

  private root(path: string): string {
    return `${this.api.baseUrl}/organizational-plans${path}`;
  }

  listStrategicGoals(filter: StrategicGoalFilterDto): Observable<StrategicGoalListItemDto[]> {
    return this.http.post<ApiResponse<unknown>>(this.root('/strategic-goals/list'), strategicGoalFilterForApi(filter)).pipe(
      map((r) => (unwrap<unknown[]>(r) ?? []).map((x) => normStrategicListItem(x as Record<string, unknown>))),
    );
  }

  getStrategicGoal(id: number): Observable<StrategicGoalDetailDto> {
    return this.http.get<ApiResponse<unknown>>(this.root(`/strategic-goals/${id}`)).pipe(
      map((r) => normStrategicDetail(unwrap<Record<string, unknown>>(r) as Record<string, unknown>)),
    );
  }

  createStrategicGoal(dto: StrategicGoalWriteDto): Observable<number> {
    return this.http
      .post<ApiResponse<number>>(this.root('/strategic-goals'), strategicGoalWriteForApi(dto))
      .pipe(map((res) => Number(unwrap<number>(res))));
  }

  updateStrategicGoal(id: number, dto: StrategicGoalWriteDto): Observable<number> {
    return this.http
      .put<ApiResponse<number>>(this.root(`/strategic-goals/${id}`), strategicGoalWriteForApi(dto))
      .pipe(map((res) => Number(unwrap<number>(res))));
  }

  listAnnualGoals(filter: AnnualGoalFilterDto): Observable<AnnualGoalListItemDto[]> {
    return this.http.post<ApiResponse<unknown>>(this.root('/annual-goals/list'), annualGoalFilterForApi(filter)).pipe(
      map((r) => (unwrap<unknown[]>(r) ?? []).map((x) => normAnnualListItem(x as Record<string, unknown>))),
    );
  }

  getAnnualGoal(id: number): Observable<AnnualGoalDetailDto> {
    return this.http.get<ApiResponse<unknown>>(this.root(`/annual-goals/${id}`)).pipe(
      map((r) => normAnnualDetail(unwrap<Record<string, unknown>>(r) as Record<string, unknown>)),
    );
  }

  createAnnualGoal(dto: AnnualGoalWriteDto): Observable<number> {
    return this.http
      .post<ApiResponse<number>>(this.root('/annual-goals'), annualGoalWriteForApi(dto))
      .pipe(map((res) => Number(unwrap<number>(res))));
  }

  updateAnnualGoal(id: number, dto: AnnualGoalWriteDto): Observable<number> {
    return this.http
      .put<ApiResponse<number>>(this.root(`/annual-goals/${id}`), annualGoalWriteForApi(dto))
      .pipe(map((res) => Number(unwrap<number>(res))));
  }

  listDepartmentGoals(filter: DepartmentGoalFilterDto): Observable<DepartmentGoalListItemDto[]> {
    return this.http.post<ApiResponse<unknown>>(this.root('/department-goals/list'), departmentGoalFilterForApi(filter)).pipe(
      map((r) => (unwrap<unknown[]>(r) ?? []).map((x) => normDeptListItem(x as Record<string, unknown>))),
    );
  }

  getDepartmentGoal(id: number): Observable<DepartmentGoalDetailDto> {
    return this.http.get<ApiResponse<unknown>>(this.root(`/department-goals/${id}`)).pipe(
      map((r) => normDeptDetail(unwrap<Record<string, unknown>>(r) as Record<string, unknown>)),
    );
  }

  createDepartmentGoal(dto: DepartmentGoalWriteDto): Observable<number> {
    return this.http
      .post<ApiResponse<number>>(this.root('/department-goals'), departmentGoalWriteForApi(dto))
      .pipe(map((res) => Number(unwrap<number>(res))));
  }

  updateDepartmentGoal(id: number, dto: DepartmentGoalWriteDto): Observable<number> {
    return this.http
      .put<ApiResponse<number>>(this.root(`/department-goals/${id}`), departmentGoalWriteForApi(dto))
      .pipe(map((res) => Number(unwrap<number>(res))));
  }
}
