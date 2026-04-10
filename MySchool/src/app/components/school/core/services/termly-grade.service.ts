import { inject, Injectable } from '@angular/core';
import { map } from 'rxjs';

import { ApiResponse } from '../../../../core/models/response.model';
import { Paginates } from '../models/Pagination.model';
import { TermlyGradeQueryPayload } from '../models/termly-grade-query.model';
import { TermlyGrade } from '../models/term.model';
import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';

@Injectable({
  providedIn: 'root'
})
export class TermlyGradeService {

  private API = inject(BackendAspService);

  /**
   * POST api/TermlyGrade/page — filters and pagination in JSON body (yearId ignored server-side; active year used).
   */
  getTermlyGradesReport(payload: TermlyGradeQueryPayload) {
    return this.API.http
      .post<ApiResponse<Paginates>>(`${this.API.baseUrl}/TermlyGrade/page`, payload)
      .pipe(
        map((res) => {
          if (!res.isSuccess || res.result == null) {
            const msg = res.errorMasseges?.[0] ?? 'Failed to load termly grades.';
            throw new Error(msg);
          }
          return res.result;
        })
      );
  }

  /** PUT api/TermlyGrade — bulk update; each row must include termlyGradeID, yearID, etc. */
  updateTermlyGrades(termlyGrades: TermlyGrade[]) {
    return this.API.http
      .put<ApiResponse<unknown>>(`${this.API.baseUrl}/TermlyGrade`, termlyGrades)
      .pipe(
        map((res) => {
          if (!res.isSuccess) {
            const msg = res.errorMasseges?.[0] ?? 'Failed to save termly grades.';
            throw new Error(msg);
          }
          return res.result;
        })
      );
  }
}
