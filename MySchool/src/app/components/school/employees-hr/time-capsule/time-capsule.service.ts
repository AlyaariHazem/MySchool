import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';

import { BackendAspService } from 'app/ASP.NET/backend-asp.service';
import { ApiResponse } from 'app/core/models/response.model';

import {
  ResignationRequestCreateDto,
  TimeCapsuleDetailDto,
  TimeCapsuleStatusDto,
} from './time-capsule.models';

function unwrap<T>(body: ApiResponse<T> | Record<string, unknown>): T {
  const b = body as Record<string, unknown>;
  const ok = (b['isSuccess'] ?? b['IsSuccess']) !== false;
  const errs = (b['errorMasseges'] ?? b['ErrorMasseges']) as string[] | undefined;
  if (!ok && errs?.length) {
    throw new Error(errs.join('; '));
  }
  return (b['result'] ?? b['Result']) as T;
}

@Injectable({ providedIn: 'root' })
export class TimeCapsuleService {
  private readonly http = inject(HttpClient);
  private readonly api = inject(BackendAspService);

  private base = `${this.api.baseUrl}/time-capsule`;

  getStatus(employeeId: number): Observable<TimeCapsuleStatusDto> {
    return this.http
      .get<ApiResponse<TimeCapsuleStatusDto>>(`${this.base}/status/${employeeId}`)
      .pipe(map((r) => unwrap<TimeCapsuleStatusDto>(r)));
  }

  getCapsule(employeeId: number): Observable<TimeCapsuleDetailDto> {
    return this.http
      .get<ApiResponse<TimeCapsuleDetailDto>>(`${this.base}/${employeeId}`)
      .pipe(map((r) => unwrap<TimeCapsuleDetailDto>(r)));
  }

  requestResignation(body: ResignationRequestCreateDto): Observable<unknown> {
    return this.http.post<ApiResponse<unknown>>(`${this.base}/resignation`, body).pipe(map((r) => unwrap(r)));
  }

  approveResignation(id: number, notes?: string | null): Observable<unknown> {
    return this.http
      .post<ApiResponse<unknown>>(`${this.base}/resignation/${id}/approve`, { notes: notes ?? null })
      .pipe(map((r) => unwrap(r)));
  }

  rejectResignation(id: number, notes?: string | null): Observable<unknown> {
    return this.http
      .post<ApiResponse<unknown>>(`${this.base}/resignation/${id}/reject`, { notes: notes ?? null })
      .pipe(map((r) => unwrap(r)));
  }

  approveUnlock(capsuleId: number, unlockReason?: string | null): Observable<unknown> {
    return this.http
      .post<ApiResponse<unknown>>(`${this.base}/unlock/${capsuleId}/approve`, {
        unlockReason: unlockReason ?? null,
      })
      .pipe(map((r) => unwrap(r)));
  }

  rejectUnlock(capsuleId: number, notes?: string | null): Observable<unknown> {
    return this.http
      .post<ApiResponse<unknown>>(`${this.base}/unlock/${capsuleId}/reject`, { notes: notes ?? null })
      .pipe(map((r) => unwrap(r)));
  }
}
