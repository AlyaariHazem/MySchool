import { inject, Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { map } from 'rxjs';

import { TermlyGrade } from '../models/term.model';
import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';

@Injectable({
  providedIn: 'root'
})
export class TermlyGradeService {

  private API = inject(BackendAspService);

  getTermlyGradesReport(termId: number, yearId: number, classId: number, subjectId: number, page: number, pageSize: number) {
    const params = new HttpParams()
      .set('pageNumber', page)
      .set('pageSize', pageSize);

    return this.API.http.get(
      `${this.API.baseUrl}/TermlyGrade/${termId}/${yearId}/${classId}/${subjectId}`,
      { params }
    ).pipe(
      map((res: any) => res.result)
    );
  }

  updateTermlyGrades(termlyGrades: TermlyGrade[]) {
    return this.API.http.put(`${this.API.baseUrl}/TermlyGrade`, termlyGrades).pipe(
      map((res: any) => {
        return res.result;
      })
    );
  }
}
