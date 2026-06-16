import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { MonthlyResult } from '../models/monthly-result.model';
import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { ApiResponse } from '../../../../core/models/response.model';

@Injectable({
  providedIn: 'root'
})
export class MonthlyResultService {

  private api = inject(BackendAspService);

  /** Uses active school year on the server; do not send yearId. */
  getMonthlyGradesReport(termId: number, monthId: number, classId: number, divisionId: number, studentId: number)
  : Observable<ApiResponse<MonthlyResult[]>> {
    return this.api.postRequest<MonthlyResult[]>('Report/monthly', {
      termId,
      monthId,
      classId,
      divisionId,
      studentId
    });
  }
  
}
