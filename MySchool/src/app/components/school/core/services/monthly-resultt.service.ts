import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { MonthlyResult } from '../models/monthly-result.model';
import { ApiResponse } from '../../../../core/models/response.model';

@Injectable({
  providedIn: 'root'
})
export class MonthlyResultService {

  private api = inject(BackendAspService);

  getMonthlyGradesReport(yearId: number,termId: number,monthId: number,classId: number,divisionId: number,studentId: number)
  :Observable<ApiResponse<MonthlyResult[]>> {
    return this.api.getRequest<MonthlyResult[]>(`Report/${yearId}/${termId}/${monthId}/${classId}/${divisionId}/${studentId}`);
  }
  
}
