import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map } from 'rxjs/operators';
import type { Observable } from 'rxjs';

import { BackendAspService } from 'app/ASP.NET/backend-asp.service';
import { ApiResponse } from 'app/core/models/response.model';

import {
  ActivityDetailDto,
  ActivityFilterDto,
  ActivityListItemDto,
  ActivityRequestWriteDto,
  activityFilterForApi,
  activityWriteForApi,
} from './activities.models';

function unwrap<T>(body: ApiResponse<T> | Record<string, unknown>): T {
  const b = body as Record<string, unknown>;
  const ok = (b['isSuccess'] ?? b['IsSuccess']) !== false;
  const errs = (b['errorMasseges'] ?? b['ErrorMasseges']) as string[] | undefined;
  if (!ok && errs?.length) {
    throw new Error(errs.join('; '));
  }
  return (b['result'] ?? b['Result']) as T;
}

export function readActivityHttpError(err: unknown): string {
  const e = err as HttpErrorResponse;
  const msgs = e?.error?.errorMasseges ?? e?.error?.ErrorMasseges;
  if (Array.isArray(msgs) && msgs.length) return msgs.join('; ');
  if (typeof e?.error?.message === 'string') return e.error.message;
  if (typeof e?.message === 'string') return e.message;
  return 'Request failed';
}

function normalizeListItem(raw: Record<string, unknown>): ActivityListItemDto {
  return {
    activityRequestID: Number(raw['activityRequestID'] ?? raw['ActivityRequestID']),
    schoolID: Number(raw['schoolID'] ?? raw['SchoolID']),
    academicYearID: Number(raw['academicYearID'] ?? raw['AcademicYearID']),
    employeeProfileID: Number(raw['employeeProfileID'] ?? raw['EmployeeProfileID']),
    employeeName: String(raw['employeeName'] ?? raw['EmployeeName'] ?? ''),
    title: String(raw['title'] ?? raw['Title'] ?? ''),
    status: Number(raw['status'] ?? raw['Status'] ?? 0),
    submittedAtUtc: String(raw['submittedAtUtc'] ?? raw['SubmittedAtUtc'] ?? ''),
    updatedAtUtc: String(raw['updatedAtUtc'] ?? raw['UpdatedAtUtc'] ?? ''),
    resolvedAtUtc: raw['resolvedAtUtc'] != null ? String(raw['resolvedAtUtc'] ?? raw['ResolvedAtUtc']) : null,
  };
}

function normalizeDetail(raw: Record<string, unknown>): ActivityDetailDto {
  const base = normalizeListItem(raw);
  const appr = (raw['approvals'] ?? raw['Approvals']) as Record<string, unknown>[] | undefined;
  const ex = (raw['executions'] ?? raw['Executions']) as Record<string, unknown>[] | undefined;
  const evs = (raw['evaluations'] ?? raw['Evaluations']) as Record<string, unknown>[] | undefined;
  const pts = (raw['points'] ?? raw['Points']) as Record<string, unknown>[] | undefined;
  return {
    ...base,
    details: (raw['details'] ?? raw['Details']) as string | null | undefined,
    approvals: Array.isArray(appr)
      ? appr.map((a) => ({
          activityApprovalID: Number(a['activityApprovalID'] ?? a['ActivityApprovalID']),
          activityRequestID: Number(a['activityRequestID'] ?? a['ActivityRequestID']),
          approverEmployeeProfileID: Number(a['approverEmployeeProfileID'] ?? a['ApproverEmployeeProfileID']),
          approverName: String(a['approverName'] ?? a['ApproverName'] ?? ''),
          sortOrder: Number(a['sortOrder'] ?? a['SortOrder'] ?? 0),
          decision: Number(a['decision'] ?? a['Decision'] ?? 0),
          comment: (a['comment'] ?? a['Comment']) as string | null | undefined,
          decidedAtUtc: a['decidedAtUtc'] != null ? String(a['decidedAtUtc'] ?? a['DecidedAtUtc']) : null,
          createdAtUtc: String(a['createdAtUtc'] ?? a['CreatedAtUtc'] ?? ''),
        }))
      : [],
    executions: Array.isArray(ex)
      ? ex.map((e) => ({
          activityExecutionID: Number(e['activityExecutionID'] ?? e['ActivityExecutionID']),
          activityRequestID: Number(e['activityRequestID'] ?? e['ActivityRequestID']),
          status: Number(e['status'] ?? e['Status'] ?? 0),
          notes: (e['notes'] ?? e['Notes']) as string | null | undefined,
          progressPercent: Number(e['progressPercent'] ?? e['ProgressPercent'] ?? 0),
          dueAtUtc: e['dueAtUtc'] != null ? String(e['dueAtUtc'] ?? e['DueAtUtc']) : null,
          executedAtUtc: e['executedAtUtc'] != null ? String(e['executedAtUtc'] ?? e['ExecutedAtUtc']) : null,
          updatedAtUtc: String(e['updatedAtUtc'] ?? e['UpdatedAtUtc'] ?? ''),
          responsibleEmployeeProfileID:
            e['responsibleEmployeeProfileID'] != null ? Number(e['responsibleEmployeeProfileID'] ?? e['ResponsibleEmployeeProfileID']) : null,
          responsibleName: (e['responsibleName'] ?? e['ResponsibleName']) as string | null | undefined,
        }))
      : [],
    evaluations: Array.isArray(evs)
      ? evs.map((ev) => ({
          activityEvaluationID: Number(ev['activityEvaluationID'] ?? ev['ActivityEvaluationID']),
          activityRequestID: Number(ev['activityRequestID'] ?? ev['ActivityRequestID']),
          evaluatorEmployeeProfileID: Number(ev['evaluatorEmployeeProfileID'] ?? ev['EvaluatorEmployeeProfileID']),
          evaluatorName: String(ev['evaluatorName'] ?? ev['EvaluatorName'] ?? ''),
          score: Number(ev['score'] ?? ev['Score'] ?? 0),
          feedback: (ev['feedback'] ?? ev['Feedback']) as string | null | undefined,
          createdAtUtc: String(ev['createdAtUtc'] ?? ev['CreatedAtUtc'] ?? ''),
        }))
      : [],
    points: Array.isArray(pts)
      ? pts.map((p) => ({
          activityPointsID: Number(p['activityPointsID'] ?? p['ActivityPointsID']),
          activityRequestID: Number(p['activityRequestID'] ?? p['ActivityRequestID']),
          points: Number(p['points'] ?? p['Points'] ?? 0),
          reason: (p['reason'] ?? p['Reason']) as string | null | undefined,
          awardedByEmployeeProfileID: Number(p['awardedByEmployeeProfileID'] ?? p['AwardedByEmployeeProfileID']),
          awardedByName: String(p['awardedByName'] ?? p['AwardedByName'] ?? ''),
          awardedAtUtc: String(p['awardedAtUtc'] ?? p['AwardedAtUtc'] ?? ''),
        }))
      : [],
  };
}

@Injectable({ providedIn: 'root' })
export class ActivitiesService {
  private readonly http = inject(HttpClient);
  private readonly api = inject(BackendAspService);

  private root(path: string): string {
    return `${this.api.baseUrl}/activities${path}`;
  }

  listActivities(filter: ActivityFilterDto): Observable<ActivityListItemDto[]> {
    return this.http.post<ApiResponse<unknown>>(this.root('/list'), activityFilterForApi(filter)).pipe(
      map((r) => {
        const rows = unwrap<unknown[]>(r) ?? [];
        return rows.map((x) => normalizeListItem(x as Record<string, unknown>));
      }),
    );
  }

  getActivity(id: number): Observable<ActivityDetailDto> {
    return this.http.get<ApiResponse<unknown>>(this.root(`/${id}`)).pipe(
      map((r) => normalizeDetail(unwrap<Record<string, unknown>>(r) as Record<string, unknown>)),
    );
  }

  createActivity(dto: ActivityRequestWriteDto): Observable<number> {
    return this.http.post<ApiResponse<number>>(this.root(''), activityWriteForApi(dto)).pipe(map((res) => Number(unwrap<number>(res))));
  }

  updateActivity(id: number, dto: ActivityRequestWriteDto): Observable<number> {
    return this.http.put<ApiResponse<number>>(this.root(`/${id}`), activityWriteForApi(dto)).pipe(map((res) => Number(unwrap<number>(res))));
  }
}
