import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { MonthlyResult } from '../models/monthly-result.model';

@Injectable({
  providedIn: 'root'
})
export class MonthlyResultService {

  Api=inject(BackendAspService);
  getMonthlyGradesReport(): Observable<MonthlyResult[]> {
    return this.Api.getRequest("Report");
  }
}
