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
  DailyEvaluationsPageRequestDto,
  DailyEvaluationFullDto,
  DailyEvaluationItemCreateDto,
  DailyEvaluationItemReadDto,
  DailyEvaluationItemUpdateDto,
  DailyEvaluationListDto,
  DailyEvaluationReadDto,
  DailyEvaluationTemplateCreateDto,
  DailyEvaluationTemplateFilterDto,
  DailyEvaluationTemplateListDto,
  DailyEvaluationTemplatesPageRequestDto,
  PagedResultDto,
  DailyEvaluationTemplateReadDto,
  DailyEvaluationTemplateUpdateDto,
  DailyEvaluationUpdateDto,
  dailyEvaluationsFilterForPageApi,
  dailyEvalTemplatesFilterForPageApi,
  EvaluationLockCreateDto,
  EvaluationLockReadDto,
  EvaluationOverrideLogReadDto,
  EvaluationOverrideRequestDto,
  EvaluationReopenDto,
  TeacherEvaluationOptionDto,
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

function normalizeTeacherOptionDto(raw: Record<string, unknown>): TeacherEvaluationOptionDto {
  const id = raw['employeeProfileID'] ?? raw['EmployeeProfileID'];
  const name = raw['displayName'] ?? raw['DisplayName'] ?? '';
  const n = typeof id === 'number' ? id : Number(id);
  return {
    employeeProfileID: Number.isFinite(n) && n > 0 ? n : 0,
    displayName: typeof name === 'string' ? name : String(name),
  };
}

@Injectable({ providedIn: 'root' })
export class DailyEvaluationsService {
  private readonly http = inject(HttpClient);
  private readonly api = inject(BackendAspService);

  private root(path: string): string {
    return `${this.api.baseUrl}/daily-evaluations${path}`;
  }

  /**
   * Paged templates (POST body: pageIndex, pageSize, filter).
   * Use for grids; for dropdowns use {@link getTemplates} which requests the first page at max size.
   */
  getTemplatesPage(
    body: DailyEvaluationTemplatesPageRequestDto,
  ): Observable<PagedResultDto<DailyEvaluationTemplateListDto>> {
    const payload: DailyEvaluationTemplatesPageRequestDto = {
      ...body,
      filter: dailyEvalTemplatesFilterForPageApi(body.filter ?? {}),
    };
    return this.http
      .post<ApiResponse<PagedResultDto<DailyEvaluationTemplateListDto>>>(this.root('/templates/page'), payload)
      .pipe(map((r) => unwrap<PagedResultDto<DailyEvaluationTemplateListDto>>(r)));
  }

  /** All templates matching the filter (first page, server max page size). For selects and non-paged callers. */
  getTemplates(filter?: DailyEvaluationTemplateFilterDto | null): Observable<DailyEvaluationTemplateListDto[]> {
    return this.getTemplatesPage({
      pageIndex: 0,
      pageSize: 500,
      filter: filter ?? {},
    }).pipe(map((p) => p.data ?? []));
  }

  /** Teacher-only: HR employee profile id for the signed-in user (self-evaluation target). */
  getMyEmployeeProfileId(): Observable<number | null> {
    return this.http.get<ApiResponse<number>>(this.root('/me/employee-profile-id')).pipe(
      map((r) => {
        const v = unwrap<number>(r);
        return typeof v === 'number' && v > 0 ? v : null;
      }),
    );
  }

  /**
   * Teachers the student may evaluate: homeroom + course-plan teachers for the division for the school's active academic year (server-resolved).
   */
  getTeachersForStudentEvaluation(schoolId: number): Observable<TeacherEvaluationOptionDto[]> {
    const q = new HttpParams().set('schoolId', String(schoolId));
    return this.http.get<ApiResponse<Record<string, unknown>[]>>(this.root(`/for-student/teachers?${q.toString()}`)).pipe(
      map((r) => {
        const rows = unwrap<Record<string, unknown>[]>(r) ?? [];
        return rows.map((row) => normalizeTeacherOptionDto(row as Record<string, unknown>)).filter((x) => x.employeeProfileID > 0);
      }),
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

  /** Paged evaluations (POST body: pageIndex, pageSize, filter). */
  getEvaluationsPage(body: DailyEvaluationsPageRequestDto): Observable<PagedResultDto<DailyEvaluationListDto>> {
    const payload: DailyEvaluationsPageRequestDto = {
      ...body,
      filter: dailyEvaluationsFilterForPageApi(body.filter ?? {}),
    };
    return this.http
      .post<ApiResponse<PagedResultDto<DailyEvaluationListDto>>>(this.root('/page'), payload)
      .pipe(map((r) => unwrap<PagedResultDto<DailyEvaluationListDto>>(r)));
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
