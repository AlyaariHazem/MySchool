import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map } from 'rxjs';
import type { Observable } from 'rxjs';

import { BackendAspService } from 'app/ASP.NET/backend-asp.service';
import { ApiResponse } from 'app/core/models/response.model';

import {
  ViolationActionWriteDto,
  ViolationDetailDto,
  violationEscalateForApi,
  ViolationEscalateDto,
  ViolationFilterDto,
  violationFilterForApi,
  ViolationListItemDto,
  ViolationResponseWriteDto,
  violationResponseWriteForApi,
  ViolationTypeListItemDto,
  violationTypesFilterForApi,
  ViolationWriteDto,
  violationActionWriteForApi,
  violationWriteForApi,
} from './violations.models';

function unwrap<T>(body: ApiResponse<T> | Record<string, unknown>): T {
  const b = body as Record<string, unknown>;
  const ok = (b['isSuccess'] ?? b['IsSuccess']) !== false;
  const errs = (b['errorMasseges'] ?? b['ErrorMasseges']) as string[] | undefined;
  if (!ok && errs?.length) {
    throw new Error(errs.join('; '));
  }
  return (b['result'] ?? b['Result']) as T;
}

export function readViolationHttpError(err: unknown): string {
  const e = err as HttpErrorResponse;
  const msgs = e?.error?.errorMasseges ?? e?.error?.ErrorMasseges;
  if (Array.isArray(msgs) && msgs.length) return msgs.join('; ');
  if (typeof e?.error?.message === 'string') return e.error.message;
  if (typeof e?.message === 'string') return e.message;
  return 'Request failed';
}

function normalizeTypeRow(raw: Record<string, unknown>): ViolationTypeListItemDto {
  return {
    violationTypeID: Number(raw['violationTypeID'] ?? raw['ViolationTypeID']),
    schoolID: Number(raw['schoolID'] ?? raw['SchoolID']),
    kind: Number(raw['kind'] ?? raw['Kind'] ?? 0),
    name: String(raw['name'] ?? raw['Name'] ?? ''),
    description: (raw['description'] ?? raw['Description']) as string | null | undefined,
    sortOrder: Number(raw['sortOrder'] ?? raw['SortOrder'] ?? 0),
    isActive: Boolean(raw['isActive'] ?? raw['IsActive'] ?? true),
  };
}

function normalizeListRow(raw: Record<string, unknown>): ViolationListItemDto {
  return {
    violationID: Number(raw['violationID'] ?? raw['ViolationID']),
    schoolID: Number(raw['schoolID'] ?? raw['SchoolID']),
    academicYearID: raw['academicYearID'] != null ? Number(raw['academicYearID']) : null,
    violationTypeID: Number(raw['violationTypeID'] ?? raw['ViolationTypeID']),
    violationTypeKind: Number(raw['violationTypeKind'] ?? raw['ViolationTypeKind'] ?? 0),
    violationTypeName: String(raw['violationTypeName'] ?? raw['ViolationTypeName'] ?? ''),
    subjectEmployeeProfileID: Number(raw['subjectEmployeeProfileID'] ?? raw['SubjectEmployeeProfileID']),
    subjectEmployeeName: String(raw['subjectEmployeeName'] ?? raw['SubjectEmployeeName'] ?? ''),
    openedByEmployeeProfileID:
      raw['openedByEmployeeProfileID'] != null ? Number(raw['openedByEmployeeProfileID']) : null,
    openedByName: (raw['openedByName'] ?? raw['OpenedByName']) as string | null | undefined,
    title: String(raw['title'] ?? raw['Title'] ?? ''),
    status: Number(raw['status'] ?? raw['Status'] ?? 0),
    openedAtUtc: String(raw['openedAtUtc'] ?? raw['OpenedAtUtc'] ?? ''),
    updatedAtUtc: String(raw['updatedAtUtc'] ?? raw['UpdatedAtUtc'] ?? ''),
    closedAtUtc: raw['closedAtUtc'] != null ? String(raw['closedAtUtc']) : null,
  };
}

function normalizeResponse(raw: Record<string, unknown>): ViolationDetailDto['responses'][number] {
  return {
    violationResponseID: Number(raw['violationResponseID'] ?? raw['ViolationResponseID']),
    violationID: Number(raw['violationID'] ?? raw['ViolationID']),
    authorEmployeeProfileID: raw['authorEmployeeProfileID'] != null ? Number(raw['authorEmployeeProfileID']) : null,
    authorName: (raw['authorName'] ?? raw['AuthorName']) as string | null | undefined,
    body: String(raw['body'] ?? raw['Body'] ?? ''),
    createdAtUtc: String(raw['createdAtUtc'] ?? raw['CreatedAtUtc'] ?? ''),
  };
}

function normalizeAction(raw: Record<string, unknown>): ViolationDetailDto['actions'][number] {
  return {
    violationActionID: Number(raw['violationActionID'] ?? raw['ViolationActionID']),
    violationID: Number(raw['violationID'] ?? raw['ViolationID']),
    category: Number(raw['category'] ?? raw['Category'] ?? 0),
    title: String(raw['title'] ?? raw['Title'] ?? ''),
    notes: (raw['notes'] ?? raw['Notes']) as string | null | undefined,
    performedByEmployeeProfileID: Number(raw['performedByEmployeeProfileID'] ?? raw['PerformedByEmployeeProfileID']),
    performedByName: String(raw['performedByName'] ?? raw['PerformedByName'] ?? ''),
    performedAtUtc: String(raw['performedAtUtc'] ?? raw['PerformedAtUtc'] ?? ''),
  };
}

function normalizeEscalation(raw: Record<string, unknown>): ViolationDetailDto['escalationHistory'][number] {
  return {
    violationEscalationHistoryID: Number(raw['violationEscalationHistoryID'] ?? raw['ViolationEscalationHistoryID']),
    violationID: Number(raw['violationID'] ?? raw['ViolationID']),
    previousViolationTypeID: raw['previousViolationTypeID'] != null ? Number(raw['previousViolationTypeID']) : null,
    previousKind: raw['previousKind'] != null ? Number(raw['previousKind']) : null,
    previousTypeName: (raw['previousTypeName'] ?? raw['PreviousTypeName']) as string | null | undefined,
    newViolationTypeID: Number(raw['newViolationTypeID'] ?? raw['NewViolationTypeID']),
    newKind: Number(raw['newKind'] ?? raw['NewKind'] ?? 0),
    newTypeName: String(raw['newTypeName'] ?? raw['NewTypeName'] ?? ''),
    reason: (raw['reason'] ?? raw['Reason']) as string | null | undefined,
    changedByEmployeeProfileID: Number(raw['changedByEmployeeProfileID'] ?? raw['ChangedByEmployeeProfileID']),
    changedByName: String(raw['changedByName'] ?? raw['ChangedByName'] ?? ''),
    changedAtUtc: String(raw['changedAtUtc'] ?? raw['ChangedAtUtc'] ?? ''),
  };
}

function normalizeDetail(raw: Record<string, unknown>): ViolationDetailDto {
  const base = normalizeListRow(raw);
  const resp = (raw['responses'] ?? raw['Responses']) as Record<string, unknown>[] | undefined;
  const act = (raw['actions'] ?? raw['Actions']) as Record<string, unknown>[] | undefined;
  const esc = (raw['escalationHistory'] ?? raw['EscalationHistory']) as Record<string, unknown>[] | undefined;
  return {
    ...base,
    details: (raw['details'] ?? raw['Details']) as string | null | undefined,
    responses: Array.isArray(resp) ? resp.map((x) => normalizeResponse(x as Record<string, unknown>)) : [],
    actions: Array.isArray(act) ? act.map((x) => normalizeAction(x as Record<string, unknown>)) : [],
    escalationHistory: Array.isArray(esc) ? esc.map((x) => normalizeEscalation(x as Record<string, unknown>)) : [],
  };
}

@Injectable({ providedIn: 'root' })
export class ViolationsService {
  private readonly http = inject(HttpClient);
  private readonly api = inject(BackendAspService);

  private root(path: string): string {
    return `${this.api.baseUrl}/violations${path}`;
  }

  listTypes(schoolID: number): Observable<ViolationTypeListItemDto[]> {
    const body = violationTypesFilterForApi(schoolID);
    return this.http.post<ApiResponse<unknown>>(this.root('/types/list'), body).pipe(
      map((r) => {
        const rows = unwrap<unknown[]>(r) ?? [];
        return rows.map((x) => normalizeTypeRow(x as Record<string, unknown>));
      }),
    );
  }

  list(filter: ViolationFilterDto): Observable<ViolationListItemDto[]> {
    const body = violationFilterForApi(filter);
    return this.http.post<ApiResponse<unknown>>(this.root('/list'), body).pipe(
      map((r) => {
        const rows = unwrap<unknown[]>(r) ?? [];
        return rows.map((x) => normalizeListRow(x as Record<string, unknown>));
      }),
    );
  }

  getById(id: number): Observable<ViolationDetailDto> {
    return this.http.get<ApiResponse<unknown>>(this.root(`/${id}`)).pipe(
      map((r) => normalizeDetail(unwrap<Record<string, unknown>>(r) as Record<string, unknown>)),
    );
  }

  create(dto: ViolationWriteDto): Observable<number> {
    return this.http.post<ApiResponse<number>>(this.root(''), violationWriteForApi(dto)).pipe(
      map((res) => Number(unwrap<number>(res))),
    );
  }

  update(id: number, dto: ViolationWriteDto): Observable<number> {
    return this.http.put<ApiResponse<number>>(this.root(`/${id}`), violationWriteForApi(dto)).pipe(
      map((res) => Number(unwrap<number>(res))),
    );
  }

  addResponse(violationId: number, dto: ViolationResponseWriteDto): Observable<number> {
    return this.http
      .post<ApiResponse<number>>(this.root(`/${violationId}/responses`), violationResponseWriteForApi(dto))
      .pipe(map((res) => Number(unwrap<number>(res))));
  }

  addAction(violationId: number, dto: ViolationActionWriteDto): Observable<number> {
    return this.http
      .post<ApiResponse<number>>(this.root(`/${violationId}/actions`), violationActionWriteForApi(dto))
      .pipe(map((res) => Number(unwrap<number>(res))));
  }

  escalate(violationId: number, dto: ViolationEscalateDto): Observable<void> {
    return this.http.post<ApiResponse<unknown>>(this.root(`/${violationId}/escalate`), violationEscalateForApi(dto)).pipe(
      map(() => undefined),
    );
  }

  delete(id: number): Observable<void> {
    return this.http.delete<ApiResponse<boolean>>(this.root(`/${id}`)).pipe(map(() => undefined));
  }
}
