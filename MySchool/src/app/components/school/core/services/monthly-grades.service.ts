import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { GuardianMonthlyGradeRow, updateMonthlyGrades } from '../models/MonthlyGrade.model';
import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { environment } from '../../../../../environments/environment';
import { ApiResponse } from '../../../../core/models/response.model';

@Injectable({
  providedIn: 'root'
})
export class MonthlyGradesService {

  API = inject(BackendAspService);
  private readonly api=`${environment.baseUrl}/MonthlyGrades`;
  
  getAllMonthlyGrades(term: number, monthId: number, classId: number, subjectId: number, pageNumber: number, pageSize: number) {
    const body = {
      termId: term,
      monthId,
      classId,
      subjectId,
      pageNumber,
      pageSize
    };
    return this.API.http.post(`${this.api}/page`, body).pipe(
      map((res: any) => res.result)
    );
  }


  updateMonthlyGrades(monthlyGrades: updateMonthlyGrades[]) {
    return this.API.http.put(`${this.api}/UpdateMany`, monthlyGrades).pipe(
      map((res: any) => {
        return res.result;
      }
      )
    );
  }

  /** Guardian: aggregated monthly grades for all linked students. */
  getGuardianMy(yearId?: number, termId?: number, monthId?: number): Observable<ApiResponse<GuardianMonthlyGradeRow[]>> {
    const parts: string[] = [];
    if (yearId != null) {
      parts.push(`yearId=${yearId}`);
    }
    if (termId != null) {
      parts.push(`termId=${termId}`);
    }
    if (monthId != null) {
      parts.push(`monthId=${monthId}`);
    }
    const qs = parts.length > 0 ? `?${parts.join('&')}` : '';
    return this.API.getRequest<GuardianMonthlyGradeRow[]>(`MonthlyGrades/guardian/my${qs}`);
  }

}
