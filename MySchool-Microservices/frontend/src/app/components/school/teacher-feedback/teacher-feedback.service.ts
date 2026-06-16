import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, type Observable } from 'rxjs';

import { BackendAspService } from 'app/ASP.NET/backend-asp.service';
import { ApiResponse } from 'app/core/models/response.model';

import {
  FeedbackQuestionDto,
  ParentFeedbackSubmitDto,
  StudentFeedbackSubmitDto,
  TeacherFeedbackCycleDetailDto,
  TeacherFeedbackCycleFilterDto,
  TeacherFeedbackCycleListItemDto,
  TeacherFeedbackCycleWriteDto,
  TeacherFeedbackOpenCycleDto,
  TeacherFeedbackParticipantFormDto,
} from './teacher-feedback.models';

function unwrap<T>(body: ApiResponse<T> | Record<string, unknown>): T {
  const b = body as Record<string, unknown>;
  const ok = (b['isSuccess'] ?? b['IsSuccess']) !== false;
  const errs = (b['errorMasseges'] ?? b['ErrorMasseges']) as string[] | undefined;
  if (!ok && errs?.length) throw new Error(errs.join('; '));
  return (b['result'] ?? b['Result']) as T;
}

export function readTeacherFeedbackHttpError(err: unknown): string {
  const e = err as HttpErrorResponse;
  const msgs = e?.error?.errorMasseges ?? e?.error?.ErrorMasseges;
  if (Array.isArray(msgs) && msgs.length) return msgs.join('; ');
  if (typeof e?.error?.message === 'string') return e.error.message;
  if (typeof e?.message === 'string') return e.message;
  return 'Request failed';
}

function normList(raw: Record<string, unknown>): TeacherFeedbackCycleListItemDto {
  return {
    teacherFeedbackCycleID: Number(raw['teacherFeedbackCycleID'] ?? raw['TeacherFeedbackCycleID']),
    schoolID: Number(raw['schoolID'] ?? raw['SchoolID']),
    academicYearID: Number(raw['academicYearID'] ?? raw['AcademicYearID']),
    teacherID: Number(raw['teacherID'] ?? raw['TeacherID']),
    teacherName: (raw['teacherName'] ?? raw['TeacherName']) as string | null | undefined,
    title: String(raw['title'] ?? raw['Title'] ?? ''),
    opensAtUtc: String(raw['opensAtUtc'] ?? raw['OpensAtUtc'] ?? ''),
    closesAtUtc: String(raw['closesAtUtc'] ?? raw['ClosesAtUtc'] ?? ''),
    status: Number(raw['status'] ?? raw['Status'] ?? 0),
    questionCount: Number(raw['questionCount'] ?? raw['QuestionCount'] ?? 0),
    studentSubmittedCount: Number(raw['studentSubmittedCount'] ?? raw['StudentSubmittedCount'] ?? 0),
    parentSubmittedCount: Number(raw['parentSubmittedCount'] ?? raw['ParentSubmittedCount'] ?? 0),
  };
}

function normQuestion(raw: Record<string, unknown>): FeedbackQuestionDto {
  return {
    feedbackQuestionID: Number(raw['feedbackQuestionID'] ?? raw['FeedbackQuestionID']),
    teacherFeedbackCycleID: Number(raw['teacherFeedbackCycleID'] ?? raw['TeacherFeedbackCycleID']),
    sortOrder: Number(raw['sortOrder'] ?? raw['SortOrder'] ?? 0),
    questionText: String(raw['questionText'] ?? raw['QuestionText'] ?? ''),
    questionType: Number(raw['questionType'] ?? raw['QuestionType'] ?? 1),
    audience: Number(raw['audience'] ?? raw['Audience'] ?? 3),
    isRequired: Boolean(raw['isRequired'] ?? raw['IsRequired'] ?? false),
  };
}

function normDetail(raw: Record<string, unknown>): TeacherFeedbackCycleDetailDto {
  const base = normList(raw);
  const qs = raw['questions'] ?? raw['Questions'];
  const sums = raw['summaries'] ?? raw['Summaries'];
  return {
    ...base,
    description: (raw['description'] ?? raw['Description']) as string | null | undefined,
    questions: Array.isArray(qs) ? (qs as Record<string, unknown>[]).map(normQuestion) : [],
    summaries: Array.isArray(sums)
      ? (sums as Record<string, unknown>[]).map((s) => ({
          feedbackSummaryID: Number(s['feedbackSummaryID'] ?? s['FeedbackSummaryID']),
          teacherFeedbackCycleID: Number(s['teacherFeedbackCycleID'] ?? s['TeacherFeedbackCycleID']),
          audience: Number(s['audience'] ?? s['Audience'] ?? 0),
          submittedCount: Number(s['submittedCount'] ?? s['SubmittedCount'] ?? 0),
          averageNumericScore:
            s['averageNumericScore'] != null || s['AverageNumericScore'] != null
              ? Number(s['averageNumericScore'] ?? s['AverageNumericScore'])
              : null,
          aggregateJson: (s['aggregateJson'] ?? s['AggregateJson']) as string | null | undefined,
          notes: (s['notes'] ?? s['Notes']) as string | null | undefined,
          computedAtUtc: String(s['computedAtUtc'] ?? s['ComputedAtUtc'] ?? ''),
        }))
      : [],
  };
}

function normOpen(raw: Record<string, unknown>): TeacherFeedbackOpenCycleDto {
  return {
    teacherFeedbackCycleID: Number(raw['teacherFeedbackCycleID'] ?? raw['TeacherFeedbackCycleID']),
    title: String(raw['title'] ?? raw['Title'] ?? ''),
    teacherName: (raw['teacherName'] ?? raw['TeacherName']) as string | null | undefined,
    closesAtUtc: String(raw['closesAtUtc'] ?? raw['ClosesAtUtc'] ?? ''),
  };
}

function normParticipantForm(raw: Record<string, unknown>): TeacherFeedbackParticipantFormDto {
  const qs = raw['questions'] ?? raw['Questions'];
  const ex = raw['existingResponses'] ?? raw['ExistingResponses'];
  return {
    teacherFeedbackCycleID: Number(raw['teacherFeedbackCycleID'] ?? raw['TeacherFeedbackCycleID']),
    title: String(raw['title'] ?? raw['Title'] ?? ''),
    teacherName: (raw['teacherName'] ?? raw['TeacherName']) as string | null | undefined,
    closesAtUtc: String(raw['closesAtUtc'] ?? raw['ClosesAtUtc'] ?? ''),
    questions: Array.isArray(qs) ? (qs as Record<string, unknown>[]).map(normQuestion) : [],
    existingResponses: Array.isArray(ex)
      ? (ex as Record<string, unknown>[]).map((r) => ({
          questionId: Number(r['questionId'] ?? r['QuestionId']),
          rating: r['rating'] != null ? Number(r['rating'] ?? r['Rating']) : null,
          text: (r['text'] ?? r['Text']) as string | null | undefined,
          yesNo: r['yesNo'] != null ? Boolean(r['yesNo'] ?? r['YesNo']) : null,
        }))
      : null,
    submissionStatus: Number(raw['submissionStatus'] ?? raw['SubmissionStatus'] ?? 0),
  };
}

function cycleWriteForApi(d: TeacherFeedbackCycleWriteDto): Record<string, unknown> {
  return {
    schoolID: d.schoolID,
    academicYearID: d.academicYearID,
    teacherID: d.teacherID,
    title: d.title,
    description: d.description ?? null,
    opensAtUtc: d.opensAtUtc,
    closesAtUtc: d.closesAtUtc,
    status: d.status,
    questions: (d.questions ?? []).map((q) => ({
      feedbackQuestionID: q.feedbackQuestionID ?? null,
      sortOrder: q.sortOrder,
      questionText: q.questionText,
      questionType: q.questionType,
      audience: q.audience,
      isRequired: q.isRequired,
    })),
  };
}

@Injectable({ providedIn: 'root' })
export class TeacherFeedbackService {
  private readonly http = inject(HttpClient);
  private readonly api = inject(BackendAspService);

  private root(path: string): string {
    return `${this.api.baseUrl}/teacher-feedback${path}`;
  }

  listCycles(filter: TeacherFeedbackCycleFilterDto): Observable<TeacherFeedbackCycleListItemDto[]> {
    const body: Record<string, unknown> = {};
    if (filter.schoolID != null && filter.schoolID > 0) body['schoolID'] = filter.schoolID;
    if (filter.academicYearID != null && filter.academicYearID > 0) body['academicYearID'] = filter.academicYearID;
    if (filter.teacherID != null && filter.teacherID > 0) body['teacherID'] = filter.teacherID;
    if (filter.status != null) body['status'] = filter.status;
    return this.http.post<ApiResponse<unknown>>(this.root('/cycles/list'), body).pipe(
      map((r) => {
        const rows = unwrap<unknown[]>(r) ?? [];
        return rows.map((x) => normList(x as Record<string, unknown>));
      }),
    );
  }

  getCycle(id: number): Observable<TeacherFeedbackCycleDetailDto> {
    return this.http.get<ApiResponse<unknown>>(this.root(`/cycles/${id}`)).pipe(
      map((r) => normDetail(unwrap<Record<string, unknown>>(r) as Record<string, unknown>)),
    );
  }

  createCycle(dto: TeacherFeedbackCycleWriteDto): Observable<number> {
    return this.http.post<ApiResponse<unknown>>(this.root('/cycles'), cycleWriteForApi(dto)).pipe(
      map((r) => {
        const o = unwrap<Record<string, unknown>>(r) as Record<string, unknown>;
        return Number(o['teacherFeedbackCycleID'] ?? o['TeacherFeedbackCycleID'] ?? 0);
      }),
    );
  }

  updateCycle(id: number, dto: TeacherFeedbackCycleWriteDto): Observable<void> {
    return this.http.put<ApiResponse<unknown>>(this.root(`/cycles/${id}`), cycleWriteForApi(dto)).pipe(map(() => undefined));
  }

  deleteCycle(id: number): Observable<void> {
    return this.http.delete<ApiResponse<unknown>>(this.root(`/cycles/${id}`)).pipe(map(() => undefined));
  }

  recomputeSummaries(id: number): Observable<void> {
    return this.http.post<ApiResponse<unknown>>(this.root(`/cycles/${id}/recompute-summaries`), {}).pipe(map(() => undefined));
  }

  /** STUDENT token */
  studentOpenCycles(): Observable<TeacherFeedbackOpenCycleDto[]> {
    return this.http.get<ApiResponse<unknown>>(this.root('/student/open-cycles')).pipe(
      map((r) => {
        const rows = unwrap<unknown[]>(r) ?? [];
        return rows.map((x) => normOpen(x as Record<string, unknown>));
      }),
    );
  }

  studentCycleForm(cycleId: number): Observable<TeacherFeedbackParticipantFormDto> {
    return this.http.get<ApiResponse<unknown>>(this.root(`/student/cycles/${cycleId}/form`)).pipe(
      map((r) => normParticipantForm(unwrap<Record<string, unknown>>(r) as Record<string, unknown>)),
    );
  }

  studentSubmit(dto: StudentFeedbackSubmitDto): Observable<void> {
    return this.http
      .post<ApiResponse<unknown>>(this.root('/student/submit'), {
        teacherFeedbackCycleID: dto.teacherFeedbackCycleID,
        submit: dto.submit,
        responses: dto.responses,
      })
      .pipe(map(() => undefined));
  }

  /** GUARDIAN token */
  parentOpenCycles(): Observable<TeacherFeedbackOpenCycleDto[]> {
    return this.http.get<ApiResponse<unknown>>(this.root('/parent/open-cycles')).pipe(
      map((r) => {
        const rows = unwrap<unknown[]>(r) ?? [];
        return rows.map((x) => normOpen(x as Record<string, unknown>));
      }),
    );
  }

  parentCycleForm(cycleId: number, studentId: number): Observable<TeacherFeedbackParticipantFormDto> {
    const params = new HttpParams().set('studentId', String(studentId));
    return this.http.get<ApiResponse<unknown>>(this.root(`/parent/cycles/${cycleId}/form`), { params }).pipe(
      map((r) => normParticipantForm(unwrap<Record<string, unknown>>(r) as Record<string, unknown>)),
    );
  }

  parentSubmit(dto: ParentFeedbackSubmitDto): Observable<void> {
    return this.http
      .post<ApiResponse<unknown>>(this.root('/parent/submit'), {
        teacherFeedbackCycleID: dto.teacherFeedbackCycleID,
        studentID: dto.studentID,
        submit: dto.submit,
        responses: dto.responses,
      })
      .pipe(map(() => undefined));
  }
}
