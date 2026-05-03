import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map } from 'rxjs/operators';
import type { Observable } from 'rxjs';

import { BackendAspService } from 'app/ASP.NET/backend-asp.service';
import { ApiResponse } from 'app/core/models/response.model';

import {
  PointsBalanceDto,
  PointsLedgerFilterDto,
  PointsLedgerListItemDto,
  PointsRuleDto,
  PointsRuleFilterDto,
  PointsRuleWriteDto,
  PointsSourceDto,
  PostCentralPointsDto,
  PostCentralPointsResultDto,
  postCentralPointsForApi,
} from './central-points.models';

function unwrap<T>(body: ApiResponse<T> | Record<string, unknown>): T {
  const b = body as Record<string, unknown>;
  const ok = (b['isSuccess'] ?? b['IsSuccess']) !== false;
  const errs = (b['errorMasseges'] ?? b['ErrorMasseges']) as string[] | undefined;
  if (!ok && errs?.length) {
    throw new Error(errs.join('; '));
  }
  return (b['result'] ?? b['Result']) as T;
}

export function readCentralPointsHttpError(err: unknown): string {
  const e = err as HttpErrorResponse;
  const msgs = e?.error?.errorMasseges ?? e?.error?.ErrorMasseges;
  if (Array.isArray(msgs) && msgs.length) return msgs.join('; ');
  if (typeof e?.error?.message === 'string') return e.error.message;
  if (typeof e?.message === 'string') return e.message;
  return 'Request failed';
}

function num(raw: unknown, fallback = 0): number {
  const n = Number(raw);
  return Number.isFinite(n) ? n : fallback;
}

function str(raw: unknown): string {
  return raw == null ? '' : String(raw);
}

function normalizeSource(raw: Record<string, unknown>): PointsSourceDto {
  return {
    pointsSourceID: num(raw['pointsSourceID'] ?? raw['PointsSourceID']),
    code: str(raw['code'] ?? raw['Code']),
    displayName: str(raw['displayName'] ?? raw['DisplayName']),
    description: (raw['description'] ?? raw['Description']) as string | null | undefined,
    sortOrder: num(raw['sortOrder'] ?? raw['SortOrder']),
    isActive: Boolean(raw['isActive'] ?? raw['IsActive']),
  };
}

function normalizeRule(raw: Record<string, unknown>): PointsRuleDto {
  return {
    pointsRuleID: num(raw['pointsRuleID'] ?? raw['PointsRuleID']),
    schoolID: raw['schoolID'] != null ? num(raw['schoolID'] ?? raw['SchoolID']) : null,
    academicYearID: raw['academicYearID'] != null ? num(raw['academicYearID'] ?? raw['AcademicYearID']) : null,
    pointsSourceID: num(raw['pointsSourceID'] ?? raw['PointsSourceID']),
    pointsSourceCode: str(raw['pointsSourceCode'] ?? raw['PointsSourceCode']),
    ruleKey: str(raw['ruleKey'] ?? raw['RuleKey'] ?? '*'),
    deltaPoints: num(raw['deltaPoints'] ?? raw['DeltaPoints']),
    priority: num(raw['priority'] ?? raw['Priority']),
    isActive: Boolean(raw['isActive'] ?? raw['IsActive']),
    effectiveFromUtc: raw['effectiveFromUtc'] != null ? str(raw['effectiveFromUtc'] ?? raw['EffectiveFromUtc']) : null,
    effectiveToUtc: raw['effectiveToUtc'] != null ? str(raw['effectiveToUtc'] ?? raw['EffectiveToUtc']) : null,
  };
}

function normalizeLedger(raw: Record<string, unknown>): PointsLedgerListItemDto {
  return {
    pointsLedgerID: num(raw['pointsLedgerID'] ?? raw['PointsLedgerID']),
    pointsTransactionID: num(raw['pointsTransactionID'] ?? raw['PointsTransactionID']),
    employeeProfileID: num(raw['employeeProfileID'] ?? raw['EmployeeProfileID']),
    employeeDisplayName: (raw['employeeDisplayName'] ?? raw['EmployeeDisplayName']) as string | null | undefined,
    schoolID: num(raw['schoolID'] ?? raw['SchoolID']),
    academicYearID: num(raw['academicYearID'] ?? raw['AcademicYearID']),
    pointsSourceCode: str(raw['pointsSourceCode'] ?? raw['PointsSourceCode']),
    pointsRuleID: raw['pointsRuleID'] != null ? num(raw['pointsRuleID'] ?? raw['PointsRuleID']) : null,
    deltaPoints: num(raw['deltaPoints'] ?? raw['DeltaPoints']),
    memo: (raw['memo'] ?? raw['Memo']) as string | null | undefined,
    createdAtUtc: str(raw['createdAtUtc'] ?? raw['CreatedAtUtc']),
    correlationEntityType: (raw['correlationEntityType'] ?? raw['CorrelationEntityType']) as string | null | undefined,
    correlationEntityID: raw['correlationEntityID'] != null ? num(raw['correlationEntityID'] ?? raw['CorrelationEntityID']) : null,
    idempotencyKey: (raw['idempotencyKey'] ?? raw['IdempotencyKey']) as string | null | undefined,
  };
}

function normalizeBalance(raw: Record<string, unknown>): PointsBalanceDto {
  return {
    employeeProfileID: num(raw['employeeProfileID'] ?? raw['EmployeeProfileID']),
    schoolID: num(raw['schoolID'] ?? raw['SchoolID']),
    academicYearID: num(raw['academicYearID'] ?? raw['AcademicYearID']),
    totalPoints: num(raw['totalPoints'] ?? raw['TotalPoints']),
    updatedAtUtc: str(raw['updatedAtUtc'] ?? raw['UpdatedAtUtc']),
  };
}

function normalizePostResult(raw: Record<string, unknown>): PostCentralPointsResultDto {
  return {
    pointsTransactionID: num(raw['pointsTransactionID'] ?? raw['PointsTransactionID']),
    pointsLedgerID: num(raw['pointsLedgerID'] ?? raw['PointsLedgerID']),
    appliedDeltaPoints: num(raw['appliedDeltaPoints'] ?? raw['AppliedDeltaPoints']),
    matchedPointsRuleID: raw['matchedPointsRuleID'] != null ? num(raw['matchedPointsRuleID'] ?? raw['MatchedPointsRuleID']) : null,
    newBalanceTotal: num(raw['newBalanceTotal'] ?? raw['NewBalanceTotal']),
    wasIdempotentReplay: Boolean(raw['wasIdempotentReplay'] ?? raw['WasIdempotentReplay']),
  };
}

@Injectable({ providedIn: 'root' })
export class CentralPointsService {
  private readonly http = inject(HttpClient);
  private readonly api = inject(BackendAspService);

  private url(path: string): string {
    return `${this.api.baseUrl}/central-points${path}`;
  }

  listSources(): Observable<PointsSourceDto[]> {
    return this.http.get<ApiResponse<unknown>>(this.url('/sources')).pipe(
      map((r) => {
        const rows = unwrap<unknown[]>(r) ?? [];
        return rows.map((x) => normalizeSource(x as Record<string, unknown>));
      }),
    );
  }

  listRules(filter: PointsRuleFilterDto): Observable<PointsRuleDto[]> {
    return this.http.post<ApiResponse<unknown>>(this.url('/rules/list'), filter).pipe(
      map((r) => {
        const rows = unwrap<unknown[]>(r) ?? [];
        return rows.map((x) => normalizeRule(x as Record<string, unknown>));
      }),
    );
  }

  createRule(dto: PointsRuleWriteDto): Observable<number> {
    return this.http.post<ApiResponse<unknown>>(this.url('/rules'), dto).pipe(
      map((r) => {
        const o = unwrap<Record<string, unknown>>(r) as Record<string, unknown>;
        return num(o['pointsRuleID'] ?? o['PointsRuleID']);
      }),
    );
  }

  updateRule(id: number, dto: PointsRuleWriteDto): Observable<void> {
    return this.http.put<ApiResponse<unknown>>(this.url(`/rules/${id}`), dto).pipe(map(() => void 0));
  }

  listLedger(filter: PointsLedgerFilterDto): Observable<{ items: PointsLedgerListItemDto[]; totalCount: number }> {
    return this.http.post<ApiResponse<unknown>>(this.url('/ledger/list'), filter).pipe(
      map((r) => {
        const o = unwrap<Record<string, unknown>>(r) as Record<string, unknown>;
        const itemsRaw = (o['items'] ?? o['Items']) as unknown[] | undefined;
        const total = num(o['totalCount'] ?? o['TotalCount']);
        const items = Array.isArray(itemsRaw) ? itemsRaw.map((x) => normalizeLedger(x as Record<string, unknown>)) : [];
        return { items, totalCount: total };
      }),
    );
  }

  getBalance(employeeProfileId: number, schoolId: number): Observable<PointsBalanceDto | null> {
    const q = `employeeProfileId=${employeeProfileId}&schoolId=${schoolId}`;
    return this.http.get<ApiResponse<unknown>>(this.url(`/balance?${q}`)).pipe(
      map((r) => {
        const raw = unwrap<Record<string, unknown> | null>(r);
        if (raw == null) return null;
        return normalizeBalance(raw);
      }),
    );
  }

  post(dto: PostCentralPointsDto): Observable<PostCentralPointsResultDto> {
    return this.http.post<ApiResponse<unknown>>(this.url('/post'), postCentralPointsForApi(dto)).pipe(
      map((r) => normalizePostResult(unwrap<Record<string, unknown>>(r) as Record<string, unknown>)),
    );
  }

  rebuildBalance(employeeProfileId: number, schoolId: number): Observable<number> {
    const q = `employeeProfileId=${employeeProfileId}&schoolId=${schoolId}`;
    return this.http.post<ApiResponse<unknown>>(this.url(`/rebuild-balance?${q}`), {}).pipe(
      map((r) => {
        const o = unwrap<Record<string, unknown>>(r) as Record<string, unknown>;
        return num(o['totalPoints'] ?? o['TotalPoints']);
      }),
    );
  }
}
