import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map } from 'rxjs';
import type { Observable } from 'rxjs';

import { BackendAspService } from 'app/ASP.NET/backend-asp.service';
import { ApiResponse } from 'app/core/models/response.model';

import {
  ComplaintDetailDto,
  ComplaintListItemDto,
  ComplaintWriteDto,
  ConcernActionLogReadDto,
  ConcernCategoryListItemDto,
  ConcernFilterDto,
  concernCategoriesFilterForApi,
  concernFilterForApi,
  complaintWriteForApi,
  SuggestionDetailDto,
  SuggestionListItemDto,
  SuggestionWriteDto,
  suggestionWriteForApi,
} from './concerns.models';

function unwrap<T>(body: ApiResponse<T> | Record<string, unknown>): T {
  const b = body as Record<string, unknown>;
  const ok = (b['isSuccess'] ?? b['IsSuccess']) !== false;
  const errs = (b['errorMasseges'] ?? b['ErrorMasseges']) as string[] | undefined;
  if (!ok && errs?.length) {
    throw new Error(errs.join('; '));
  }
  return (b['result'] ?? b['Result']) as T;
}

export function readConcernHttpError(err: unknown): string {
  const e = err as HttpErrorResponse;
  const msgs = e?.error?.errorMasseges ?? e?.error?.ErrorMasseges;
  if (Array.isArray(msgs) && msgs.length) return msgs.join('; ');
  if (typeof e?.error?.message === 'string') return e.error.message;
  if (typeof e?.message === 'string') return e.message;
  return 'Request failed';
}

function normalizeCategory(raw: Record<string, unknown>): ConcernCategoryListItemDto {
  return {
    concernCategoryID: Number(raw['concernCategoryID'] ?? raw['ConcernCategoryID']),
    schoolID: Number(raw['schoolID'] ?? raw['SchoolID']),
    code: String(raw['code'] ?? raw['Code'] ?? ''),
    categoryKind: Number(raw['categoryKind'] ?? raw['CategoryKind'] ?? 0),
    name: String(raw['name'] ?? raw['Name'] ?? ''),
    nameAr: (raw['nameAr'] ?? raw['NameAr']) as string | null | undefined,
    description: (raw['description'] ?? raw['Description']) as string | null | undefined,
    isActive: Boolean(raw['isActive'] ?? raw['IsActive'] ?? true),
  };
}

function normalizeComplaintList(raw: Record<string, unknown>): ComplaintListItemDto {
  return {
    complaintID: Number(raw['complaintID'] ?? raw['ComplaintID']),
    schoolID: Number(raw['schoolID'] ?? raw['SchoolID']),
    academicYearID: Number(raw['academicYearID'] ?? raw['AcademicYearID']),
    concernCategoryID: Number(raw['concernCategoryID'] ?? raw['ConcernCategoryID']),
    categoryCode: String(raw['categoryCode'] ?? raw['CategoryCode'] ?? ''),
    categoryName: String(raw['categoryName'] ?? raw['CategoryName'] ?? ''),
    categoryNameAr: (raw['categoryNameAr'] ?? raw['CategoryNameAr']) as string | null | undefined,
    submitterEmployeeProfileID: Number(raw['submitterEmployeeProfileID'] ?? raw['SubmitterEmployeeProfileID']),
    submitterName: String(raw['submitterName'] ?? raw['SubmitterName'] ?? ''),
    assignedToEmployeeProfileID:
      raw['assignedToEmployeeProfileID'] != null ? Number(raw['assignedToEmployeeProfileID']) : null,
    assignedToName: (raw['assignedToName'] ?? raw['AssignedToName']) as string | null | undefined,
    title: String(raw['title'] ?? raw['Title'] ?? ''),
    status: Number(raw['status'] ?? raw['Status'] ?? 0),
    submittedAtUtc: String(raw['submittedAtUtc'] ?? raw['SubmittedAtUtc'] ?? ''),
    updatedAtUtc: String(raw['updatedAtUtc'] ?? raw['UpdatedAtUtc'] ?? ''),
    closedAtUtc: raw['closedAtUtc'] != null ? String(raw['closedAtUtc'] ?? raw['ClosedAtUtc']) : null,
  };
}

function normalizeSuggestionList(raw: Record<string, unknown>): SuggestionListItemDto {
  return {
    suggestionID: Number(raw['suggestionID'] ?? raw['SuggestionID']),
    schoolID: Number(raw['schoolID'] ?? raw['SchoolID']),
    academicYearID: Number(raw['academicYearID'] ?? raw['AcademicYearID']),
    concernCategoryID: Number(raw['concernCategoryID'] ?? raw['ConcernCategoryID']),
    categoryCode: String(raw['categoryCode'] ?? raw['CategoryCode'] ?? ''),
    categoryName: String(raw['categoryName'] ?? raw['CategoryName'] ?? ''),
    categoryNameAr: (raw['categoryNameAr'] ?? raw['CategoryNameAr']) as string | null | undefined,
    submitterEmployeeProfileID: Number(raw['submitterEmployeeProfileID'] ?? raw['SubmitterEmployeeProfileID']),
    submitterName: String(raw['submitterName'] ?? raw['SubmitterName'] ?? ''),
    assignedToEmployeeProfileID:
      raw['assignedToEmployeeProfileID'] != null ? Number(raw['assignedToEmployeeProfileID']) : null,
    assignedToName: (raw['assignedToName'] ?? raw['AssignedToName']) as string | null | undefined,
    title: String(raw['title'] ?? raw['Title'] ?? ''),
    status: Number(raw['status'] ?? raw['Status'] ?? 0),
    submittedAtUtc: String(raw['submittedAtUtc'] ?? raw['SubmittedAtUtc'] ?? ''),
    updatedAtUtc: String(raw['updatedAtUtc'] ?? raw['UpdatedAtUtc'] ?? ''),
    closedAtUtc: raw['closedAtUtc'] != null ? String(raw['closedAtUtc'] ?? raw['ClosedAtUtc']) : null,
  };
}

function normalizeActionLog(raw: Record<string, unknown>): ConcernActionLogReadDto {
  return {
    concernActionLogID: Number(raw['concernActionLogID'] ?? raw['ConcernActionLogID']),
    actionKind: Number(raw['actionKind'] ?? raw['ActionKind'] ?? 0),
    oldStatus: raw['oldStatus'] != null ? Number(raw['oldStatus'] ?? raw['OldStatus']) : null,
    newStatus: raw['newStatus'] != null ? Number(raw['newStatus'] ?? raw['NewStatus']) : null,
    comment: (raw['comment'] ?? raw['Comment']) as string | null | undefined,
    actorEmployeeProfileID:
      raw['actorEmployeeProfileID'] != null ? Number(raw['actorEmployeeProfileID'] ?? raw['ActorEmployeeProfileID']) : null,
    actorName: (raw['actorName'] ?? raw['ActorName']) as string | null | undefined,
    createdAtUtc: String(raw['createdAtUtc'] ?? raw['CreatedAtUtc'] ?? ''),
  };
}

function normalizeComplaintDetail(raw: Record<string, unknown>): ComplaintDetailDto {
  const base = normalizeComplaintList(raw);
  const logs = (raw['actionLogs'] ?? raw['ActionLogs']) as Record<string, unknown>[] | undefined;
  return {
    ...base,
    details: (raw['details'] ?? raw['Details']) as string | null | undefined,
    actionLogs: Array.isArray(logs) ? logs.map((x) => normalizeActionLog(x as Record<string, unknown>)) : [],
  };
}

function normalizeSuggestionDetail(raw: Record<string, unknown>): SuggestionDetailDto {
  const base = normalizeSuggestionList(raw);
  const logs = (raw['actionLogs'] ?? raw['ActionLogs']) as Record<string, unknown>[] | undefined;
  return {
    ...base,
    details: (raw['details'] ?? raw['Details']) as string | null | undefined,
    actionLogs: Array.isArray(logs) ? logs.map((x) => normalizeActionLog(x as Record<string, unknown>)) : [],
  };
}

@Injectable({ providedIn: 'root' })
export class ConcernsService {
  private readonly http = inject(HttpClient);
  private readonly api = inject(BackendAspService);

  private root(path: string): string {
    return `${this.api.baseUrl}/concerns${path}`;
  }

  listCategories(schoolID: number, categoryKind?: number | null): Observable<ConcernCategoryListItemDto[]> {
    const body = concernCategoriesFilterForApi(schoolID, categoryKind);
    return this.http.post<ApiResponse<unknown>>(this.root('/categories/list'), body).pipe(
      map((r) => {
        const rows = unwrap<unknown[]>(r) ?? [];
        return rows.map((x) => normalizeCategory(x as Record<string, unknown>));
      }),
    );
  }

  listComplaints(filter: ConcernFilterDto): Observable<ComplaintListItemDto[]> {
    return this.http.post<ApiResponse<unknown>>(this.root('/complaints/list'), concernFilterForApi(filter)).pipe(
      map((r) => {
        const rows = unwrap<unknown[]>(r) ?? [];
        return rows.map((x) => normalizeComplaintList(x as Record<string, unknown>));
      }),
    );
  }

  getComplaint(id: number): Observable<ComplaintDetailDto> {
    return this.http.get<ApiResponse<unknown>>(this.root(`/complaints/${id}`)).pipe(
      map((r) => normalizeComplaintDetail(unwrap<Record<string, unknown>>(r) as Record<string, unknown>)),
    );
  }

  createComplaint(dto: ComplaintWriteDto): Observable<number> {
    return this.http.post<ApiResponse<number>>(this.root('/complaints'), complaintWriteForApi(dto)).pipe(
      map((res) => Number(unwrap<number>(res))),
    );
  }

  updateComplaint(id: number, dto: ComplaintWriteDto): Observable<number> {
    return this.http.put<ApiResponse<number>>(this.root(`/complaints/${id}`), complaintWriteForApi(dto)).pipe(
      map((res) => Number(unwrap<number>(res))),
    );
  }

  listSuggestions(filter: ConcernFilterDto): Observable<SuggestionListItemDto[]> {
    return this.http.post<ApiResponse<unknown>>(this.root('/suggestions/list'), concernFilterForApi(filter)).pipe(
      map((r) => {
        const rows = unwrap<unknown[]>(r) ?? [];
        return rows.map((x) => normalizeSuggestionList(x as Record<string, unknown>));
      }),
    );
  }

  getSuggestion(id: number): Observable<SuggestionDetailDto> {
    return this.http.get<ApiResponse<unknown>>(this.root(`/suggestions/${id}`)).pipe(
      map((r) => normalizeSuggestionDetail(unwrap<Record<string, unknown>>(r) as Record<string, unknown>)),
    );
  }

  createSuggestion(dto: SuggestionWriteDto): Observable<number> {
    return this.http.post<ApiResponse<number>>(this.root('/suggestions'), suggestionWriteForApi(dto)).pipe(
      map((res) => Number(unwrap<number>(res))),
    );
  }

  updateSuggestion(id: number, dto: SuggestionWriteDto): Observable<number> {
    return this.http.put<ApiResponse<number>>(this.root(`/suggestions/${id}`), suggestionWriteForApi(dto)).pipe(
      map((res) => Number(unwrap<number>(res))),
    );
  }
}
