import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { BackendAspService } from '../../ASP.NET/backend-asp.service';
import { ApiResponse } from '../models/response.model';
import {
  CreateScheduledExam,
  ExamResultRow,
  ExamSession,
  ExamType,
  ScheduledExamList,
  StudentExamCard,
} from '../models/exams.model';

@Injectable({ providedIn: 'root' })
export class ExamsService {
  private readonly api = inject(BackendAspService);
  private readonly http = inject(HttpClient);

  private base(): string {
    return `${this.api.baseUrl}/Exams`;
  }

  getExamTypes(includeInactive = false): Observable<ApiResponse<ExamType[]>> {
    const params = new HttpParams().set('includeInactive', String(includeInactive));
    return this.http.get<ApiResponse<ExamType[]>>(`${this.base()}/types`, { params });
  }

  updateExamType(id: number, body: ExamType): Observable<ApiResponse<ExamType>> {
    return this.http.put<ApiResponse<ExamType>>(`${this.base()}/types/${id}`, body);
  }

  getSessions(yearId?: number, termId?: number): Observable<ApiResponse<ExamSession[]>> {
    let p = new HttpParams();
    if (yearId != null) p = p.set('yearId', String(yearId));
    if (termId != null) p = p.set('termId', String(termId));
    return this.http.get<ApiResponse<ExamSession[]>>(`${this.base()}/sessions`, { params: p });
  }

  createSession(body: { yearID: number; termID: number; name: string; isActive: boolean }): Observable<ApiResponse<ExamSession>> {
    return this.http.post<ApiResponse<ExamSession>>(`${this.base()}/sessions`, body);
  }

  getScheduled(filter: {
    yearID?: number;
    termID?: number;
    classID?: number;
    divisionID?: number;
    subjectID?: number;
    teacherID?: number;
    upcomingOnly?: boolean;
  }): Observable<ApiResponse<ScheduledExamList[]>> {
    let p = new HttpParams();
    Object.entries(filter).forEach(([k, v]) => {
      if (v !== undefined && v !== null) p = p.set(k, String(v));
    });
    return this.http.get<ApiResponse<ScheduledExamList[]>>(`${this.base()}/scheduled`, { params: p });
  }

  createScheduled(body: CreateScheduledExam): Observable<ApiResponse<ScheduledExamList>> {
    return this.http.post<ApiResponse<ScheduledExamList>>(`${this.base()}/scheduled`, body);
  }

  deleteScheduled(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base()}/scheduled/${id}`);
  }

  getTeacherMy(filter: Record<string, string | number | boolean | undefined>): Observable<ApiResponse<ScheduledExamList[]>> {
    let p = new HttpParams();
    Object.entries(filter).forEach(([k, v]) => {
      if (v !== undefined && v !== null) p = p.set(k, String(v));
    });
    return this.http.get<ApiResponse<ScheduledExamList[]>>(`${this.base()}/teacher/my`, { params: p });
  }

  getResults(scheduledExamId: number): Observable<ApiResponse<ExamResultRow[]>> {
    return this.http.get<ApiResponse<ExamResultRow[]>>(`${this.base()}/scheduled/${scheduledExamId}/results`);
  }

  saveResults(scheduledExamId: number, rows: ExamResultRow[]): Observable<ApiResponse<unknown>> {
    return this.http.put<ApiResponse<unknown>>(`${this.base()}/scheduled/${scheduledExamId}/results`, {
      rows: rows.map((r) => ({
        examResultID: r.examResultID,
        studentID: r.studentID,
        score: r.score,
        isAbsent: r.isAbsent,
        remarks: r.remarks ?? null,
      })),
    });
  }

  publishResults(scheduledExamId: number, publish = true): Observable<ApiResponse<unknown>> {
    const params = new HttpParams().set('publish', String(publish));
    return this.http.post<ApiResponse<unknown>>(`${this.base()}/scheduled/${scheduledExamId}/publish-results`, null, {
      params,
    });
  }

  publishSchedule(scheduledExamId: number, publish = true): Observable<ApiResponse<unknown>> {
    const params = new HttpParams().set('publish', String(publish));
    return this.http.post<ApiResponse<unknown>>(`${this.base()}/scheduled/${scheduledExamId}/publish-schedule`, null, {
      params,
    });
  }

  getStudentMy(upcomingOnly = false): Observable<ApiResponse<StudentExamCard[]>> {
    const params = new HttpParams().set('upcomingOnly', String(upcomingOnly));
    return this.http.get<ApiResponse<StudentExamCard[]>>(`${this.base()}/student/my`, { params });
  }

  getClassSheet(scheduledExamId: number): Observable<ApiResponse<unknown>> {
    return this.http.get<ApiResponse<unknown>>(`${this.base()}/reports/class-sheet/${scheduledExamId}`);
  }
}
