import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map } from 'rxjs/operators';
import type { Observable } from 'rxjs';

import { BackendAspService } from 'app/ASP.NET/backend-asp.service';
import { ApiResponse } from 'app/core/models/response.model';

import {
  MeetingDetailDto,
  MeetingFilterDto,
  MeetingListItemDto,
  MeetingMinutesWriteDto,
  MeetingTaskWriteDto,
  MeetingWriteDto,
  meetingFilterForApi,
  meetingMinutesWriteForApi,
  meetingTasksWriteForApi,
  meetingWriteForApi,
} from './meetings.models';

function unwrap<T>(body: ApiResponse<T> | Record<string, unknown>): T {
  const b = body as Record<string, unknown>;
  const ok = (b['isSuccess'] ?? b['IsSuccess']) !== false;
  const errs = (b['errorMasseges'] ?? b['ErrorMasseges']) as string[] | undefined;
  if (!ok && errs?.length) {
    throw new Error(errs.join('; '));
  }
  return (b['result'] ?? b['Result']) as T;
}

export function readMeetingHttpError(err: unknown): string {
  const e = err as HttpErrorResponse;
  const msgs = e?.error?.errorMasseges ?? e?.error?.ErrorMasseges;
  if (Array.isArray(msgs) && msgs.length) return msgs.join('; ');
  if (typeof e?.error?.message === 'string') return e.error.message;
  if (typeof e?.message === 'string') return e.message;
  return 'Request failed';
}

function normalizeListItem(raw: Record<string, unknown>): MeetingListItemDto {
  return {
    meetingID: Number(raw['meetingID'] ?? raw['MeetingID']),
    schoolID: Number(raw['schoolID'] ?? raw['SchoolID']),
    academicYearID: Number(raw['academicYearID'] ?? raw['AcademicYearID']),
    title: String(raw['title'] ?? raw['Title'] ?? ''),
    status: Number(raw['status'] ?? raw['Status'] ?? 0),
    startAtUtc: String(raw['startAtUtc'] ?? raw['StartAtUtc'] ?? ''),
    endAtUtc: raw['endAtUtc'] != null ? String(raw['endAtUtc'] ?? raw['EndAtUtc']) : null,
    organizerEmployeeProfileID: Number(raw['organizerEmployeeProfileID'] ?? raw['OrganizerEmployeeProfileID'] ?? 0),
    organizerName: String(raw['organizerName'] ?? raw['OrganizerName'] ?? ''),
    createdAtUtc: String(raw['createdAtUtc'] ?? raw['CreatedAtUtc'] ?? ''),
    updatedAtUtc: String(raw['updatedAtUtc'] ?? raw['UpdatedAtUtc'] ?? ''),
    attendeeCount: Number(raw['attendeeCount'] ?? raw['AttendeeCount'] ?? 0),
  };
}

function normalizeFollowUp(raw: Record<string, unknown>) {
  return {
    meetingTaskFollowUpID: Number(raw['meetingTaskFollowUpID'] ?? raw['MeetingTaskFollowUpID']),
    meetingTaskID: Number(raw['meetingTaskID'] ?? raw['MeetingTaskID']),
    note: String(raw['note'] ?? raw['Note'] ?? ''),
    progressPercent: raw['progressPercent'] != null ? Number(raw['progressPercent'] ?? raw['ProgressPercent']) : null,
    authorEmployeeProfileID:
      raw['authorEmployeeProfileID'] != null ? Number(raw['authorEmployeeProfileID'] ?? raw['AuthorEmployeeProfileID']) : null,
    authorName: (raw['authorName'] ?? raw['AuthorName']) as string | null | undefined,
    createdAtUtc: String(raw['createdAtUtc'] ?? raw['CreatedAtUtc'] ?? ''),
  };
}

function normalizeTask(raw: Record<string, unknown>): MeetingDetailDto['tasks'][0] {
  const fus = (raw['followUps'] ?? raw['FollowUps']) as Record<string, unknown>[] | undefined;
  return {
    meetingTaskID: Number(raw['meetingTaskID'] ?? raw['MeetingTaskID']),
    meetingID: Number(raw['meetingID'] ?? raw['MeetingID']),
    title: String(raw['title'] ?? raw['Title'] ?? ''),
    details: (raw['details'] ?? raw['Details']) as string | null | undefined,
    assignedToEmployeeProfileID:
      raw['assignedToEmployeeProfileID'] != null ? Number(raw['assignedToEmployeeProfileID'] ?? raw['AssignedToEmployeeProfileID']) : null,
    assignedToName: (raw['assignedToName'] ?? raw['AssignedToName']) as string | null | undefined,
    dueAtUtc: raw['dueAtUtc'] != null ? String(raw['dueAtUtc'] ?? raw['DueAtUtc']) : null,
    status: Number(raw['status'] ?? raw['Status'] ?? 0),
    sortOrder: Number(raw['sortOrder'] ?? raw['SortOrder'] ?? 0),
    createdAtUtc: String(raw['createdAtUtc'] ?? raw['CreatedAtUtc'] ?? ''),
    updatedAtUtc: String(raw['updatedAtUtc'] ?? raw['UpdatedAtUtc'] ?? ''),
    followUps: Array.isArray(fus) ? fus.map((x) => normalizeFollowUp(x as Record<string, unknown>)) : [],
  };
}

function normalizeDetail(raw: Record<string, unknown>): MeetingDetailDto {
  const base = normalizeListItem(raw);
  const atts = (raw['attendees'] ?? raw['Attendees']) as Record<string, unknown>[] | undefined;
  const mins = raw['minutes'] ?? raw['Minutes'];
  const tasks = (raw['tasks'] ?? raw['Tasks']) as Record<string, unknown>[] | undefined;
  return {
    ...base,
    description: (raw['description'] ?? raw['Description']) as string | null | undefined,
    location: (raw['location'] ?? raw['Location']) as string | null | undefined,
    attendees: Array.isArray(atts)
      ? atts.map((a) => ({
          meetingAttendeeID: Number(a['meetingAttendeeID'] ?? a['MeetingAttendeeID']),
          employeeProfileID: Number(a['employeeProfileID'] ?? a['EmployeeProfileID']),
          employeeName: String(a['employeeName'] ?? a['EmployeeName'] ?? ''),
          role: Number(a['role'] ?? a['Role'] ?? 0),
          response: Number(a['response'] ?? a['Response'] ?? 0),
          notes: (a['notes'] ?? a['Notes']) as string | null | undefined,
          createdAtUtc: String(a['createdAtUtc'] ?? a['CreatedAtUtc'] ?? ''),
          updatedAtUtc: String(a['updatedAtUtc'] ?? a['UpdatedAtUtc'] ?? ''),
        }))
      : [],
    minutes:
      mins && typeof mins === 'object'
        ? {
            meetingMinutesID: Number((mins as Record<string, unknown>)['meetingMinutesID'] ?? (mins as Record<string, unknown>)['MeetingMinutesID']),
            meetingID: Number((mins as Record<string, unknown>)['meetingID'] ?? (mins as Record<string, unknown>)['MeetingID']),
            body: String((mins as Record<string, unknown>)['body'] ?? (mins as Record<string, unknown>)['Body'] ?? ''),
            recordedByEmployeeProfileID: Number(
              (mins as Record<string, unknown>)['recordedByEmployeeProfileID'] ??
                (mins as Record<string, unknown>)['RecordedByEmployeeProfileID'] ??
                0,
            ),
            recordedByName: String((mins as Record<string, unknown>)['recordedByName'] ?? (mins as Record<string, unknown>)['RecordedByName'] ?? ''),
            recordedAtUtc: String((mins as Record<string, unknown>)['recordedAtUtc'] ?? (mins as Record<string, unknown>)['RecordedAtUtc'] ?? ''),
            updatedAtUtc: String((mins as Record<string, unknown>)['updatedAtUtc'] ?? (mins as Record<string, unknown>)['UpdatedAtUtc'] ?? ''),
            approvedByEmployeeProfileID:
              (mins as Record<string, unknown>)['approvedByEmployeeProfileID'] != null
                ? Number(
                    (mins as Record<string, unknown>)['approvedByEmployeeProfileID'] ??
                      (mins as Record<string, unknown>)['ApprovedByEmployeeProfileID'],
                  )
                : null,
            approvedByName: ((mins as Record<string, unknown>)['approvedByName'] ?? (mins as Record<string, unknown>)['ApprovedByName']) as
              | string
              | null
              | undefined,
            approvedAtUtc:
              (mins as Record<string, unknown>)['approvedAtUtc'] != null
                ? String((mins as Record<string, unknown>)['approvedAtUtc'] ?? (mins as Record<string, unknown>)['ApprovedAtUtc'])
                : null,
          }
        : null,
    tasks: Array.isArray(tasks) ? tasks.map((t) => normalizeTask(t as Record<string, unknown>)) : [],
  };
}

@Injectable({ providedIn: 'root' })
export class MeetingsService {
  private readonly http = inject(HttpClient);
  private readonly api = inject(BackendAspService);

  private root(path: string): string {
    return `${this.api.baseUrl}/meetings${path}`;
  }

  listMeetings(filter: MeetingFilterDto): Observable<MeetingListItemDto[]> {
    return this.http.post<ApiResponse<unknown>>(this.root('/list'), meetingFilterForApi(filter)).pipe(
      map((r) => {
        const rows = unwrap<unknown[]>(r) ?? [];
        return rows.map((x) => normalizeListItem(x as Record<string, unknown>));
      }),
    );
  }

  getMeeting(id: number): Observable<MeetingDetailDto> {
    return this.http.get<ApiResponse<unknown>>(this.root(`/${id}`)).pipe(
      map((r) => normalizeDetail(unwrap<Record<string, unknown>>(r) as Record<string, unknown>)),
    );
  }

  createMeeting(dto: MeetingWriteDto): Observable<number> {
    return this.http.post<ApiResponse<number>>(this.root(''), meetingWriteForApi(dto)).pipe(map((res) => Number(unwrap<number>(res))));
  }

  updateMeeting(id: number, dto: MeetingWriteDto): Observable<number> {
    return this.http.put<ApiResponse<number>>(this.root(`/${id}`), meetingWriteForApi(dto)).pipe(map((res) => Number(unwrap<number>(res))));
  }

  upsertMinutes(meetingId: number, dto: MeetingMinutesWriteDto): Observable<number> {
    return this.http
      .put<ApiResponse<number>>(this.root(`/${meetingId}/minutes`), meetingMinutesWriteForApi(dto))
      .pipe(map((res) => Number(unwrap<number>(res))));
  }

  replaceTasks(meetingId: number, tasks: MeetingTaskWriteDto[]): Observable<number> {
    return this.http
      .put<ApiResponse<number>>(this.root(`/${meetingId}/tasks`), meetingTasksWriteForApi(tasks))
      .pipe(map((res) => Number(unwrap<number>(res))));
  }
}
