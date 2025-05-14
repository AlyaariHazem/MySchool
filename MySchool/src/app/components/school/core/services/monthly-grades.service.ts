import { inject, Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { map } from 'rxjs';

import { updateMonthlyGrades } from '../models/MonthlyGrade.model';
import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { environment } from '../../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class MonthlyGradesService {

  API = inject(BackendAspService);
  private readonly api=`${environment.baseUrl}/MonthlyGrades`;
  
  getAllMonthlyGrades(term: number, monthId: number, classId: number, subjectId: number, pageNumber: number, pageSize: number) {
    const params=new HttpParams()
    .set('pageNumber', pageNumber)
    .set('pageSize', pageSize);
    return this.API.http.get(`${this.api}/${term}/${monthId}/${classId}/${subjectId}`, { params: params }).pipe(
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

}
