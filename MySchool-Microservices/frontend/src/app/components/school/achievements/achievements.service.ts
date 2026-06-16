import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map } from 'rxjs';
import type { Observable } from 'rxjs';

import { BackendAspService } from 'app/ASP.NET/backend-asp.service';
import { ApiResponse } from 'app/core/models/response.model';

import {
  AchievementCatalogFilterDto,
  AchievementCatalogItemDto,
  AchievementRequestDetailDto,
  AchievementRequestFilterDto,
  AchievementRequestListItemDto,
  achievementCatalogFilterForApi,
  achievementRequestFilterForApi,
  AchievementRequestWriteDto,
  achievementRequestWriteForApi,
} from './achievements.models';

function unwrap<T>(body: ApiResponse<T> | Record<string, unknown>): T {
  const b = body as Record<string, unknown>;
  const ok = (b['isSuccess'] ?? b['IsSuccess']) !== false;
  const errs = (b['errorMasseges'] ?? b['ErrorMasseges']) as string[] | undefined;
  if (!ok && errs?.length) {
    throw new Error(errs.join('; '));
  }
  return (b['result'] ?? b['Result']) as T;
}

export function readAchievementHttpError(err: unknown): string {
  const e = err as HttpErrorResponse;
  const msgs = e?.error?.errorMasseges ?? e?.error?.ErrorMasseges;
  if (Array.isArray(msgs) && msgs.length) return msgs.join('; ');
  if (typeof e?.error?.message === 'string') return e.error.message;
  if (typeof e?.message === 'string') return e.message;
  return 'Request failed';
}

function normalizeListRow(raw: Record<string, unknown>): AchievementRequestListItemDto {
  return {
    achievementRequestID: Number(raw['achievementRequestID'] ?? raw['AchievementRequestID']),
    schoolID: Number(raw['schoolID'] ?? raw['SchoolID']),
    academicYearID: Number(raw['academicYearID'] ?? raw['AcademicYearID']),
    employeeProfileID: Number(raw['employeeProfileID'] ?? raw['EmployeeProfileID']),
    employeeName: String(raw['employeeName'] ?? raw['EmployeeName'] ?? ''),
    achievementID: raw['achievementID'] != null ? Number(raw['achievementID']) : null,
    achievementTitle: (raw['achievementTitle'] ?? raw['AchievementTitle']) as string | null | undefined,
    customTitle: (raw['customTitle'] ?? raw['CustomTitle']) as string | null | undefined,
    status: Number(raw['status'] ?? raw['Status'] ?? 0),
    submittedAtUtc: String(raw['submittedAtUtc'] ?? raw['SubmittedAtUtc'] ?? ''),
    resolvedAtUtc: raw['resolvedAtUtc'] != null ? String(raw['resolvedAtUtc']) : null,
  };
}

function normalizeApproval(raw: Record<string, unknown>) {
  return {
    achievementApprovalID: Number(raw['achievementApprovalID'] ?? raw['AchievementApprovalID']),
    approverEmployeeProfileID: Number(raw['approverEmployeeProfileID'] ?? raw['ApproverEmployeeProfileID']),
    approverName: String(raw['approverName'] ?? raw['ApproverName'] ?? ''),
    decision: Number(raw['decision'] ?? raw['Decision'] ?? 0),
    comment: (raw['comment'] ?? raw['Comment']) as string | null | undefined,
    sortOrder: Number(raw['sortOrder'] ?? raw['SortOrder'] ?? 0),
    decidedAtUtc: raw['decidedAtUtc'] != null ? String(raw['decidedAtUtc']) : null,
    createdAtUtc: String(raw['createdAtUtc'] ?? raw['CreatedAtUtc'] ?? ''),
  };
}

function normalizeAttachment(raw: Record<string, unknown>) {
  return {
    achievementAttachmentID: Number(raw['achievementAttachmentID'] ?? raw['AchievementAttachmentID']),
    fileName: String(raw['fileName'] ?? raw['FileName'] ?? ''),
    contentType: (raw['contentType'] ?? raw['ContentType']) as string | null | undefined,
    fileSizeBytes: raw['fileSizeBytes'] != null ? Number(raw['fileSizeBytes']) : null,
    uploadedAtUtc: String(raw['uploadedAtUtc'] ?? raw['UploadedAtUtc'] ?? ''),
  };
}

function normalizeLedger(raw: Record<string, unknown>) {
  return {
    achievementPointsLedgerID: Number(raw['achievementPointsLedgerID'] ?? raw['AchievementPointsLedgerID']),
    deltaPoints: Number(raw['deltaPoints'] ?? raw['DeltaPoints'] ?? 0),
    reason: String(raw['reason'] ?? raw['Reason'] ?? ''),
    createdAtUtc: String(raw['createdAtUtc'] ?? raw['CreatedAtUtc'] ?? ''),
  };
}

function normalizeDetail(raw: Record<string, unknown>): AchievementRequestDetailDto {
  const base = normalizeListRow(raw);
  const appr = (raw['approvals'] ?? raw['Approvals']) as Record<string, unknown>[] | undefined;
  const att = (raw['attachments'] ?? raw['Attachments']) as Record<string, unknown>[] | undefined;
  const led = (raw['ledgerEntries'] ?? raw['LedgerEntries']) as Record<string, unknown>[] | undefined;
  return {
    ...base,
    notes: (raw['notes'] ?? raw['Notes']) as string | null | undefined,
    updatedAtUtc: String(raw['updatedAtUtc'] ?? raw['UpdatedAtUtc'] ?? ''),
    approvals: Array.isArray(appr) ? appr.map((x) => normalizeApproval(x as Record<string, unknown>)) : [],
    attachments: Array.isArray(att) ? att.map((x) => normalizeAttachment(x as Record<string, unknown>)) : [],
    ledgerEntries: Array.isArray(led) ? led.map((x) => normalizeLedger(x as Record<string, unknown>)) : [],
  };
}

function normalizeCatalogItem(raw: Record<string, unknown>): AchievementCatalogItemDto {
  return {
    achievementID: Number(raw['achievementID'] ?? raw['AchievementID']),
    code: String(raw['code'] ?? raw['Code'] ?? ''),
    title: String(raw['title'] ?? raw['Title'] ?? ''),
    defaultPoints: Number(raw['defaultPoints'] ?? raw['DefaultPoints'] ?? 0),
    academicYearID: raw['academicYearID'] != null ? Number(raw['academicYearID']) : null,
  };
}

@Injectable({ providedIn: 'root' })
export class AchievementsService {
  private readonly http = inject(HttpClient);
  private readonly api = inject(BackendAspService);

  private root(path: string): string {
    return `${this.api.baseUrl}/achievement-requests${path}`;
  }

  catalog(filter: AchievementCatalogFilterDto): Observable<AchievementCatalogItemDto[]> {
    const body = achievementCatalogFilterForApi(filter);
    return this.http.post<ApiResponse<unknown>>(this.root('/catalog'), body).pipe(
      map((r) => {
        const rows = unwrap<unknown[]>(r) ?? [];
        return rows.map((x) => normalizeCatalogItem(x as Record<string, unknown>));
      }),
    );
  }

  list(filter: AchievementRequestFilterDto): Observable<AchievementRequestListItemDto[]> {
    const body = achievementRequestFilterForApi(filter);
    return this.http.post<ApiResponse<unknown>>(this.root('/list'), body).pipe(
      map((r) => {
        const rows = unwrap<unknown[]>(r) ?? [];
        return rows.map((x) => normalizeListRow(x as Record<string, unknown>));
      }),
    );
  }

  getById(id: number): Observable<AchievementRequestDetailDto> {
    return this.http.get<ApiResponse<unknown>>(this.root(`/${id}`)).pipe(
      map((r) => normalizeDetail(unwrap<Record<string, unknown>>(r) as Record<string, unknown>)),
    );
  }

  create(dto: AchievementRequestWriteDto): Observable<number> {
    return this.http.post<ApiResponse<number>>(this.root(''), achievementRequestWriteForApi(dto)).pipe(
      map((res) => Number(unwrap<number>(res))),
    );
  }

  update(id: number, dto: AchievementRequestWriteDto): Observable<number> {
    return this.http.put<ApiResponse<number>>(this.root(`/${id}`), achievementRequestWriteForApi(dto)).pipe(
      map((res) => Number(unwrap<number>(res))),
    );
  }

  delete(id: number): Observable<void> {
    return this.http.delete<ApiResponse<boolean>>(this.root(`/${id}`)).pipe(map(() => undefined));
  }
}
