import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, catchError, map, of, shareReplay } from 'rxjs';

import { BackendAspService } from 'app/ASP.NET/backend-asp.service';
import { ApiResponse } from 'app/core/models/response.model';
import { PagedResultDto } from 'app/core/models/students.model';

import {
  EmployeeDocumentDto,
  EmployeeHistoryDto,
  EmployeeLeaveDto,
  EmployeePerformanceSummaryDto,
  EmployeeProfileCreateDto,
  EmployeeProfileFullDto,
  employeeProfileListFilterForPostApi,
  EmployeeProfileListFilterDto,
  EmployeeProfileOptionDto,
  EmployeeProfilePageRequestDto,
  employeeProfilePageRequestForPostApi,
  EmployeeProfileReadDto,
  EmployeeProfileUpdateDto,
  EmployeeQualificationDto,
  EmployeeSpecializationDto,
  EmployeeJobTypeDto,
} from './employees-hr.models';

function unwrap<T>(body: ApiResponse<T> | Record<string, unknown>): T {
  const b = body as Record<string, unknown>;
  const ok = (b['isSuccess'] ?? b['IsSuccess']) !== false;
  const errs = (b['errorMasseges'] ?? b['ErrorMasseges']) as string[] | undefined;
  if (!ok && errs?.length) {
    throw new Error(errs.join('; '));
  }
  return (b['result'] ?? b['Result']) as T;
}

export function readHttpError(err: unknown): string {
  const e = err as HttpErrorResponse;
  const msgs = e?.error?.errorMasseges ?? e?.error?.ErrorMasseges;
  if (Array.isArray(msgs) && msgs.length) return msgs.join('; ');
  if (typeof e?.error?.message === 'string') return e.error.message;
  if (typeof e?.message === 'string') return e.message;
  return 'Request failed';
}

@Injectable({ providedIn: 'root' })
export class EmployeesHrService {
  private readonly http = inject(HttpClient);
  private readonly api = inject(BackendAspService);

  /** Cached successful (or empty fallback) job types — avoids duplicate HTTP calls across HR screens. */
  private jobTypes$: Observable<EmployeeJobTypeDto[]> | null = null;

  /** Clears cached job types (e.g. after tenant admin changes lookup data — optional). */
  clearEmployeeJobTypesCache(): void {
    this.jobTypes$ = null;
  }

  getEmployeeJobTypes(): Observable<EmployeeJobTypeDto[]> {
    if (!this.jobTypes$) {
      this.jobTypes$ = this.http
        .get<ApiResponse<EmployeeJobTypeDto[]>>(`${this.api.baseUrl}/employees/job-types`)
        .pipe(
          map((r) => unwrap<EmployeeJobTypeDto[]>(r) ?? []),
          catchError(() => of([] as EmployeeJobTypeDto[])),
          shareReplay(1),
        );
    }
    return this.jobTypes$;
  }

  getEmployees(filter?: EmployeeProfileListFilterDto | null): Observable<EmployeeProfileReadDto[]> {
    const body = employeeProfileListFilterForPostApi(filter ?? {});
    return this.http
      .post<ApiResponse<EmployeeProfileReadDto[]>>(`${this.api.baseUrl}/employees/list`, body)
      .pipe(map((r) => unwrap<EmployeeProfileReadDto[]>(r) ?? []));
  }

  /** Paged employees (id + fullName). Zero-based pageIndex. */
  getEmployeesPage(body: EmployeeProfilePageRequestDto): Observable<PagedResultDto<EmployeeProfileOptionDto>> {
    const payload = employeeProfilePageRequestForPostApi(body);
    return this.http
      .post<ApiResponse<PagedResultDto<EmployeeProfileOptionDto>>>(`${this.api.baseUrl}/employees/page`, payload)
      .pipe(map((r) => unwrap<PagedResultDto<EmployeeProfileOptionDto>>(r)));
  }

  getEmployeeById(id: number): Observable<EmployeeProfileReadDto> {
    return this.http.get<ApiResponse<EmployeeProfileReadDto>>(`${this.api.baseUrl}/employees/${id}`).pipe(
      map((r) => unwrap<EmployeeProfileReadDto>(r)),
    );
  }

  getEmployeeFullProfile(id: number): Observable<EmployeeProfileFullDto> {
    return this.http
      .get<ApiResponse<EmployeeProfileFullDto>>(`${this.api.baseUrl}/employees/${id}/full-profile`)
      .pipe(map((r) => unwrap<EmployeeProfileFullDto>(r)));
  }

  createEmployee(payload: EmployeeProfileCreateDto): Observable<EmployeeProfileReadDto> {
    return this.http.post<ApiResponse<EmployeeProfileReadDto>>(`${this.api.baseUrl}/employees`, payload).pipe(
      map((r) => unwrap<EmployeeProfileReadDto>(r)),
    );
  }

  updateEmployee(id: number, payload: EmployeeProfileUpdateDto): Observable<EmployeeProfileReadDto> {
    return this.http.put<ApiResponse<EmployeeProfileReadDto>>(`${this.api.baseUrl}/employees/${id}`, payload).pipe(
      map((r) => unwrap<EmployeeProfileReadDto>(r)),
    );
  }

  deactivateEmployee(id: number): Observable<void> {
    return this.http.delete(`${this.api.baseUrl}/employees/${id}`, { observe: 'response' }).pipe(
      map((res) => {
        if (res.status === 204 || res.status === 200) return void 0;
        throw new Error('Unexpected status');
      }),
    );
  }

  addQualification(employeeId: number, payload: EmployeeQualificationDto): Observable<EmployeeQualificationDto> {
    return this.http
      .post<ApiResponse<EmployeeQualificationDto>>(
        `${this.api.baseUrl}/employees/${employeeId}/qualifications`,
        payload,
      )
      .pipe(map((r) => unwrap<EmployeeQualificationDto>(r)));
  }

  addSpecialization(employeeId: number, payload: EmployeeSpecializationDto): Observable<EmployeeSpecializationDto> {
    return this.http
      .post<ApiResponse<EmployeeSpecializationDto>>(
        `${this.api.baseUrl}/employees/${employeeId}/specializations`,
        payload,
      )
      .pipe(map((r) => unwrap<EmployeeSpecializationDto>(r)));
  }

  addHistory(employeeId: number, payload: EmployeeHistoryDto): Observable<EmployeeHistoryDto> {
    return this.http
      .post<ApiResponse<EmployeeHistoryDto>>(`${this.api.baseUrl}/employees/${employeeId}/history`, payload)
      .pipe(map((r) => unwrap<EmployeeHistoryDto>(r)));
  }

  addDocument(employeeId: number, payload: EmployeeDocumentDto): Observable<EmployeeDocumentDto> {
    return this.http
      .post<ApiResponse<EmployeeDocumentDto>>(`${this.api.baseUrl}/employees/${employeeId}/documents`, payload)
      .pipe(map((r) => unwrap<EmployeeDocumentDto>(r)));
  }

  addLeave(employeeId: number, payload: EmployeeLeaveDto): Observable<EmployeeLeaveDto> {
    return this.http
      .post<ApiResponse<EmployeeLeaveDto>>(`${this.api.baseUrl}/employees/${employeeId}/leaves`, payload)
      .pipe(map((r) => unwrap<EmployeeLeaveDto>(r)));
  }

  addPerformanceSummary(
    employeeId: number,
    payload: EmployeePerformanceSummaryDto,
  ): Observable<EmployeePerformanceSummaryDto> {
    return this.http
      .post<ApiResponse<EmployeePerformanceSummaryDto>>(
        `${this.api.baseUrl}/employees/${employeeId}/performance-summaries`,
        payload,
      )
      .pipe(map((r) => unwrap<EmployeePerformanceSummaryDto>(r)));
  }
}
