import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';

import { BackendAspService } from 'app/ASP.NET/backend-asp.service';
import { ApiResponse } from 'app/core/models/response.model';

import {
  DailyEvaluationCreateDto,
  DailyEvaluationCriteriaCreateDto,
  DailyEvaluationCriteriaReadDto,
  DailyEvaluationCriteriaUpdateDto,
  DailyEvaluationFilterDto,
  DailyEvaluationFullDto,
  DailyEvaluationItemCreateDto,
  DailyEvaluationItemReadDto,
  DailyEvaluationItemUpdateDto,
  DailyEvaluationListDto,
  DailyEvaluationReadDto,
  DailyEvaluationTemplateCreateDto,
  DailyEvaluationTemplateFilterDto,
  DailyEvaluationTemplateListDto,
  DailyEvaluationTemplateReadDto,
  DailyEvaluationTemplateUpdateDto,
  DailyEvaluationUpdateDto,
  EvaluationLockCreateDto,
  EvaluationLockReadDto,
  EvaluationOverrideLogReadDto,
  EvaluationOverrideRequestDto,
  EvaluationReopenDto,
} from './daily-evaluations.models';

function unwrap<T>(body: ApiResponse<T> | Record<string, unknown>): T {
  const b = body as Record<string, unknown>;
  const ok = (b['isSuccess'] ?? b['IsSuccess']) !== false;
  const errs = (b['errorMasseges'] ?? b['ErrorMasseges']) as string[] | undefined;
  if (!ok && errs?.length) {
    throw new Error(errs.join('; '));
  }
  return (b['result'] ?? b['Result']) as T;
}

export function readDailyEvalHttpError(err: unknown): string {
  const e = err as HttpErrorResponse;
  const msgs = e?.error?.errorMasseges ?? e?.error?.ErrorMasseges;
  if (Array.isArray(msgs) && msgs.length) return msgs.join('; ');
  if (typeof e?.error?.message === 'string') return e.error.message;
  if (typeof e?.message === 'string') return e.message;
  return 'Request failed';
}

function templateFilterParams(f?: DailyEvaluationTemplateFilterDto | null): HttpParams {
  let p = new HttpParams();
  if (!f) return p;
  if (f.schoolID != null && f.schoolID > 0) p = p.set('schoolID', String(f.schoolID));
  if (f.academicYearID != null && f.academicYearID > 0) p = p.set('academicYearID', String(f.academicYearID));
  if (f.employeeJobTypeID != null && f.employeeJobTypeID > 0) p = p.set('employeeJobTypeID', String(f.employeeJobTypeID));
  if (f.status != null) p = p.set('status', String(f.status));
  if (f.isActive != null) p = p.set('isActive', String(f.isActive));
  return p;
}

function evaluationFilterParams(f?: DailyEvaluationFilterDto | null): HttpParams {
  let p = new HttpParams();
  if (!f) return p;
  if (f.schoolID != null && f.schoolID > 0) p = p.set('schoolID', String(f.schoolID));
  if (f.academicYearID != null && f.academicYearID > 0) p = p.set('academicYearID', String(f.academicYearID));
  if (f.evaluatedEmployeeProfileID != null && f.evaluatedEmployeeProfileID > 0) {
    p = p.set('evaluatedEmployeeProfileID', String(f.evaluatedEmployeeProfileID));
  }
  if (f.dailyEvaluationTemplateID != null && f.dailyEvaluationTemplateID > 0) {
    p = p.set('dailyEvaluationTemplateID', String(f.dailyEvaluationTemplateID));
  }
  if (f.fromDate) p = p.set('fromDate', f.fromDate);
  if (f.toDate) p = p.set('toDate', f.toDate);
  if (f.status != null) p = p.set('status', String(f.status));
  return p;
}

@Injectable({ providedIn: 'root' })
export class DailyEvaluationsService {
  private readonly http = inject(HttpClient);
  private readonly api = inject(BackendAspService);

  private root(path: string): string {
    return `${this.api.baseUrl}/daily-evaluations${path}`;
  }

  getTemplates(filter?: DailyEvaluationTemplateFilterDto | null): Observable<DailyEvaluationTemplateListDto[]> {
    const q = templateFilterParams(filter);
    const qs = q.keys().length ? `?${q.toString()}` : '';
    return this.http.get<ApiResponse<DailyEvaluationTemplateListDto[]>>(this.root(`/templates${qs}`)).pipe(
      map((r) => unwrap<DailyEvaluationTemplateListDto[]>(r) ?? []),
    );
  }

  getTemplateById(id: number): Observable<DailyEvaluationTemplateReadDto> {
    return this.http.get<ApiResponse<DailyEvaluationTemplateReadDto>>(this.root(`/templates/${id}`)).pipe(
      map((r) => unwrap<DailyEvaluationTemplateReadDto>(r)),
    );
  }

  createTemplate(payload: DailyEvaluationTemplateCreateDto): Observable<DailyEvaluationTemplateReadDto> {
    return this.http.post<ApiResponse<DailyEvaluationTemplateReadDto>>(this.root('/templates'), payload).pipe(
      map((r) => unwrap<DailyEvaluationTemplateReadDto>(r)),
    );
  }

  updateTemplate(id: number, payload: DailyEvaluationTemplateUpdateDto): Observable<DailyEvaluationTemplateReadDto> {
    return this.http.put<ApiResponse<DailyEvaluationTemplateReadDto>>(this.root(`/templates/${id}`), payload).pipe(
      map((r) => unwrap<DailyEvaluationTemplateReadDto>(r)),
    );
  }

  activateTemplate(id: number): Observable<DailyEvaluationTemplateReadDto> {
    return this.http.post<ApiResponse<DailyEvaluationTemplateReadDto>>(this.root(`/templates/${id}/activate`), {}).pipe(
      map((r) => unwrap<DailyEvaluationTemplateReadDto>(r)),
    );
  }

  deactivateTemplate(id: number): Observable<DailyEvaluationTemplateReadDto> {
    return this.http.post<ApiResponse<DailyEvaluationTemplateReadDto>>(this.root(`/templates/${id}/deactivate`), {}).pipe(
      map((r) => unwrap<DailyEvaluationTemplateReadDto>(r)),
    );
  }

  archiveTemplate(id: number): Observable<DailyEvaluationTemplateReadDto> {
    return this.http.post<ApiResponse<DailyEvaluationTemplateReadDto>>(this.root(`/templates/${id}/archive`), {}).pipe(
      map((r) => unwrap<DailyEvaluationTemplateReadDto>(r)),
    );
  }

  getCriteria(templateId: number): Observable<DailyEvaluationCriteriaReadDto[]> {
    return this.http
      .get<ApiResponse<DailyEvaluationCriteriaReadDto[]>>(this.root(`/templates/${templateId}/criteria`))
      .pipe(map((r) => unwrap<DailyEvaluationCriteriaReadDto[]>(r) ?? []));
  }

  createCriteria(
    templateId: number,
    payload: DailyEvaluationCriteriaCreateDto,
  ): Observable<DailyEvaluationCriteriaReadDto> {
    return this.http
      .post<ApiResponse<DailyEvaluationCriteriaReadDto>>(this.root(`/templates/${templateId}/criteria`), payload)
      .pipe(map((r) => unwrap<DailyEvaluationCriteriaReadDto>(r)));
  }

  updateCriteria(
    criteriaId: number,
    payload: DailyEvaluationCriteriaUpdateDto,
  ): Observable<DailyEvaluationCriteriaReadDto> {
    return this.http
      .put<ApiResponse<DailyEvaluationCriteriaReadDto>>(this.root(`/criteria/${criteriaId}`), payload)
      .pipe(map((r) => unwrap<DailyEvaluationCriteriaReadDto>(r)));
  }

  getEvaluations(filter?: DailyEvaluationFilterDto | null): Observable<DailyEvaluationListDto[]> {
    const q = evaluationFilterParams(filter);
    const qs = q.keys().length ? `?${q.toString()}` : '';
    return this.http.get<ApiResponse<DailyEvaluationListDto[]>>(this.root(`${qs}`)).pipe(
      map((r) => unwrap<DailyEvaluationListDto[]>(r) ?? []),
    );
  }

  getEvaluationById(id: number): Observable<DailyEvaluationReadDto> {
    return this.http.get<ApiResponse<DailyEvaluationReadDto>>(this.root(`/${id}`)).pipe(
      map((r) => unwrap<DailyEvaluationReadDto>(r)),
    );
  }

  getEvaluationFull(id: number): Observable<DailyEvaluationFullDto> {
    return this.http.get<ApiResponse<DailyEvaluationFullDto>>(this.root(`/${id}/full`)).pipe(
      map((r) => unwrap<DailyEvaluationFullDto>(r)),
    );
  }

  createEvaluation(payload: DailyEvaluationCreateDto): Observable<DailyEvaluationReadDto> {
    return this.http.post<ApiResponse<DailyEvaluationReadDto>>(this.root(''), payload).pipe(
      map((r) => unwrap<DailyEvaluationReadDto>(r)),
    );
  }

  updateEvaluation(id: number, payload: DailyEvaluationUpdateDto): Observable<DailyEvaluationReadDto> {
    return this.http.put<ApiResponse<DailyEvaluationReadDto>>(this.root(`/${id}`), payload).pipe(
      map((r) => unwrap<DailyEvaluationReadDto>(r)),
    );
  }

  submitEvaluation(id: number): Observable<DailyEvaluationReadDto> {
    return this.http.post<ApiResponse<DailyEvaluationReadDto>>(this.root(`/${id}/submit`), {}).pipe(
      map((r) => unwrap<DailyEvaluationReadDto>(r)),
    );
  }

  createEvaluationItem(
    evaluationId: number,
    payload: DailyEvaluationItemCreateDto,
  ): Observable<DailyEvaluationItemReadDto> {
    return this.http
      .post<ApiResponse<DailyEvaluationItemReadDto>>(this.root(`/${evaluationId}/items`), payload)
      .pipe(map((r) => unwrap(r)));
  }

  updateEvaluationItem(itemId: number, payload: DailyEvaluationItemUpdateDto): Observable<DailyEvaluationItemReadDto> {
    return this.http
      .put<ApiResponse<DailyEvaluationItemReadDto>>(this.root(`/items/${itemId}`), payload)
      .pipe(map((r) => unwrap(r)));
  }

  getLockByDate(params: {
    schoolId: number;
    academicYearId: number;
    date: string;
    templateId?: number | null;
  }): Observable<EvaluationLockReadDto | null> {
    let p = new HttpParams()
      .set('schoolId', String(params.schoolId))
      .set('academicYearId', String(params.academicYearId))
      .set('date', params.date);
    if (params.templateId != null && params.templateId > 0) {
      p = p.set('templateId', String(params.templateId));
    }
    return this.http.get<ApiResponse<EvaluationLockReadDto | null>>(this.root(`/locks/by-date?${p.toString()}`)).pipe(
      map((r) => unwrap<EvaluationLockReadDto | null>(r) ?? null),
    );
  }

  createLock(payload: EvaluationLockCreateDto): Observable<EvaluationLockReadDto> {
    return this.http.post<ApiResponse<EvaluationLockReadDto>>(this.root('/locks'), payload).pipe(
      map((r) => unwrap<EvaluationLockReadDto>(r)),
    );
  }

  reopenLock(lockId: number, payload: EvaluationReopenDto): Observable<EvaluationLockReadDto> {
    return this.http.post<ApiResponse<EvaluationLockReadDto>>(this.root(`/locks/${lockId}/reopen`), payload).pipe(
      map((r) => unwrap<EvaluationLockReadDto>(r)),
    );
  }

  overrideUpdateAfterLock(evaluationId: number, payload: EvaluationOverrideRequestDto): Observable<DailyEvaluationReadDto> {
    return this.http
      .post<ApiResponse<DailyEvaluationReadDto>>(this.root(`/${evaluationId}/override-update`), payload)
      .pipe(map((r) => unwrap<DailyEvaluationReadDto>(r)));
  }

  getOverrideLogs(evaluationId: number): Observable<EvaluationOverrideLogReadDto[]> {
    return this.http
      .get<ApiResponse<EvaluationOverrideLogReadDto[]>>(this.root(`/${evaluationId}/override-logs`))
      .pipe(map((r) => unwrap<EvaluationOverrideLogReadDto[]>(r) ?? []));
  }
}
