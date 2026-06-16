import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { ApiResponse } from '../../../../core/models/response.model';
import { MonthDto } from '../models/month.model';

@Injectable({
  providedIn: 'root'
})
export class MonthService {
  private readonly api = inject(BackendAspService);

  /** All months for the tenant (ordered by term then month on the server). */
  getAllMonths(): Observable<ApiResponse<MonthDto[]>> {
    return this.api.getRequest<MonthDto[]>('Month');
  }
}
