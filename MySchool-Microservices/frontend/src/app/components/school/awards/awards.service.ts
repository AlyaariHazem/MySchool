import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map } from 'rxjs/operators';
import type { Observable } from 'rxjs';

import { BackendAspService } from 'app/ASP.NET/backend-asp.service';
import { ApiResponse } from 'app/core/models/response.model';

import {
  AwardCycleDto,
  AwardCycleWriteDto,
  AwardDto,
  AwardNominationDto,
  AwardWinnerDto,
  AwardWriteDto,
} from './awards.models';

function unwrap<T>(body: ApiResponse<T> | Record<string, unknown>): T {
  const b = body as Record<string, unknown>;
  const ok = (b['isSuccess'] ?? b['IsSuccess']) !== false;
  const errs = (b['errorMasseges'] ?? b['ErrorMasseges']) as string[] | undefined;
  if (!ok && errs?.length) throw new Error(errs.join('; '));
  return (b['result'] ?? b['Result']) as T;
}

export function readAwardsHttpError(err: unknown): string {
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

@Injectable({ providedIn: 'root' })
export class AwardsService {
  private readonly http = inject(HttpClient);
  private readonly api = inject(BackendAspService);

  private url(path: string): string {
    return `${this.api.baseUrl}/awards${path}`;
  }

  listAwards(schoolID?: number | null): Observable<AwardDto[]> {
    const body = schoolID != null && schoolID > 0 ? { schoolID } : {};
    return this.http.post<ApiResponse<unknown>>(this.url('/list'), body).pipe(
      map((r) => {
        const rows = unwrap<unknown[]>(r) ?? [];
        return rows.map((x) => this.normAward(x as Record<string, unknown>));
      }),
    );
  }

  createAward(dto: AwardWriteDto): Observable<number> {
    return this.http.post<ApiResponse<unknown>>(this.url(''), dto).pipe(
      map((r) => {
        const o = unwrap<Record<string, unknown>>(r);
        return num(o['awardID'] ?? o['AwardID']);
      }),
    );
  }

  updateAward(id: number, dto: AwardWriteDto): Observable<void> {
    return this.http.put<ApiResponse<unknown>>(this.url(`/${id}`), dto).pipe(map(() => void 0));
  }

  listCycles(schoolID?: number | null): Observable<AwardCycleDto[]> {
    const body = schoolID != null && schoolID > 0 ? { schoolID } : {};
    return this.http.post<ApiResponse<unknown>>(this.url('/cycles/list'), body).pipe(
      map((r) => {
        const rows = unwrap<unknown[]>(r) ?? [];
        return rows.map((x) => this.normCycle(x as Record<string, unknown>));
      }),
    );
  }

  createCycle(dto: AwardCycleWriteDto): Observable<number> {
    return this.http.post<ApiResponse<unknown>>(this.url('/cycles'), dto).pipe(
      map((r) => {
        const o = unwrap<Record<string, unknown>>(r);
        return num(o['awardCycleID'] ?? o['AwardCycleID']);
      }),
    );
  }

  updateCycle(id: number, dto: AwardCycleWriteDto): Observable<void> {
    return this.http.put<ApiResponse<unknown>>(this.url(`/cycles/${id}`), dto).pipe(map(() => void 0));
  }

  listNominations(schoolID?: number | null): Observable<AwardNominationDto[]> {
    const body = schoolID != null && schoolID > 0 ? { schoolID } : {};
    return this.http.post<ApiResponse<unknown>>(this.url('/nominations/list'), body).pipe(
      map((r) => {
        const rows = unwrap<unknown[]>(r) ?? [];
        return rows.map((x) => this.normNomination(x as Record<string, unknown>));
      }),
    );
  }

  listWinners(schoolID?: number | null): Observable<AwardWinnerDto[]> {
    const body = schoolID != null && schoolID > 0 ? { schoolID } : {};
    return this.http.post<ApiResponse<unknown>>(this.url('/winners/list'), body).pipe(
      map((r) => {
        const rows = unwrap<unknown[]>(r) ?? [];
        return rows.map((x) => this.normWinner(x as Record<string, unknown>));
      }),
    );
  }

  private normAward(raw: Record<string, unknown>): AwardDto {
    return {
      awardID: num(raw['awardID'] ?? raw['AwardID']),
      schoolID: num(raw['schoolID'] ?? raw['SchoolID']),
      code: str(raw['code'] ?? raw['Code']),
      title: str(raw['title'] ?? raw['Title']),
      description: (raw['description'] ?? raw['Description']) as string | null | undefined,
      cycleKind: num(raw['cycleKind'] ?? raw['CycleKind']) as AwardDto['cycleKind'],
      isActive: Boolean(raw['isActive'] ?? raw['IsActive']),
      sortOrder: num(raw['sortOrder'] ?? raw['SortOrder']),
    };
  }

  private normCycle(raw: Record<string, unknown>): AwardCycleDto {
    return {
      awardCycleID: num(raw['awardCycleID'] ?? raw['AwardCycleID']),
      awardID: num(raw['awardID'] ?? raw['AwardID']),
      awardTitle: (raw['awardTitle'] ?? raw['AwardTitle']) as string | undefined,
      academicYearID: num(raw['academicYearID'] ?? raw['AcademicYearID']),
      termID: raw['termID'] != null ? num(raw['termID'] ?? raw['TermID']) : null,
      periodStartUtc: str(raw['periodStartUtc'] ?? raw['PeriodStartUtc']),
      periodEndUtc: str(raw['periodEndUtc'] ?? raw['PeriodEndUtc']),
      status: num(raw['status'] ?? raw['Status']) as AwardCycleDto['status'],
    };
  }

  private normNomination(raw: Record<string, unknown>): AwardNominationDto {
    return {
      awardNominationID: num(raw['awardNominationID'] ?? raw['AwardNominationID']),
      awardCycleID: num(raw['awardCycleID'] ?? raw['AwardCycleID']),
      studentID: num(raw['studentID'] ?? raw['StudentID']),
      studentName: (raw['studentName'] ?? raw['StudentName']) as string | undefined,
      nominatedByEmployeeProfileID:
        raw['nominatedByEmployeeProfileID'] != null
          ? num(raw['nominatedByEmployeeProfileID'] ?? raw['NominatedByEmployeeProfileID'])
          : null,
      nominatedByEmployeeName: (raw['nominatedByEmployeeName'] ?? raw['NominatedByEmployeeName']) as string | undefined,
      notes: (raw['notes'] ?? raw['Notes']) as string | null | undefined,
      status: num(raw['status'] ?? raw['Status']) as AwardNominationDto['status'],
      createdAtUtc: str(raw['createdAtUtc'] ?? raw['CreatedAtUtc']),
    };
  }

  private normWinner(raw: Record<string, unknown>): AwardWinnerDto {
    return {
      awardWinnerID: num(raw['awardWinnerID'] ?? raw['AwardWinnerID']),
      awardCycleID: num(raw['awardCycleID'] ?? raw['AwardCycleID']),
      studentID: num(raw['studentID'] ?? raw['StudentID']),
      studentName: (raw['studentName'] ?? raw['StudentName']) as string | undefined,
      rank: num(raw['rank'] ?? raw['Rank']),
      selectedByEmployeeProfileID:
        raw['selectedByEmployeeProfileID'] != null
          ? num(raw['selectedByEmployeeProfileID'] ?? raw['SelectedByEmployeeProfileID'])
          : null,
      selectedByEmployeeName: (raw['selectedByEmployeeName'] ?? raw['SelectedByEmployeeName']) as string | undefined,
      notes: (raw['notes'] ?? raw['Notes']) as string | null | undefined,
      selectedAtUtc: str(raw['selectedAtUtc'] ?? raw['SelectedAtUtc']),
    };
  }
}
