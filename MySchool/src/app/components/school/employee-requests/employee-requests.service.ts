import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map } from 'rxjs';
import type { Observable } from 'rxjs';

import { BackendAspService } from 'app/ASP.NET/backend-asp.service';
import { ApiResponse } from 'app/core/models/response.model';

import {
  EmployeeRequestApprovalDecideDto,
  EmployeeRequestApprovalStepWriteDto,
  EmployeeRequestDailySummaryWriteDto,
  EmployeeRequestDetailDto,
  EmployeeRequestExecutionWriteDto,
  EmployeeRequestFilterDto,
  EmployeeRequestListItemDto,
  EmployeeRequestTypeListItemDto,
  EmployeeRequestWriteDto,
  employeeRequestApprovalDecideForApi,
  employeeRequestApprovalStepWriteForApi,
  employeeRequestDailySummaryWriteForApi,
  employeeRequestExecutionWriteForApi,
  employeeRequestFilterForApi,
  employeeRequestTypesFilterForApi,
  employeeRequestWriteForApi,
} from './employee-requests.models';

function unwrap<T>(body: ApiResponse<T> | Record<string, unknown>): T {
  const b = body as Record<string, unknown>;
  const ok = (b['isSuccess'] ?? b['IsSuccess']) !== false;
  const errs = (b['errorMasseges'] ?? b['ErrorMasseges']) as string[] | undefined;
  if (!ok && errs?.length) {
    throw new Error(errs.join('; '));
  }
  return (b['result'] ?? b['Result']) as T;
}

export function readEmployeeRequestHttpError(err: unknown): string {
  const e = err as HttpErrorResponse;
  const msgs = e?.error?.errorMasseges ?? e?.error?.ErrorMasseges;
  if (Array.isArray(msgs) && msgs.length) return msgs.join('; ');
  if (typeof e?.error?.message === 'string') return e.error.message;
  if (typeof e?.message === 'string') return e.message;
  return 'Request failed';
}

function normalizeTypeRow(raw: Record<string, unknown>): EmployeeRequestTypeListItemDto {
  return {
    requestTypeID: Number(raw['requestTypeID'] ?? raw['RequestTypeID']),
    schoolID: Number(raw['schoolID'] ?? raw['SchoolID']),
    code: String(raw['code'] ?? raw['Code'] ?? ''),
    category: Number(raw['category'] ?? raw['Category'] ?? 0),
    name: String(raw['name'] ?? raw['Name'] ?? ''),
    nameAr: (raw['nameAr'] ?? raw['NameAr']) as string | null | undefined,
    description: (raw['description'] ?? raw['Description']) as string | null | undefined,
    isActive: Boolean(raw['isActive'] ?? raw['IsActive'] ?? true),
  };
}

function normalizeListRow(raw: Record<string, unknown>): EmployeeRequestListItemDto {
  return {
    employeeRequestID: Number(raw['employeeRequestID'] ?? raw['EmployeeRequestID']),
    schoolID: Number(raw['schoolID'] ?? raw['SchoolID']),
    academicYearID: Number(raw['academicYearID'] ?? raw['AcademicYearID']),
    employeeProfileID: Number(raw['employeeProfileID'] ?? raw['EmployeeProfileID']),
    employeeName: String(raw['employeeName'] ?? raw['EmployeeName'] ?? ''),
    requestTypeID: Number(raw['requestTypeID'] ?? raw['RequestTypeID']),
    requestTypeCode: String(raw['requestTypeCode'] ?? raw['RequestTypeCode'] ?? ''),
    requestTypeCategory: Number(raw['requestTypeCategory'] ?? raw['RequestTypeCategory'] ?? 0),
    requestTypeName: String(raw['requestTypeName'] ?? raw['RequestTypeName'] ?? ''),
    requestTypeNameAr: (raw['requestTypeNameAr'] ?? raw['RequestTypeNameAr']) as string | null | undefined,
    title: String(raw['title'] ?? raw['Title'] ?? ''),
    status: Number(raw['status'] ?? raw['Status'] ?? 0),
    requestedAmount:
      raw['requestedAmount'] != null && raw['requestedAmount'] !== ''
        ? Number(raw['requestedAmount'] ?? raw['RequestedAmount'])
        : null,
    submittedAtUtc: String(raw['submittedAtUtc'] ?? raw['SubmittedAtUtc'] ?? ''),
    updatedAtUtc: String(raw['updatedAtUtc'] ?? raw['UpdatedAtUtc'] ?? ''),
    resolvedAtUtc: raw['resolvedAtUtc'] != null ? String(raw['resolvedAtUtc'] ?? raw['ResolvedAtUtc']) : null,
  };
}

function normalizeApproval(raw: Record<string, unknown>): EmployeeRequestDetailDto['approvalSteps'][number] {
  return {
    requestApprovalStepID: Number(raw['requestApprovalStepID'] ?? raw['RequestApprovalStepID']),
    employeeRequestID: Number(raw['employeeRequestID'] ?? raw['EmployeeRequestID']),
    approverEmployeeProfileID: Number(raw['approverEmployeeProfileID'] ?? raw['ApproverEmployeeProfileID']),
    approverName: String(raw['approverName'] ?? raw['ApproverName'] ?? ''),
    stepOrder: Number(raw['stepOrder'] ?? raw['StepOrder'] ?? 0),
    decision: Number(raw['decision'] ?? raw['Decision'] ?? 0),
    comment: (raw['comment'] ?? raw['Comment']) as string | null | undefined,
    decidedAtUtc: raw['decidedAtUtc'] != null ? String(raw['decidedAtUtc'] ?? raw['DecidedAtUtc']) : null,
    createdAtUtc: String(raw['createdAtUtc'] ?? raw['CreatedAtUtc'] ?? ''),
  };
}

function normalizeExecution(raw: Record<string, unknown>): EmployeeRequestDetailDto['executions'][number] {
  return {
    requestExecutionID: Number(raw['requestExecutionID'] ?? raw['RequestExecutionID']),
    employeeRequestID: Number(raw['employeeRequestID'] ?? raw['EmployeeRequestID']),
    status: Number(raw['status'] ?? raw['Status'] ?? 0),
    notes: (raw['notes'] ?? raw['Notes']) as string | null | undefined,
    progressPercent: Number(raw['progressPercent'] ?? raw['ProgressPercent'] ?? 0),
    dueAtUtc: raw['dueAtUtc'] != null ? String(raw['dueAtUtc'] ?? raw['DueAtUtc']) : null,
    executedAtUtc: raw['executedAtUtc'] != null ? String(raw['executedAtUtc'] ?? raw['ExecutedAtUtc']) : null,
    updatedAtUtc: String(raw['updatedAtUtc'] ?? raw['UpdatedAtUtc'] ?? ''),
    responsibleEmployeeProfileID:
      raw['responsibleEmployeeProfileID'] != null ? Number(raw['responsibleEmployeeProfileID']) : null,
    responsibleName: (raw['responsibleName'] ?? raw['ResponsibleName']) as string | null | undefined,
  };
}

function normalizeSummary(raw: Record<string, unknown>): EmployeeRequestDetailDto['dailySummaries'][number] {
  return {
    requestDailySummaryID: Number(raw['requestDailySummaryID'] ?? raw['RequestDailySummaryID']),
    employeeRequestID: Number(raw['employeeRequestID'] ?? raw['EmployeeRequestID']),
    summaryDate: String(raw['summaryDate'] ?? raw['SummaryDate'] ?? ''),
    summary: String(raw['summary'] ?? raw['Summary'] ?? ''),
    progressPercent: raw['progressPercent'] != null ? Number(raw['progressPercent']) : null,
    isFinalForDay: Boolean(raw['isFinalForDay'] ?? raw['IsFinalForDay'] ?? false),
    createdByEmployeeProfileID:
      raw['createdByEmployeeProfileID'] != null ? Number(raw['createdByEmployeeProfileID']) : null,
    createdByName: (raw['createdByName'] ?? raw['CreatedByName']) as string | null | undefined,
    createdAtUtc: String(raw['createdAtUtc'] ?? raw['CreatedAtUtc'] ?? ''),
  };
}

function normalizeDetail(raw: Record<string, unknown>): EmployeeRequestDetailDto {
  const base = normalizeListRow(raw);
  const ap = (raw['approvalSteps'] ?? raw['ApprovalSteps']) as Record<string, unknown>[] | undefined;
  const ex = (raw['executions'] ?? raw['Executions']) as Record<string, unknown>[] | undefined;
  const sm = (raw['dailySummaries'] ?? raw['DailySummaries']) as Record<string, unknown>[] | undefined;
  return {
    ...base,
    details: (raw['details'] ?? raw['Details']) as string | null | undefined,
    approvalSteps: Array.isArray(ap) ? ap.map((x) => normalizeApproval(x as Record<string, unknown>)) : [],
    executions: Array.isArray(ex) ? ex.map((x) => normalizeExecution(x as Record<string, unknown>)) : [],
    dailySummaries: Array.isArray(sm) ? sm.map((x) => normalizeSummary(x as Record<string, unknown>)) : [],
  };
}

@Injectable({ providedIn: 'root' })
export class EmployeeRequestsService {
  private readonly http = inject(HttpClient);
  private readonly api = inject(BackendAspService);

  private root(path: string): string {
    return `${this.api.baseUrl}/employee-requests${path}`;
  }

  listTypes(schoolID: number): Observable<EmployeeRequestTypeListItemDto[]> {
    const body = employeeRequestTypesFilterForApi(schoolID);
    return this.http.post<ApiResponse<unknown>>(this.root('/types/list'), body).pipe(
      map((r) => {
        const rows = unwrap<unknown[]>(r) ?? [];
        return rows.map((x) => normalizeTypeRow(x as Record<string, unknown>));
      }),
    );
  }

  list(filter: EmployeeRequestFilterDto): Observable<EmployeeRequestListItemDto[]> {
    const body = employeeRequestFilterForApi(filter);
    return this.http.post<ApiResponse<unknown>>(this.root('/list'), body).pipe(
      map((r) => {
        const rows = unwrap<unknown[]>(r) ?? [];
        return rows.map((x) => normalizeListRow(x as Record<string, unknown>));
      }),
    );
  }

  getById(id: number): Observable<EmployeeRequestDetailDto> {
    return this.http.get<ApiResponse<unknown>>(this.root(`/${id}`)).pipe(
      map((r) => normalizeDetail(unwrap<Record<string, unknown>>(r) as Record<string, unknown>)),
    );
  }

  create(dto: EmployeeRequestWriteDto): Observable<number> {
    return this.http.post<ApiResponse<number>>(this.root(''), employeeRequestWriteForApi(dto)).pipe(
      map((res) => Number(unwrap<number>(res))),
    );
  }

  update(id: number, dto: EmployeeRequestWriteDto): Observable<number> {
    return this.http.put<ApiResponse<number>>(this.root(`/${id}`), employeeRequestWriteForApi(dto)).pipe(
      map((res) => Number(unwrap<number>(res))),
    );
  }

  addExecution(requestId: number, dto: EmployeeRequestExecutionWriteDto): Observable<number> {
    return this.http
      .post<ApiResponse<number>>(this.root(`/${requestId}/executions`), employeeRequestExecutionWriteForApi(dto))
      .pipe(map((res) => Number(unwrap<number>(res))));
  }

  addDailySummary(requestId: number, dto: EmployeeRequestDailySummaryWriteDto): Observable<number> {
    return this.http
      .post<ApiResponse<number>>(this.root(`/${requestId}/daily-summaries`), employeeRequestDailySummaryWriteForApi(dto))
      .pipe(map((res) => Number(unwrap<number>(res))));
  }

  addApprovalStep(requestId: number, dto: EmployeeRequestApprovalStepWriteDto): Observable<number> {
    return this.http
      .post<ApiResponse<number>>(this.root(`/${requestId}/approval-steps`), employeeRequestApprovalStepWriteForApi(dto))
      .pipe(map((res) => Number(unwrap<number>(res))));
  }

  decideApprovalStep(requestId: number, stepId: number, dto: EmployeeRequestApprovalDecideDto): Observable<void> {
    return this.http
      .post<ApiResponse<unknown>>(
        this.root(`/${requestId}/approval-steps/${stepId}/decide`),
        employeeRequestApprovalDecideForApi(dto),
      )
      .pipe(map(() => undefined));
  }
}
