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
   * POST api/TermlyGrade/page — filters and pagination in JSON body (active year resolved on server).
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

  /** PUT api/TermlyGrade — bulk update; do not send yearID (server keeps the row’s academic year). */
  updateTermlyGrades(termlyGrades: TermlyGrade[]) {
    const body = termlyGrades.map(({ yearID: _y, ...row }) => row);
    return this.API.http
      .put<ApiResponse<unknown>>(`${this.API.baseUrl}/TermlyGrade`, body)
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
