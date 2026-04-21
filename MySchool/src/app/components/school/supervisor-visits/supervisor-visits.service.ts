import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map } from 'rxjs';
import type { Observable } from 'rxjs';

import { BackendAspService } from 'app/ASP.NET/backend-asp.service';
import { ApiResponse } from 'app/core/models/response.model';

import {
  RecommendationFollowUpReadDto,
  SupervisorVisitDetailDto,
  SupervisorVisitFilterDto,
  SupervisorVisitListItemDto,
  supervisorVisitFilterForApi,
  SupervisorVisitWriteDto,
  supervisorVisitWriteForApi,
  VisitObservationReadDto,
  VisitRecommendationReadDto,
} from './supervisor-visits.models';

function unwrap<T>(body: ApiResponse<T> | Record<string, unknown>): T {
  const b = body as Record<string, unknown>;
  const ok = (b['isSuccess'] ?? b['IsSuccess']) !== false;
  const errs = (b['errorMasseges'] ?? b['ErrorMasseges']) as string[] | undefined;
  if (!ok && errs?.length) {
    throw new Error(errs.join('; '));
  }
  return (b['result'] ?? b['Result']) as T;
}

export function readSupervisorVisitHttpError(err: unknown): string {
  const e = err as HttpErrorResponse;
  const msgs = e?.error?.errorMasseges ?? e?.error?.ErrorMasseges;
  if (Array.isArray(msgs) && msgs.length) return msgs.join('; ');
  if (typeof e?.error?.message === 'string') return e.error.message;
  if (typeof e?.message === 'string') return e.message;
  return 'Request failed';
}

function normalizeListRow(raw: Record<string, unknown>): SupervisorVisitListItemDto {
  return {
    supervisorVisitID: Number(raw['supervisorVisitID'] ?? raw['SupervisorVisitID']),
    schoolID: Number(raw['schoolID'] ?? raw['SchoolID']),
    academicYearID: Number(raw['academicYearID'] ?? raw['AcademicYearID']),
    visitedTeacherID: Number(raw['visitedTeacherID'] ?? raw['VisitedTeacherID']),
    visitedTeacherName: String(raw['visitedTeacherName'] ?? raw['VisitedTeacherName'] ?? ''),
    classID: raw['classID'] != null ? Number(raw['classID']) : null,
    className: (raw['className'] ?? raw['ClassName']) as string | null | undefined,
    subjectID: raw['subjectID'] != null ? Number(raw['subjectID']) : null,
    subjectName: (raw['subjectName'] ?? raw['SubjectName']) as string | null | undefined,
    supervisorEmployeeProfileID: Number(raw['supervisorEmployeeProfileID'] ?? raw['SupervisorEmployeeProfileID']),
    supervisorName: String(raw['supervisorName'] ?? raw['SupervisorName'] ?? ''),
    visitDate: String(raw['visitDate'] ?? raw['VisitDate'] ?? ''),
    status: Number(raw['status'] ?? raw['Status'] ?? 0),
    overallScoreOutOf100: Number(raw['overallScoreOutOf100'] ?? raw['OverallScoreOutOf100'] ?? 0),
  };
}

function normalizeFollowUp(raw: Record<string, unknown>): RecommendationFollowUpReadDto {
  return {
    recommendationFollowUpID: Number(raw['recommendationFollowUpID'] ?? raw['RecommendationFollowUpID']),
    visitRecommendationID: Number(raw['visitRecommendationID'] ?? raw['VisitRecommendationID']),
    followUpNote: String(raw['followUpNote'] ?? raw['FollowUpNote'] ?? ''),
    followUpDate: String(raw['followUpDate'] ?? raw['FollowUpDate'] ?? ''),
    followUpByEmployeeProfileID:
      raw['followUpByEmployeeProfileID'] != null ? Number(raw['followUpByEmployeeProfileID']) : null,
    createdAtUtc: String(raw['createdAtUtc'] ?? raw['CreatedAtUtc'] ?? ''),
  };
}

function normalizeObservation(raw: Record<string, unknown>): VisitObservationReadDto {
  return {
    visitObservationID: Number(raw['visitObservationID'] ?? raw['VisitObservationID']),
    supervisorVisitID: Number(raw['supervisorVisitID'] ?? raw['SupervisorVisitID']),
    category: (raw['category'] ?? raw['Category']) as string | null | undefined,
    observationText: String(raw['observationText'] ?? raw['ObservationText'] ?? ''),
    sortOrder: Number(raw['sortOrder'] ?? raw['SortOrder'] ?? 0),
    createdAtUtc: String(raw['createdAtUtc'] ?? raw['CreatedAtUtc'] ?? ''),
  };
}

function normalizeRecommendation(raw: Record<string, unknown>): VisitRecommendationReadDto {
  const followUpsRaw = (raw['followUps'] ?? raw['FollowUps']) as Record<string, unknown>[] | undefined;
  return {
    visitRecommendationID: Number(raw['visitRecommendationID'] ?? raw['VisitRecommendationID']),
    supervisorVisitID: Number(raw['supervisorVisitID'] ?? raw['SupervisorVisitID']),
    recommendationText: String(raw['recommendationText'] ?? raw['RecommendationText'] ?? ''),
    implementationStatus: Number(raw['implementationStatus'] ?? raw['ImplementationStatus'] ?? 0),
    dueDate: raw['dueDate'] != null ? String(raw['dueDate']) : null,
    completedAtUtc: raw['completedAtUtc'] != null ? String(raw['completedAtUtc']) : null,
    sortOrder: Number(raw['sortOrder'] ?? raw['SortOrder'] ?? 0),
    createdAtUtc: String(raw['createdAtUtc'] ?? raw['CreatedAtUtc'] ?? ''),
    updatedAtUtc: String(raw['updatedAtUtc'] ?? raw['UpdatedAtUtc'] ?? ''),
    followUps: Array.isArray(followUpsRaw) ? followUpsRaw.map((f) => normalizeFollowUp(f as Record<string, unknown>)) : [],
  };
}

function normalizeDetail(raw: Record<string, unknown>): SupervisorVisitDetailDto {
  const base = normalizeListRow(raw);
  const obs = (raw['observations'] ?? raw['Observations']) as Record<string, unknown>[] | undefined;
  const rec = (raw['recommendations'] ?? raw['Recommendations']) as Record<string, unknown>[] | undefined;
  return {
    ...base,
    summaryNotes: (raw['summaryNotes'] ?? raw['SummaryNotes']) as string | null | undefined,
    createdAtUtc: String(raw['createdAtUtc'] ?? raw['CreatedAtUtc'] ?? ''),
    updatedAtUtc: String(raw['updatedAtUtc'] ?? raw['UpdatedAtUtc'] ?? ''),
    observations: Array.isArray(obs) ? obs.map((o) => normalizeObservation(o)) : [],
    recommendations: Array.isArray(rec) ? rec.map((r) => normalizeRecommendation(r)) : [],
  };
}

@Injectable({ providedIn: 'root' })
export class SupervisorVisitsService {
  private readonly http = inject(HttpClient);
  private readonly api = inject(BackendAspService);

  private root(path: string): string {
    return `${this.api.baseUrl}/supervisor-visits${path}`;
  }

  list(filter: SupervisorVisitFilterDto): Observable<SupervisorVisitListItemDto[]> {
    const body = supervisorVisitFilterForApi(filter);
    return this.http.post<ApiResponse<unknown>>(this.root('/list'), body).pipe(
      map((r) => {
        const rows = unwrap<unknown[]>(r) ?? [];
        return rows.map((x) => normalizeListRow(x as Record<string, unknown>));
      }),
    );
  }

  getById(id: number): Observable<SupervisorVisitDetailDto> {
    return this.http.get<ApiResponse<unknown>>(this.root(`/${id}`)).pipe(
      map((r) => normalizeDetail(unwrap<Record<string, unknown>>(r) as Record<string, unknown>)),
    );
  }

  create(dto: SupervisorVisitWriteDto): Observable<number> {
    return this.http.post<ApiResponse<number>>(this.root(''), supervisorVisitWriteForApi(dto)).pipe(
      map((r) => Number(unwrap<number>(r))),
    );
  }

  update(id: number, dto: SupervisorVisitWriteDto): Observable<number> {
    return this.http.put<ApiResponse<number>>(this.root(`/${id}`), supervisorVisitWriteForApi(dto)).pipe(
      map((r) => Number(unwrap<number>(r))),
    );
  }

  delete(id: number): Observable<void> {
    return this.http.delete<ApiResponse<boolean>>(this.root(`/${id}`)).pipe(map(() => undefined));
  }
}
