import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { BackendAspService } from '../../ASP.NET/backend-asp.service';
import { ApiResponse } from '../models/response.model';
import {
  CreateHomeworkTask,
  HomeworkActivitySummary,
  HomeworkSubmissionRow,
  HomeworkTaskDetail,
  HomeworkTaskList,
  ReviewHomeworkSubmission,
  GuardianStudentHomeworkRow,
  StudentHomeworkDetail,
  StudentHomeworkItem,
  StudentSubmitHomework,
} from '../models/homework.model';

@Injectable({ providedIn: 'root' })
export class HomeworkService {
  private readonly api = inject(BackendAspService);
  private readonly http = inject(HttpClient);

  private base(): string {
    return `${this.api.baseUrl}/Homework`;
  }

  private paramsFrom(obj: Record<string, string | number | boolean | null | undefined>): HttpParams {
    let p = new HttpParams();
    Object.entries(obj).forEach(([k, v]) => {
      if (v !== undefined && v !== null && v !== '') {
        p = p.set(k, String(v));
      }
    });
    return p;
  }

  // Teacher / privileged
  getTeacherTasks(filter: {
    yearID?: number;
    termID?: number;
    classID?: number;
    divisionID?: number;
    subjectID?: number;
    teacherID?: number;
  }): Observable<ApiResponse<HomeworkTaskList[]>> {
    return this.http.get<ApiResponse<HomeworkTaskList[]>>(`${this.base()}/teacher/tasks`, {
      params: this.paramsFrom(filter as Record<string, string | number | boolean | null | undefined>),
    });
  }

  getTeacherTask(id: number): Observable<ApiResponse<HomeworkTaskDetail>> {
    return this.http.get<ApiResponse<HomeworkTaskDetail>>(`${this.base()}/teacher/tasks/${id}`);
  }

  createTask(body: CreateHomeworkTask): Observable<ApiResponse<HomeworkTaskDetail>> {
    return this.http.post<ApiResponse<HomeworkTaskDetail>>(`${this.base()}/teacher/tasks`, body);
  }

  updateTask(id: number, body: CreateHomeworkTask & { homeworkTaskID: number }): Observable<ApiResponse<HomeworkTaskDetail>> {
    return this.http.put<ApiResponse<HomeworkTaskDetail>>(`${this.base()}/teacher/tasks/${id}`, body);
  }

  deleteTask(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base()}/teacher/tasks/${id}`);
  }

  getTaskSubmissions(taskId: number): Observable<ApiResponse<HomeworkSubmissionRow[]>> {
    return this.http.get<ApiResponse<HomeworkSubmissionRow[]>>(`${this.base()}/teacher/tasks/${taskId}/submissions`);
  }

  reviewSubmission(submissionId: number, body: ReviewHomeworkSubmission): Observable<ApiResponse<HomeworkSubmissionRow>> {
    return this.http.put<ApiResponse<HomeworkSubmissionRow>>(`${this.base()}/teacher/submissions/${submissionId}`, body);
  }

  // Student
  getStudentTasks(filter?: string): Observable<ApiResponse<StudentHomeworkItem[]>> {
    const params = filter ? new HttpParams().set('filter', filter) : new HttpParams();
    return this.http.get<ApiResponse<StudentHomeworkItem[]>>(`${this.base()}/student/tasks`, { params });
  }

  getStudentTask(taskId: number): Observable<ApiResponse<StudentHomeworkDetail>> {
    return this.http.get<ApiResponse<StudentHomeworkDetail>>(`${this.base()}/student/tasks/${taskId}`);
  }

  submitStudentTask(taskId: number, body: StudentSubmitHomework): Observable<ApiResponse<StudentHomeworkDetail>> {
    return this.http.post<ApiResponse<StudentHomeworkDetail>>(`${this.base()}/student/tasks/${taskId}/submit`, body);
  }

  // Guardian
  getGuardianAllTasks(filter?: string): Observable<ApiResponse<GuardianStudentHomeworkRow[]>> {
    const params = filter ? new HttpParams().set('filter', filter) : new HttpParams();
    return this.http.get<ApiResponse<GuardianStudentHomeworkRow[]>>(`${this.base()}/guardian/tasks`, { params });
  }

  getGuardianStudentTaskDetail(studentId: number, taskId: number): Observable<ApiResponse<StudentHomeworkDetail>> {
    return this.http.get<ApiResponse<StudentHomeworkDetail>>(
      `${this.base()}/guardian/students/${studentId}/tasks/${taskId}`,
    );
  }

  // Manager / admin
  getActivityReport(yearId: number, termId: number, classId?: number, teacherId?: number): Observable<ApiResponse<HomeworkActivitySummary>> {
    let p = new HttpParams().set('yearId', String(yearId)).set('termId', String(termId));
    if (classId != null) p = p.set('classId', String(classId));
    if (teacherId != null) p = p.set('teacherId', String(teacherId));
    return this.http.get<ApiResponse<HomeworkActivitySummary>>(`${this.base()}/reports/activity`, { params: p });
  }
}
