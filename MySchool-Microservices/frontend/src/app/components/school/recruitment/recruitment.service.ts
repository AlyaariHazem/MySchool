import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';

import { BackendAspService } from 'app/ASP.NET/backend-asp.service';
import { ApiResponse } from 'app/core/models/response.model';

import {
  CandidateEvaluationCreateDto,
  CandidateEvaluationReadDto,
  CandidateEvaluationUpdateDto,
  HiringDecisionCreateDto,
  HiringDecisionReadDto,
  HiringDecisionUpdateDto,
  InterviewCreateDto,
  InterviewReadDto,
  InterviewUpdateDto,
  JobApplicationCreateDto,
  JobApplicationFilterDto,
  JobApplicationFullDto,
  JobApplicationReadDto,
  JobApplicationStatusMoveDto,
  JobApplicationUpdateDto,
  JobPostingCreateDto,
  JobPostingFilterDto,
  JobPostingListDto,
  JobPostingReadDto,
  JobPostingUpdateDto,
  JobApplicationListDto,
  ConvertApplicantToEmployeeDto,
  ConvertApplicantToEmployeeResultDto,
} from './recruitment.models';

function unwrap<T>(body: ApiResponse<T> | Record<string, unknown>): T {
  const b = body as Record<string, unknown>;
  const ok = (b['isSuccess'] ?? b['IsSuccess']) !== false;
  const errs = (b['errorMasseges'] ?? b['ErrorMasseges']) as string[] | undefined;
  if (!ok && errs?.length) {
    throw new Error(errs.join('; '));
  }
  return (b['result'] ?? b['Result']) as T;
}

export function readRecruitmentHttpError(err: unknown): string {
  const e = err as HttpErrorResponse;
  const msgs = e?.error?.errorMasseges ?? e?.error?.ErrorMasseges;
  if (Array.isArray(msgs) && msgs.length) return msgs.join('; ');
  if (typeof e?.error?.message === 'string') return e.error.message;
  if (typeof e?.message === 'string') return e.message;
  return 'Request failed';
}

function postingParams(f?: JobPostingFilterDto | null): HttpParams {
  let p = new HttpParams();
  if (!f) return p;
  if (f.schoolID != null && f.schoolID > 0) p = p.set('schoolID', String(f.schoolID));
  if (f.academicYearID != null && f.academicYearID > 0) p = p.set('academicYearID', String(f.academicYearID));
  if (f.employeeJobTypeID != null && f.employeeJobTypeID > 0) p = p.set('employeeJobTypeID', String(f.employeeJobTypeID));
  if (f.status != null) p = p.set('status', String(f.status));
  if (f.isActive != null) p = p.set('isActive', String(f.isActive));
  return p;
}

function applicationParams(f?: JobApplicationFilterDto | null): HttpParams {
  let p = new HttpParams();
  if (!f) return p;
  if (f.schoolID != null && f.schoolID > 0) p = p.set('schoolID', String(f.schoolID));
  if (f.academicYearID != null && f.academicYearID > 0) p = p.set('academicYearID', String(f.academicYearID));
  if (f.jobPostingID != null && f.jobPostingID > 0) p = p.set('jobPostingID', String(f.jobPostingID));
  if (f.status != null) p = p.set('status', String(f.status));
  if (f.email) p = p.set('email', f.email.trim());
  if (f.nationalID) p = p.set('nationalID', f.nationalID.trim());
  if (f.isActive != null) p = p.set('isActive', String(f.isActive));
  return p;
}

@Injectable({ providedIn: 'root' })
export class RecruitmentService {
  private readonly http = inject(HttpClient);
  private readonly api = inject(BackendAspService);

  private base(path: string): string {
    return `${this.api.baseUrl}/recruitment/${path}`;
  }

  // --- Job postings ---
  getJobPostings(filter?: JobPostingFilterDto | null): Observable<JobPostingListDto[]> {
    const q = postingParams(filter);
    const qs = q.keys().length ? `?${q.toString()}` : '';
    return this.http.get<ApiResponse<JobPostingListDto[]>>(`${this.base('job-postings')}${qs}`).pipe(
      map((r) => unwrap<JobPostingListDto[]>(r) ?? []),
    );
  }

  getJobPostingById(id: number): Observable<JobPostingReadDto> {
    return this.http.get<ApiResponse<JobPostingReadDto>>(this.base(`job-postings/${id}`)).pipe(
      map((r) => unwrap<JobPostingReadDto>(r)),
    );
  }

  createJobPosting(payload: JobPostingCreateDto): Observable<JobPostingReadDto> {
    return this.http.post<ApiResponse<JobPostingReadDto>>(this.base('job-postings'), payload).pipe(
      map((r) => unwrap<JobPostingReadDto>(r)),
    );
  }

  updateJobPosting(id: number, payload: JobPostingUpdateDto): Observable<JobPostingReadDto> {
    return this.http.put<ApiResponse<JobPostingReadDto>>(this.base(`job-postings/${id}`), payload).pipe(
      map((r) => unwrap<JobPostingReadDto>(r)),
    );
  }

  openJobPosting(id: number): Observable<JobPostingReadDto> {
    return this.http.post<ApiResponse<JobPostingReadDto>>(this.base(`job-postings/${id}/open`), {}).pipe(
      map((r) => unwrap<JobPostingReadDto>(r)),
    );
  }

  closeJobPosting(id: number): Observable<JobPostingReadDto> {
    return this.http.post<ApiResponse<JobPostingReadDto>>(this.base(`job-postings/${id}/close`), {}).pipe(
      map((r) => unwrap<JobPostingReadDto>(r)),
    );
  }

  archiveJobPosting(id: number): Observable<JobPostingReadDto> {
    return this.http.post<ApiResponse<JobPostingReadDto>>(this.base(`job-postings/${id}/archive`), {}).pipe(
      map((r) => unwrap<JobPostingReadDto>(r)),
    );
  }

  // --- Job applications ---
  getJobApplications(filter?: JobApplicationFilterDto | null): Observable<JobApplicationListDto[]> {
    const q = applicationParams(filter);
    const qs = q.keys().length ? `?${q.toString()}` : '';
    return this.http.get<ApiResponse<JobApplicationListDto[]>>(`${this.base('job-applications')}${qs}`).pipe(
      map((r) => unwrap<JobApplicationListDto[]>(r) ?? []),
    );
  }

  getJobApplicationById(id: number): Observable<JobApplicationReadDto> {
    return this.http.get<ApiResponse<JobApplicationReadDto>>(this.base(`job-applications/${id}`)).pipe(
      map((r) => unwrap<JobApplicationReadDto>(r)),
    );
  }

  getJobApplicationFull(id: number): Observable<JobApplicationFullDto> {
    return this.http.get<ApiResponse<JobApplicationFullDto>>(this.base(`job-applications/${id}/full`)).pipe(
      map((r) => unwrap<JobApplicationFullDto>(r)),
    );
  }

  createJobApplication(payload: JobApplicationCreateDto): Observable<JobApplicationReadDto> {
    return this.http.post<ApiResponse<JobApplicationReadDto>>(this.base('job-applications'), payload).pipe(
      map((r) => unwrap<JobApplicationReadDto>(r)),
    );
  }

  updateJobApplication(id: number, payload: JobApplicationUpdateDto): Observable<JobApplicationReadDto> {
    return this.http.put<ApiResponse<JobApplicationReadDto>>(this.base(`job-applications/${id}`), payload).pipe(
      map((r) => unwrap<JobApplicationReadDto>(r)),
    );
  }

  moveJobApplicationStatus(id: number, payload: JobApplicationStatusMoveDto): Observable<JobApplicationReadDto> {
    return this.http
      .post<ApiResponse<JobApplicationReadDto>>(this.base(`job-applications/${id}/status`), payload)
      .pipe(map((r) => unwrap<JobApplicationReadDto>(r)));
  }

  // --- Interviews ---
  getApplicationInterviews(applicationId: number): Observable<InterviewReadDto[]> {
    return this.http
      .get<ApiResponse<InterviewReadDto[]>>(this.base(`job-applications/${applicationId}/interviews`))
      .pipe(map((r) => unwrap<InterviewReadDto[]>(r) ?? []));
  }

  createInterview(applicationId: number, payload: InterviewCreateDto): Observable<InterviewReadDto> {
    return this.http
      .post<ApiResponse<InterviewReadDto>>(this.base(`job-applications/${applicationId}/interviews`), payload)
      .pipe(map((r) => unwrap<InterviewReadDto>(r)));
  }

  updateInterview(id: number, payload: InterviewUpdateDto): Observable<InterviewReadDto> {
    return this.http.put<ApiResponse<InterviewReadDto>>(this.base(`interviews/${id}`), payload).pipe(
      map((r) => unwrap<InterviewReadDto>(r)),
    );
  }

  completeInterview(id: number): Observable<InterviewReadDto> {
    return this.http.post<ApiResponse<InterviewReadDto>>(this.base(`interviews/${id}/complete`), {}).pipe(
      map((r) => unwrap<InterviewReadDto>(r)),
    );
  }

  cancelInterview(id: number): Observable<InterviewReadDto> {
    return this.http.post<ApiResponse<InterviewReadDto>>(this.base(`interviews/${id}/cancel`), {}).pipe(
      map((r) => unwrap<InterviewReadDto>(r)),
    );
  }

  noShowInterview(id: number): Observable<InterviewReadDto> {
    return this.http.post<ApiResponse<InterviewReadDto>>(this.base(`interviews/${id}/no-show`), {}).pipe(
      map((r) => unwrap<InterviewReadDto>(r)),
    );
  }

  // --- Evaluations ---
  getApplicationEvaluations(applicationId: number): Observable<CandidateEvaluationReadDto[]> {
    return this.http
      .get<ApiResponse<CandidateEvaluationReadDto[]>>(this.base(`job-applications/${applicationId}/evaluations`))
      .pipe(map((r) => unwrap<CandidateEvaluationReadDto[]>(r) ?? []));
  }

  createEvaluation(applicationId: number, payload: CandidateEvaluationCreateDto): Observable<CandidateEvaluationReadDto> {
    return this.http
      .post<ApiResponse<CandidateEvaluationReadDto>>(
        this.base(`job-applications/${applicationId}/evaluations`),
        payload,
      )
      .pipe(map((r) => unwrap<CandidateEvaluationReadDto>(r)));
  }

  updateEvaluation(id: number, payload: CandidateEvaluationUpdateDto): Observable<CandidateEvaluationReadDto> {
    return this.http.put<ApiResponse<CandidateEvaluationReadDto>>(this.base(`evaluations/${id}`), payload).pipe(
      map((r) => unwrap<CandidateEvaluationReadDto>(r)),
    );
  }

  // --- Hiring decisions ---
  getApplicationDecision(applicationId: number): Observable<HiringDecisionReadDto | null> {
    return this.http.get<ApiResponse<HiringDecisionReadDto | null>>(this.base(`job-applications/${applicationId}/decision`)).pipe(
      map((r) => unwrap<HiringDecisionReadDto | null>(r) ?? null),
    );
  }

  createDecision(applicationId: number, payload: HiringDecisionCreateDto): Observable<HiringDecisionReadDto> {
    return this.http
      .post<ApiResponse<HiringDecisionReadDto>>(this.base(`job-applications/${applicationId}/decision`), payload)
      .pipe(map((r) => unwrap<HiringDecisionReadDto>(r)));
  }

  updateDecision(decisionId: number, payload: HiringDecisionUpdateDto): Observable<HiringDecisionReadDto> {
    return this.http.put<ApiResponse<HiringDecisionReadDto>>(this.base(`decisions/${decisionId}`), payload).pipe(
      map((r) => unwrap<HiringDecisionReadDto>(r)),
    );
  }

  acceptApplication(applicationId: number, payload: HiringDecisionCreateDto): Observable<HiringDecisionReadDto> {
    return this.http
      .post<ApiResponse<HiringDecisionReadDto>>(this.base(`job-applications/${applicationId}/accept`), payload)
      .pipe(map((r) => unwrap<HiringDecisionReadDto>(r)));
  }

  rejectApplication(applicationId: number, payload: HiringDecisionCreateDto): Observable<HiringDecisionReadDto> {
    return this.http
      .post<ApiResponse<HiringDecisionReadDto>>(this.base(`job-applications/${applicationId}/reject`), payload)
      .pipe(map((r) => unwrap<HiringDecisionReadDto>(r)));
  }

  convertApplicantToEmployee(
    applicationId: number,
    payload: ConvertApplicantToEmployeeDto,
  ): Observable<ConvertApplicantToEmployeeResultDto> {
    return this.http
      .post<ApiResponse<ConvertApplicantToEmployeeResultDto>>(
        this.base(`job-applications/${applicationId}/convert-to-employee`),
        payload,
      )
      .pipe(map((r) => unwrap<ConvertApplicantToEmployeeResultDto>(r)));
  }
}
